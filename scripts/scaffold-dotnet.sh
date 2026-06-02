#!/bin/bash
# ============================================================
# scaffold-dotnet.sh — 创建新的 .NET 后端子项目
# 基于 templates/dotnet-service/ 模板生成标准结构
# ============================================================
# 用法: bash scripts/scaffold-dotnet.sh <项目名> [端口号]
# 示例: bash scripts/scaffold-dotnet.sh Monitor 8051
# ============================================================

set -euo pipefail

NAME="$1"
PORT="${2:-8050}"
NAME_LOWER=$(echo "$NAME" | tr '[:upper:]' '[:lower:]')

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TEMPLATE="$ROOT/templates/dotnet-service"
TARGET="$ROOT/$NAME"

if [ -z "$NAME" ]; then
  echo "用法: bash scripts/scaffold-dotnet.sh <项目名> [端口号]"
  echo "示例: bash scripts/scaffold-dotnet.sh Monitor 8051"
  exit 1
fi

if [ -d "$TARGET" ]; then
  echo "❌ 目录已存在: $TARGET"
  exit 1
fi

echo "=== 创建 $NAME 后端项目 ==="

# 1. 复制模板
cp -r "$TEMPLATE" "$TARGET"
echo "✅ 模板复制完成"

# 2. 按项目名重命名文件（含 __ProjectName__ 占位符的文件名）
find "$TARGET" -depth -name "*__ProjectName__*" | while read f; do
  dir=$(dirname "$f")
  base=$(basename "$f")
  newbase="${base//__ProjectName__/$NAME}"
  mv "$f" "$dir/$newbase"
done
echo "✅ 文件名替换完成"

# 3. 替换文件内容中的占位符
find "$TARGET" -type f ! -path "*/bin/*" ! -path "*/obj/*" | while read f; do
  sed -i "s/__ProjectName__/$NAME/g" "$f"
  sed -i "s/__project-name__/$NAME_LOWER/g" "$f"
done
echo "✅ 内容占位符替换完成"

# 4. 端口替换（Program.cs 和 launchSettings）
find "$TARGET" -type f -name "*.json" -o -name "*.cs" | while read f; do
  sed -i "s/__PORT__/$PORT/g" "$f" 2>/dev/null || true
done

echo ""
echo "🎉 $NAME 后端项目创建完成!"
echo ""
echo "目录结构:"
find "$TARGET" -type f ! -path "*/bin/*" ! -path "*/obj/*" | sed "s|$TARGET/||" | sort
echo ""
echo "下一步:"
echo "  1. 在 Program.cs 中注册服务"
echo "  2. 在 Endpoints/${NAME}Endpoints.cs 中添加业务端点"
echo "  3. 在 Services/${NAME}Service.cs 中实现业务逻辑"
