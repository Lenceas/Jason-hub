# DEPLOY.md — 部署规范

> Jason-hub 部署架构、CI/CD 流程、域名与反向代理规范。

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
    portfolio:8000  frontend:8001  api:8050 ...
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

---

## 域名映射

| 类型 | 域名 | 代理目标 | 说明 |
|------|------|----------|------|
| 主站 | `lujiesheng.cn` | `127.0.0.1:8000` | Portfolio 主站 |
| 子项目前端 | `todo.lujiesheng.cn` | `127.0.0.1:8001` | 子项目 SPA 页面 |
| 子项目 API | `api-todo.lujiesheng.cn` | `127.0.0.1:8050` | 子项目后端 API |

SSL 证书使用 Let's Encrypt（certbot）或腾讯云免费证书，建议申请**通配符证书** `*.lujiesheng.cn` 一次覆盖所有二级域名。

---

## Nginx 反向代理（主机级）

主机 Nginx 负责 SSL termination 和路由分发，不运行在 Docker 内。

```nginx
# /etc/nginx/conf.d/lujiesheng.cn.conf
server {
    listen 443 ssl http2;
    server_name lujiesheng.cn;

    ssl_certificate     /etc/ssl/certs/lujiesheng.cn/fullchain.pem;
    ssl_certificate_key /etc/ssl/certs/lujiesheng.cn/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:8000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}

server {
    listen 443 ssl http2;
    server_name todo.lujiesheng.cn;

    ssl_certificate     /etc/ssl/certs/lujiesheng.cn/fullchain.pem;
    ssl_certificate_key /etc/ssl/certs/lujiesheng.cn/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:8001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

server {
    listen 443 ssl http2;
    server_name api-todo.lujiesheng.cn;

    ssl_certificate     /etc/ssl/certs/lujiesheng.cn/fullchain.pem;
    ssl_certificate_key /etc/ssl/certs/lujiesheng.cn/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:8050;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

HTTP 强制跳转 HTTPS：

```nginx
server {
    listen 80;
    server_name lujiesheng.cn todo.lujiesheng.cn api-todo.lujiesheng.cn;
    return 301 https://$host$request_uri;
}
```

---

## 各项目容器化

### Astro / Vue 等前端项目

使用**多阶段构建**，最终产物扔进 `nginx:alpine`。

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

附带的 `nginx.conf` 仅做静态文件服务，不需反代逻辑：

```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;
    location / { try_files $uri $uri/ =404; }
}
```

### 后端 API 项目

直接编译运行，镜像中不包含 Nginx：

```dockerfile
# project-todo/api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8050
ENTRYPOINT ["dotnet", "TodoApi.dll"]
```

---

## Docker Compose

```yaml
# docker-compose.yml
services:
  portfolio:
    build: ./Portfolio
    ports:
      - "8000:80"
    restart: unless-stopped

  todo-web:
    build: ./project-todo/web
    ports:
      - "8001:80"
    restart: unless-stopped

  todo-api:
    build: ./project-todo/api
    ports:
      - "8050:8050"
    environment:
      - ConnectionStrings__Default=${TODO_DB_CONNECTION}
    depends_on:
      todo-db:
        condition: service_healthy
    restart: unless-stopped

  todo-db:
    image: mysql:8
    volumes:
      - todo-data:/var/lib/mysql
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PW}
      MYSQL_DATABASE: todo
    healthcheck:
      test: ["CMD", "mysqladmin", "ping"]
    restart: unless-stopped

volumes:
  todo-data:
```

**.env 文件**（不提交到 git）：

```
MYSQL_ROOT_PW=your_secure_password
TODO_DB_CONNECTION=Server=todo-db;Port=3306;Database=todo;User=root;Password=your_secure_password;
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
| `SERVER_HOST` | 服务器 IP 或域名 |
| `SERVER_USER` | SSH 用户名 |
| `SERVER_PASSWORD` | SSH 密码 |

> **注意**：服务器在国内无法直连 GitHub，故采用 Actions runner 打包代码 → SCP 上传 → Docker 重建的方案，不使用服务器 `git pull`。

### 服务器目录结构

```
/opt/lujiesheng/
├── docker-compose.yml
├── .env                    # 环境变量（不纳入 git）
├── Portfolio/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── src/...
└── project-todo/
    ├── web/
    │   ├── Dockerfile
    │   └── ...
    └── api/
        ├── Dockerfile
        └── ...
```

### 部署流程

```
git push → GitHub Actions 触发
         → tar 打包代码（仅 git 追踪文件）
         → scp 上传到服务器 /tmp
         → SSH 解压覆盖 /opt/lujiesheng
         → docker compose up --build -d （构建+启动新容器）
         → docker image prune -f （清理旧镜像）
```

---

## 端口规则

参考 [AGENTS.md](./AGENTS.md) 端口分配表，新增子项目时：

| 类型 | 范围 | 分配规则 |
|------|------|----------|
| 前端容器 | 8000–8049 | 按项目依次递增，与宿主机端口一一映射 |
| 后端容器 | 8050–8099 | 与前端对应，API 端口 = 前端端口 + 50 |
| 数据库 | 3306+ | 仅容器内互联，不暴露到宿主机 |

示例：

| 项目 | 前端端口 | API 端口 | 数据库 |
|------|---------|----------|--------|
| Portfolio | 8000 | — | — |
| Todo App | 8001 | 8050 | 容器内 3306 |
| 项目 2 | 8002 | 8051 | 容器内 3307 |

---

## SSL 证书续期

Let's Encrypt 自动续期（certbot）：

```bash
# 安装 certbot
apt install certbot python3-certbot-nginx

# 申请通配符证书
certbot certonly --manual -d lujiesheng.cn -d *.lujiesheng.cn

# 自动续期（cron）
echo "0 3 * * * certbot renew --quiet && systemctl reload nginx" | crontab -
```

腾讯云免费证书需在控制台手动下载上传，或用腾讯云 API 自动续期。

---

## 快速入门（首次部署）

```bash
# 1. 服务器环境准备
apt install docker docker-compose nginx

# 2. 拉取项目
git clone https://github.com/Lenceas/Jason-hub.git /opt/lujiesheng

# 3. 配置环境变量
cp .env.example .env
vim .env

# 4. 启动所有服务
docker-compose up -d

# 5. 配置 Nginx 反向代理 + SSL
vim /etc/nginx/conf.d/lujiesheng.cn.conf
systemctl reload nginx

# 6. GitHub Actions 自动部署后续更新
```

---

## 分支与版本

| 分支 | CI/CD 行为 |
|------|-----------|
| `main` | 自动触发部署 |
| `project/*` / `feat/*` / `fix/*` | 仅提交，不触发自动部署 |

新增子项目时，在 `docker-compose.yml` 追加 service 即可，不影响已有服务。
