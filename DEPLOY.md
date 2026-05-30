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
| `api-auth.lujiesheng.cn` | `api-auth.lujiesheng.cn.pem` / `.key` | ECC |
| `api-notification.lujiesheng.cn` | `api-notification.lujiesheng.cn.pem` / `.key` | ECC |

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
    server_name monitor.lujiesheng.cn api-monitor.lujiesheng.cn;
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
    ports: ["127.0.0.1:8100:8080"]
    environment:
      - ConnectionStrings__Default=${AUTH_DB_CONNECTION}
    depends_on:
      mysql: { condition: service_healthy }
    restart: unless-stopped

  # ---- Monitor 监控面板（待开发，当前 docker-compose.yml 中注释状态） ----
  monitor-web:
    build: ./Monitor/web
    ports: ["127.0.0.1:8001:80"]
    restart: unless-stopped

  monitor-api:
    build: ./Monitor/api
    ports: ["127.0.0.1:8051:8080"]
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
```

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

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 1

      - name: 打包代码
        run: tar czf deploy.tar.gz $(git ls-files)

      - name: 部署到服务器
        run: |
          sudo apt-get install -qq -y sshpass
          sshpass -p "${{ secrets.SERVER_PASSWORD }}" scp -o StrictHostKeyChecking=no deploy.tar.gz ${{ secrets.SERVER_USER }}@${{ secrets.SERVER_HOST }}:/tmp/deploy.tar.gz
          sshpass -p "${{ secrets.SERVER_PASSWORD }}" ssh -o StrictHostKeyChecking=no ${{ secrets.SERVER_USER }}@${{ secrets.SERVER_HOST }} '
            rm -rf /opt/lujiesheng/Portfolio
            tar xzf /tmp/deploy.tar.gz -C /opt/lujiesheng
            rm /tmp/deploy.tar.gz
            cd /opt/lujiesheng
            docker compose up --build -d portfolio
            docker image prune -f
          '
```

GitHub Secrets 配置：

| Secret | 说明 |
|--------|------|
| `SERVER_HOST` | 服务器 IP |
| `SERVER_USER` | SSH 用户名 |
| `SERVER_PASSWORD` | SSH 密码 |

> 新增子项目时，deploy.yml 中 `docker compose up` 需追加新 service 名称。

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
         → tar 打包代码（仅 git 追踪文件）
         → scp 上传到服务器 /tmp
         → SSH 解压覆盖 /opt/lujiesheng
         → docker compose up --build -d （构建+启动新容器）
         → docker image prune -f （清理旧镜像）
```

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
- [ ] GitHub Actions `deploy.yml` 增加新 service 构建命令
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
