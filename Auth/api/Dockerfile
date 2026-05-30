FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制 Shared 库
COPY Auth/Shared/AuthShared.csproj Auth/Shared/
COPY Auth/api/AuthApi.csproj Auth/api/
RUN dotnet restore Auth/api/AuthApi.csproj

# 复制全部源码
COPY Auth/ .
WORKDIR /src/Auth/api
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080

# 密钥存储卷
VOLUME /app/keys

ENTRYPOINT ["dotnet", "AuthApi.dll"]
