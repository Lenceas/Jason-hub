FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制 Project 文件并恢复依赖
COPY Monitor/Shared/MonitorShared.csproj Monitor/Shared/
COPY Monitor/api/MonitorApi.csproj Monitor/api/
RUN dotnet restore Monitor/api/MonitorApi.csproj

# 复制源码（保持目录结构）
COPY Monitor/Shared/ Monitor/Shared/
COPY Monitor/api/ Monitor/api/

WORKDIR /src/Monitor/api
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8051

ENTRYPOINT ["dotnet", "MonitorApi.dll"]
