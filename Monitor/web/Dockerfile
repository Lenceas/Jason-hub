FROM node:22 AS build
WORKDIR /app
COPY Monitor/web/package*.json ./
RUN npm ci
COPY Monitor/web/ ./
RUN npx vite build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY Monitor/web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
