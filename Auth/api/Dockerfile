FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制 Project 文件并恢复依赖
COPY Auth/Shared/AuthShared.csproj Auth/Shared/
COPY Auth/api/AuthApi.csproj Auth/api/
RUN dotnet restore Auth/api/AuthApi.csproj

# 复制源码（保持目录结构）
COPY Auth/Shared/ Auth/Shared/
COPY Auth/api/ Auth/api/

WORKDIR /src/Auth/api
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080

# 密钥存储卷
VOLUME /app/keys

ENTRYPOINT ["dotnet", "AuthApi.dll"]
