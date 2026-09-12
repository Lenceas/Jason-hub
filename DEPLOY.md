# DEPLOY.md — 部署规范

> Jason-hub 部署架构、CI/CD 流程、域名、SSL 与反向代理规范。

---

## 整体架构

```
                   公网
                     │
              ┌──────┴──────┐
              │  Nginx (主机) │  ← 监听 80/443，SSL termination
              │  反向代理     │
              └──────┬──────┘
                     │
         ┌───────────┼───────────┐
         │           │           │
    portfolio:8000  Monitor:8001  api-Monitor:8051 ...
         │           │           │
   ┌─────┴┐    ┌────┴────┐  ┌───┴────┐
   │ Astro │    │ Vue 3  │  │ .NET   │  ← Docker 容器
   │ static│    │ SPA    │  │ API    │
   └───────┘    └────────┘  └───┬────┘
                                │
                           ┌────┴────┐
                           │ MySQL   │  ← 容器内数据库
                           └─────────┘
```

> 所有 Docker 容器端口仅绑定 `127.0.0.1`，不对外暴露。外网流量由主机 Nginx 统一处理 SSL + 反代。

---

## 域名映射

| 类型 | 域名 | 代理目标 | 说明 |
|------|------|----------|------|
| 主站 | `lujiesheng.cn` `www.lujiesheng.cn` | `127.0.0.1:8000` | Portfolio 主站 |
| 子项目前端 | `<name>.lujiesheng.cn` | `127.0.0.1:<80xx>` | 子项目 SPA 页面 |
| 子项目 API | `api-<name>.lujiesheng.cn` | `127.0.0.1:<80xx+50>` | 子项目后端 API |

**DNS 解析**：所有域名 A 记录指向 `81.71.136.3`。

---

## SSL 证书（acme.sh + Let's Encrypt）

使用 **acme.sh** + 腾讯云 DNS API 自动申请和续期，走 DNS-01 验证（不需要开放额外端口）。

**策略**：一个域名一张免费证书，每个子项目独立管理。

### 当前证书

| 域名 | 证书文件 | 类型 |
|------|---------|------|
| `lujiesheng.cn` + `www.lujiesheng.cn` | `lujiesheng.cn.pem` / `.key` | 双域名 ECC |
| `monitor.lujiesheng.cn` + `api-monitor.lujiesheng.cn` | `monitor.lujiesheng.cn.pem` / `.key` | 双域名 ECC（`api-monitor.*` 为指向该文件的**软链**） |
| `api-auth.lujiesheng.cn` | `api-auth.lujiesheng.cn.pem` / `.key` | ECC |
| `api-notification.lujiesheng.cn` | `api-notification.lujiesheng.cn.pem` / `.key` | ECC（服务未上线） |

### 主站申请流程

```bash
~/.acme.sh/acme.sh --issue --dns dns_tencent -d lujiesheng.cn -d www.lujiesheng.cn
~/.acme.sh/acme.sh --install-cert -d lujiesheng.cn \
  --key-file       ~/.acme.sh/ssl/lujiesheng.cn.key \
  --fullchain-file ~/.acme.sh/ssl/lujiesheng.cn.pem \
  --reloadcmd      "sudo cp ~/.acme.sh/ssl/lujiesheng.cn.key /etc/nginx/ssl/lujiesheng.cn.key && sudo cp ~/.acme.sh/ssl/lujiesheng.cn.pem /etc/nginx/ssl/lujiesheng.cn.pem && sudo chmod 600 /etc/nginx/ssl/lujiesheng.cn.key && sudo chmod 644 /etc/nginx/ssl/lujiesheng.cn.pem && sudo systemctl reload nginx"
```

### 新增子项目 SSL 流程

子项目命名规则：`<name>.lujiesheng.cn` → 证书前缀 `<name>`

```bash
# 以 Monitor 子项目为例
~/.acme.sh/acme.sh --issue --dns dns_tencent -d monitor.lujiesheng.cn
~/.acme.sh/acme.sh --install-cert -d monitor.lujiesheng.cn \
  --key-file       ~/.acme.sh/ssl/monitor.lujiesheng.cn.key \
  --fullchain-file ~/.acme.sh/ssl/monitor.lujiesheng.cn.pem \
  --reloadcmd      "sudo cp ~/.acme.sh/ssl/monitor.lujiesheng.cn.key /etc/nginx/ssl/monitor.lujiesheng.cn.key && sudo cp ~/.acme.sh/ssl/monitor.lujiesheng.cn.pem /etc/nginx/ssl/monitor.lujiesheng.cn.pem && sudo chmod 600 /etc/nginx/ssl/monitor.lujiesheng.cn.key && sudo chmod 644 /etc/nginx/ssl/monitor.lujiesheng.cn.pem && sudo systemctl reload nginx"
```

### 自动续期

- acme.sh 每天凌晨 3 点通过 cron 检查证书有效期
- 剩余 ≤ 7 天时自动续期（`Le_RenewalDays=7`）
- 续期后自动拷贝到 `/etc/nginx/ssl/` 并 `systemctl reload nginx`
- 腾讯云 DNS API 凭证保存在 `~/.acme.sh/account.conf`（仅服务器本地）

### ⚠️ `--install-cert` 是必需的，只跑 `--issue` 会导致证书静默过期

> **这是 2026-09-12 实际发生过的生产故障，务必理解。**

`--issue` 只把证书签发进 acme.sh 的仓库（`~/.acme.sh/<domain>_ecc/`），**不会**放进 nginx 使用的位置，**也不会**登记"续期后自动安装"这个动作。**只有执行过 `--install-cert` 的域名**，acme.sh 才会在每次续期后自动拷贝证书 + 重载 nginx。

故障当时的状态：

| 域名 | 是否跑过 `--install-cert` | 结果 |
|------|--------------------------|------|
| `lujiesheng.cn` | ✅ 跑过 | 续期后自动安装，正常 |
| `monitor.lujiesheng.cn` | ❌ 只跑了 `--issue` | acme.sh 显示已续期（Created 9/11），但 `/etc/nginx/ssl/` 里仍是 6/05 的旧证书 → **过期 9 天** |
| `api-auth.lujiesheng.cn` | ❌ 只跑了 `--issue` | 同上 → **过期 14 天** |

危险之处在于**故障完全静默**：`acme.sh --list` 的 `Renew` 时间正常、cron 每天无报错、`nginx -t` 也通过，只有真的访问站点才会看到证书警告。而 Portfolio 首页的项目卡片正指向这些子域名。

### 证书健康检查（建议每月执行一次）

```bash
# 1. acme.sh 侧：证书是否在正常续期
~/.acme.sh/acme.sh --list

# 2. nginx 侧：实际部署的证书有效期
for f in /etc/nginx/ssl/*.pem; do
  case "$f" in *.pre-*) continue;; esac
  echo "--- $(basename $f) ---"; sudo openssl x509 -in "$f" -noout -dates
done

# 3. 端到端：证书链是否有效（0 = ok，10 = 已过期）
for d in lujiesheng.cn monitor.lujiesheng.cn api-monitor.lujiesheng.cn api-auth.lujiesheng.cn; do
  echo "$d -> $(echo | openssl s_client -connect 127.0.0.1:443 -servername $d 2>/dev/null | grep '^Verify return code' | head -1)"
done

# 4. 确认续期后会自动安装（应列出各域名的 conf）
sudo grep -l "Le_ReloadCmd" ~/.acme.sh/*_ecc/*.conf
```

> **排查要点**：若第 1 步显示已续期、但第 2 步的文件日期很旧，就是"只 issue 未 install"问题——对缺失的域名补跑一次 `--install-cert`（命令见上节），即可同时修复当前证书**并**登记后续自动续期。
> 证书为 **ECC**，配置存放在 `~/.acme.sh/<domain>_ecc/<domain>.conf`（**不是** `<domain>/`）；`Le_ReloadCmd` 的值以 base64 存储，这是 acme.sh 的正常行为，不要误判为损坏。
> 子域名带 SAN 时（如 `monitor.lujiesheng.cn` 含 `api-monitor.lujiesheng.cn`），只需为**主证书**跑 `--install-cert`，另一条域名用软链指向同一组文件即可。

### 前置依赖

服务器需预先安装：

```bash
curl -sL https://gitee.com/neilpang/acme.sh/raw/master/acme.sh -o ~/.acme.sh/acme.sh
chmod +x ~/.acme.sh/acme.sh

# 腾讯云 DNS API hook
mkdir -p ~/.acme.sh/dnsapi
curl -sL https://gitee.com/neilpang/acme.sh/raw/master/dnsapi/dns_tencent.sh -o ~/.acme.sh/dnsapi/dns_tencent.sh

# 配置 API 密钥（子用户，仅 DNSPod 权限）
cat >> ~/.acme.sh/account.conf << "EOF"
export Tencent_SecretId="<SecretId>"
export Tencent_SecretKey="<SecretKey>"
Le_RenewalDays=7
EOF
```

---

## Nginx 反向代理（主机级）

主机 Nginx 负责 SSL termination 和路由分发，不运行在 Docker 内。
每个域名使用各自独立的 SSL 证书文件。

```nginx
# /etc/nginx/sites-available/lujiesheng.cn

# HTTP → HTTPS 强制跳转
server {
    listen 80;
    server_name lujiesheng.cn www.lujiesheng.cn;
    return 301 https://$host$request_uri;
}

# 主站
server {
    listen 443 ssl;
    server_name lujiesheng.cn www.lujiesheng.cn;

    ssl_certificate     /etc/nginx/ssl/lujiesheng.cn.pem;
    ssl_certificate_key /etc/nginx/ssl/lujiesheng.cn.key;
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_ciphers         HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://127.0.0.1:8000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# HTTP 跳转（子域名）
server {
    listen 80;
    server_name monitor.lujiesheng.cn api-monitor.lujiesheng.cn api-auth.lujiesheng.cn;
    return 301 https://$host$request_uri;
}

# 子项目示例：Monitor 前端
server {
    listen 443 ssl;
    server_name monitor.lujiesheng.cn;

    ssl_certificate     /etc/nginx/ssl/monitor.lujiesheng.cn.pem;
    ssl_certificate_key /etc/nginx/ssl/monitor.lujiesheng.cn.key;
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_ciphers         HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://127.0.0.1:8001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# 基础设施服务示例：Auth 鉴权
server {
    listen 443 ssl;
    server_name api-auth.lujiesheng.cn;

    ssl_certificate     /etc/nginx/ssl/api-auth.lujiesheng.cn.pem;
    ssl_certificate_key /etc/nginx/ssl/api-auth.lujiesheng.cn.key;
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_ciphers         HIGH:!aNULL:!MD5;

    # /healthz 仅限内网，公网拦截
    location = /healthz {
        return 404;
    }

    location / {
        proxy_pass http://127.0.0.1:8100;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 各项目容器化

### Astro / Vue 等前端项目

使用**多阶段构建**，最终产物由 `nginx:alpine` 提供服务。

```dockerfile
# Portfolio/Dockerfile
FROM node:22 AS build
WORKDIR /app
COPY . .
RUN npm ci && npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

容器内 nginx.conf — 仅做静态文件服务：

```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;
    location / { try_files $uri $uri/ =404; }
}
```

### 后端 API 项目

直接编译运行，不包含 Nginx：

```dockerfile
# Monitor/api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8050
ENTRYPOINT ["dotnet", "NoteApi.dll"]
```

---

## Docker Compose

分为**基础设施层**（全局共享）和**应用层**（按子项目分组）。所有端口仅绑定 `127.0.0.1`。

### 基础设施层

MySQL / Redis / MongoDB 三数据库，所有子项目共用，一次启动。

```yaml
services:
  mysql:
    image: mysql:8.4
    ports: ["127.0.0.1:3306:3306"]
    volumes: [mysql-data:/var/lib/mysql]
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PW}
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
    restart: unless-stopped

  redis:
    image: redis:8-alpine
    ports: ["127.0.0.1:6379:6379"]
    volumes: [redis-data:/data]
    command: redis-server --requirepass ${REDIS_PW}
    healthcheck:
      test: ["CMD", "redis-cli", "--no-auth-warning", "-a", "${REDIS_PW}", "ping"]
    restart: unless-stopped

  mongo:
    image: mongo:8
    ports: ["127.0.0.1:27017:27017"]
    volumes: [mongo-data:/data/db]
    environment:
      MONGO_INITDB_ROOT_USERNAME: ${MONGO_ROOT_USER}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_ROOT_PW}
    healthcheck:
      test: echo 'db.runCommand("ping").ok' | mongosh --quiet
    restart: unless-stopped
```

### 应用层

每个子项目按需依赖基础设施。

```yaml
  # ---- Portfolio 主站 ----
  portfolio:
    build: ./Portfolio
    ports: ["127.0.0.1:8000:80"]
    restart: unless-stopped

  # ---- Auth 鉴权服务（基础设施） ----
  auth:
    build: ./Auth/api
    ports: ["127.0.0.1:8100:8100"]
    volumes:
      - auth-keys:/app/keys
      - ./ip2region:/app/ip2region
    environment:
      - ConnectionStrings__Default=${AUTH_DB_CONNECTION}
      - ip2region__DatabasePath=/app/ip2region/ip2region_v4.xdb
    depends_on:
      mysql: { condition: service_healthy }
    restart: unless-stopped

  # ---- Monitor 监控面板（开发中） ----
  monitor-web:
    build: ./Monitor/web
    ports: ["127.0.0.1:8001:80"]
    restart: unless-stopped

  monitor-api:
    build: ./Monitor/api
    ports: ["127.0.0.1:8051:8051"]
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock    # Agent 采集 Docker 容器状态需要
    environment:
      - ConnectionStrings__Default=${MONITOR_DB_CONNECTION}
      - Redis__ConnectionString=${MONITOR_REDIS_CONNECTION}
    depends_on:
      mysql: { condition: service_healthy }
      redis: { condition: service_healthy }
    restart: unless-stopped

volumes:
  mysql-data:
  redis-data:
  mongo-data:
  auth-keys:
```

> ip2region 目录（`./ip2region:/app/ip2region`）为 bind mount，由服务器 cron（每月 1 号凌晨 3:00）自动更新数据库文件。
> 服务器另设 cron（每月 1 号凌晨 3:00）自动检查更新。

**.env 文件**（不提交到 git）：

```
MYSQL_ROOT_PW=your_secure_password
MONGO_ROOT_USER=admin
MONGO_ROOT_PW=your_secure_password
MONITOR_DB_CONNECTION=Server=127.0.0.1;Port=3306;Database=jason_monitor;User=root;Password=<pw>;
MONITOR_REDIS_CONNECTION=127.0.0.1:6379
```

---

## CI/CD 流程

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy
on:
  push:
    branches: [main]
    paths-ignore:          # 纯文档变更不触发部署，见下方「触发条件」
      - '**.md'
      - '.dsh/**'
      - '.gitignore'
      - 'LICENSE'

concurrency:
  group: deploy
  cancel-in-progress: false   # 连续推送时排队串行，不互相取消

env:
  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true
  TCR_REGISTRY: ccr.ccs.tencentyun.com/jason-hub
  TCR_USERNAME: '100012562502'

jobs:
  # ---- 构建层：4 个镜像并行构建，互不阻塞 ----
  build:
    runs-on: ubuntu-latest
    timeout-minutes: 60
    name: build ${{ matrix.service }}
    strategy:
      fail-fast: false          # 某个镜像失败不取消其他镜像的构建
      matrix:
        include:
          - service: portfolio
            dockerfile: Portfolio/Dockerfile
          - service: auth
            dockerfile: Auth/api/Dockerfile
          - service: monitor-web
            dockerfile: Monitor/web/Dockerfile
          - service: monitor-api
            dockerfile: Monitor/api/Dockerfile
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 1

      - name: 配置 Buildx
        uses: docker/setup-buildx-action@v3

      - name: 登录腾讯云 TCR
        uses: docker/login-action@v3
        with:
          registry: ccr.ccs.tencentyun.com
          username: ${{ env.TCR_USERNAME }}
          password: ${{ secrets.TCR_PASSWORD }}

      - name: 构建并推送 ${{ matrix.service }}
        id: build
        uses: docker/build-push-action@v6
        with:
          context: .
          file: ${{ matrix.dockerfile }}
          push: true
          tags: ${{ env.TCR_REGISTRY }}/${{ matrix.service }}:latest
          # GHA 层缓存：基础镜像层、npm ci / dotnet restore 层跨次复用
          cache-from: type=gha,scope=${{ matrix.service }}
          cache-to: type=gha,mode=max,scope=${{ matrix.service }}

  # ---- 部署层：4 个镜像全部构建成功后才执行 ----
  deploy:
    needs: build
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 1

      # 专用 CI 部署密钥（ed25519），与开发者个人 ubuntu.pem 分离
      - name: 配置 SSH 私钥
        run: |
          mkdir -p ~/.ssh
          printf '%s\n' "${{ secrets.SERVER_SSH_KEY }}" > ~/.ssh/deploy_key
          chmod 600 ~/.ssh/deploy_key

      # 固定服务器主机公钥（公开信息），替代原先放弃校验的
      # StrictHostKeyChecking=no
      - name: 固定服务器主机公钥
        run: |
          cat >> ~/.ssh/known_hosts <<'KNOWN_HOSTS'
          81.71.136.3 ssh-ed25519 AAAA...（完整指纹见仓库 deploy.yml）
          KNOWN_HOSTS
          chmod 600 ~/.ssh/known_hosts

      - name: 部署到服务器
        run: |
          scp -i ~/.ssh/deploy_key docker-compose.yml ${{ secrets.SERVER_USER }}@${{ secrets.SERVER_HOST }}:/opt/lujiesheng/docker-compose.yml
          ssh -i ~/.ssh/deploy_key ${{ secrets.SERVER_USER }}@${{ secrets.SERVER_HOST }} '
            set -e
            cd /opt/lujiesheng
            echo "${{ secrets.TCR_PASSWORD }}" | docker login ccr.ccs.tencentyun.com -u ${{ env.TCR_USERNAME }} --password-stdin
            docker compose pull portfolio auth monitor-web monitor-api
            docker compose up -d portfolio auth monitor-web monitor-api
            docker image prune -f
          '
```

**为什么拆成两个 job：** 4 个 `docker build` 原先串行跑在同一个 job 里共用一个 40 分钟总超时，任一镜像构建变慢都会挤占后续镜像的时间，最终整条流水线被杀、"部署到服务器"根本没机会执行——生产环境静默停在旧镜像上。现在每个镜像独立 job + 独立超时（60 分钟），最慢的一个只拖累自己；`deploy` 通过 `needs: build` 汇聚，4 个镜像全部成功才部署，避免"3 个新镜像 + 1 个旧镜像"的半吊子状态上生产。

**GHA 层缓存（`type=gha`）：** 原先每次都是全新 runner 冷构建——重新拉 `mcr.microsoft.com/dotnet/sdk:10.0`（约 1.7GB）、重跑 NuGet restore。缓存后基础镜像层、`npm ci`、`dotnet restore` 跨次复用。`mode=max` 必须显式指定，默认的 `min` 只缓存最后一级，而耗时的 restore / publish / npm ci 全在中间的 build 阶段。

> ⚠️ GHA 缓存有 **10GB / 仓库**上限，超限按 LRU 淘汰。4 个 service 各自 `scope` 独立，两个 .NET 镜像的 SDK 层占大头，接近上限时最早的缓存会被挤掉、退回冷构建（只是变慢，不会失败）。

**构建上下文（`.dockerignore`）：** 仓库根目录的 `.dockerignore` 排除 `node_modules`（约 350MB）、`bin/obj`、`.git`、`ip2region/`（10.6MB，服务器 bind mount）、`.env*`、`**/*.pem|key`，构建上下文从约 460MB 降到约 10MB；同时避免宿主机 `node_modules` 覆盖容器内 `npm ci` 刚装好的依赖，并堵住密钥误入镜像的路径。

> 新增子项目时：在 `matrix.include` 追加一行 `service` + `dockerfile`，并在服务器端 `docker compose pull/up` 中追加新 service 名称。

GitHub Secrets 配置：

| Secret | 说明 |
|--------|------|
| `SERVER_HOST` | 服务器 IP，**必须为 `81.71.136.3`**（`known_hosts` 按 IP 录入，用域名会校验失败） |
| `SERVER_USER` | SSH 用户名（`ubuntu`） |
| `SERVER_SSH_KEY` | **专用 CI 部署私钥**（ed25519，OpenSSH 格式全文） |
| `TCR_PASSWORD` | 腾讯云 TCR 镜像仓库密码 |

> ⚠️ CI/CD 使用**专用部署密钥**，不再是开发者个人密钥：`actions/checkout` 走 GitHub 自动注入的 `GITHUB_TOKEN`，部署到服务器走 `SERVER_SSH_KEY`（ed25519，服务器侧该公钥带 `no-port-forwarding,no-agent-forwarding,no-X11-forwarding` 限制）。
> 已弃用 `sshpass -p` 密码认证——密码会出现在 runner 的**进程命令行**中（`ps aux` 可读），且 `apt-get install sshpass` 本身是脆弱依赖（v1.10.1 流水线 `#121` 的部署失败点即在它）。
> 主机校验采用**固定公钥**而非 `StrictHostKeyChecking=no`（后者等于放弃主机校验，中间人可截获部署流量与 TCR 凭据）。服务器重装后主机密钥变更，需更新 `deploy.yml` 中的 `known_hosts` 块；报 `Host key verification failed` 即此原因。

---

## 服务器实际信息

| 项目 | 值 |
|------|----|
| IP | `81.71.136.3` |
| 系统 | Ubuntu 24.04 LTS |
| 配置 | 2C4G / 70GB SSD / 6Mbps |
| 项目路径 | `/opt/lujiesheng/` |
| Docker Engine | 29.5.2 |
| Docker Compose | v5.1.4 |
| SSL | acme.sh + Let's Encrypt (自动续期) |

### 目录结构

```
/opt/lujiesheng/
├── docker-compose.yml
├── .env                         # 环境变量（不纳入 git）
├── Portfolio/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── src/...
├── Monitor/                ← 子项目示例
│   ├── web/
│   │   ├── Dockerfile
│   │   └── ...
│   └── api/
│       ├── Dockerfile
│       └── ...
└── ...
```

---

## 部署流程

```
git push → GitHub Actions 触发
         → 4 个镜像【并行】构建（矩阵 job）并推送 TCR
         → 4 个镜像全部成功后，scp 上传 docker-compose.yml
         → SSH: docker compose pull && up -d（仅应用层 4 服务）
         → docker image prune -f （清理旧镜像）
```

> 镜像构建在 CI 中完成（`docker build` + `docker push`），服务器端只拉取和启动，不再在服务器上构建。
> 构建阶段是 4 个并行 job（`fail-fast: false`，互不取消）；任一个失败则整条部署中止（`deploy` 依赖 `needs: build`），不会出现新旧镜像混跑。

### 触发条件（`paths-ignore`）

`deploy.yml` 的 `on.push` 配置了路径过滤，**纯文档变更不会触发部署**：

| 路径 | 是否触发部署 |
|------|-------------|
| `**.md`（含根文档、各子项目文档、`.dsh/skills/**/SKILL.md`） | ❌ 跳过 |
| `.dsh/**`、`.gitignore`、`LICENSE` | ❌ 跳过 |
| `Portfolio/**`、`Auth/**`、`Monitor/**`、`templates/**`、`scripts/**` | ✅ 触发 |
| `docker-compose.yml`、`.github/workflows/deploy.yml`、`.dockerignore` | ✅ 触发 |

> 语义是"本次推送的**全部**变更路径都命中忽略规则才跳过"。所以"改文档 + 改代码"混在同一个提交里时仍会正常部署，不会漏发布。
> 代价：`main` 分支不再是"任何推送都上线"，纯文档推送只进版本库、不动生产。

---

## 防火墙

腾讯云轻量服务器防火墙需放行以下端口：

| 端口 | 用途 |
|------|------|
| 22 | SSH |
| 80 | HTTP（Nginx） |
| 443 | HTTPS（Nginx SSL） |

> Docker 容器端口（8000-8099）无需对外开放。

---

## 端口规则

参考 [AGENTS.md](./AGENTS.md) 端口分配表：

| 类型 | 范围 | 分配规则 |
|------|------|----------|
| 前端容器 | 8000–8049 | 按项目依次递增，Docker 仅绑定 `127.0.0.1` |
| 子项目 API | 8050–8099 | API 端口 = 前端端口 + 50 |
| 基础设施服务 | 8100–8149 | Auth 鉴权 / 通知 / 任务调度 / 消息队列，依次递增 |
| 数据库 | 3306+ | 仅容器内互联，不暴露 |

示例：

| 项目 | 前端容器端口 | API 容器端口 | 域名 | 数据库 |
|------|-------------|-------------|------|--------|
| Portfolio | 8000 | — | `lujiesheng.cn` | — |
| Monitor | 8001 | 8051 | `monitor.lujiesheng.cn` + `api-monitor.lujiesheng.cn` | 容器内 3306 |
| Auth | — | 8100 | `api-auth.lujiesheng.cn` | 容器内 3306 |
| Notification | — | 8110 | `api-notification.lujiesheng.cn` | 容器内 3306 |
| 项目 2 | 8002 | 8052 | `<name>.lujiesheng.cn` + `api-<name>.lujiesheng.cn` | 容器内 3307 |

---

## 新增子项目部署检查清单

按以下顺序完成：

- [ ] 创建子项目（Vue 3 / .NET 等）
- [ ] 编写 `Dockerfile` + 容器内 `nginx.conf`（前端项目）
- [ ] `docker-compose.yml` 追加 service（端口只绑 `127.0.0.1`）
- [ ] GitHub Actions `deploy.yml` 的 `matrix.include` 追加一行 `service` + `dockerfile`
- [ ] DNS 添加 A 记录（`<name>.lujiesheng.cn` / `api-<name>.lujiesheng.cn` → `81.71.136.3`）
- [ ] **SSL**：使用 acme.sh 申请子域名证书（`<name>.lujiesheng.cn`），见上方 "新增子项目 SSL 流程"
- [ ] Nginx 添加子域名 `server` 块 → `nginx -t` → `systemctl reload nginx`
- [ ] Portfolio `projects.json` 添加项目卡片
- [ ] 更新各 MD 文档（CHANGELOG / README / AGENTS 端口表 / 本清单）

---

## 快速入门（首次部署）

```bash
# 1. 服务器环境准备
apt install nginx
# Docker 从官方源安装（非 apt 自带）
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker.gpg
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu noble stable" > /etc/apt/sources.list.d/docker.list
apt update && apt install docker-ce docker-compose-plugin

# 2. 拉取项目
git clone https://github.com/Lenceas/Jason-hub.git /opt/lujiesheng

# 3. 配置 SSL（acme.sh + 腾讯云 DNS API）
# 见上方 "SSL 证书" 章节

# 4. 配置 Nginx 反代 + SSL
vim /etc/nginx/sites-available/lujiesheng.cn
ln -s /etc/nginx/sites-available/lujiesheng.cn /etc/nginx/sites-enabled/
rm /etc/nginx/sites-enabled/default
nginx -t && systemctl reload nginx

# 5. 启动所有服务
cd /opt/lujiesheng
docker compose up -d

# 6. 配置 .env
cp .env.example .env && vim .env

# 7. GitHub Actions 自动部署后续更新
# 在 GitHub Secrets 中配置 SERVER_HOST / SERVER_USER / SERVER_PASSWORD
```

---

## 分支与版本

| 分支 | CI/CD 行为 |
|------|-----------|
| `main` | 自动触发部署 |
| `project/*` / `feat/*` / `fix/*` | 仅提交，不触发自动部署 |
