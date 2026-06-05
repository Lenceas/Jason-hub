FROM node:22 AS build
WORKDIR /app
COPY Portfolio/package.json Portfolio/package-lock.json ./
RUN npm ci
COPY Portfolio/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY Portfolio/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
