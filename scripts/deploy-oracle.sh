#!/usr/bin/env bash
#
# Validate và deploy POLYMIND trên Oracle VM.
#   bash scripts/deploy-oracle.sh
#
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="${ENV_FILE:-${ROOT_DIR}/.env.production}"
COMPOSE_FILE="${COMPOSE_FILE:-${ROOT_DIR}/docker-compose.production.yml}"

cd "$ROOT_DIR"

command -v docker >/dev/null 2>&1 || { echo "Chưa có Docker. Chạy scripts/oracle-bootstrap.sh trước." >&2; exit 1; }
[ -s "$ENV_FILE" ] || { echo "Thiếu ${ENV_FILE}. Chạy scripts/init-production-env.sh trước." >&2; exit 1; }

required_keys=(
    SUPERADMIN_EMAIL SUPERADMIN_PASSWORD DOMAIN ACME_EMAIL
    POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD
    MINIO_ROOT_USER MINIO_ROOT_PASSWORD MINIO_BUCKET JWT_KEY
)

failed=0
for key in "${required_keys[@]}"; do
    value="$(sed -n "s/^${key}=//p" "$ENV_FILE" | tail -n 1)"
    if [ -z "$value" ] || [[ "$value" == *CHANGE_ME* ]]; then
        echo "Thiếu hoặc chưa đổi giá trị: ${key}" >&2
        failed=1
    fi
done
[ "$failed" -eq 0 ] || exit 1

domain="$(sed -n 's/^DOMAIN=//p' "$ENV_FILE" | tail -n 1)"

echo "==> [1/5] Kiểm tra Docker Compose"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" --profile caddy config -q

echo "==> [2/5] Kéo image ARM64"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" --profile caddy pull postgres minio caddy

echo "==> [3/5] Build web .NET"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" --profile caddy build web

echo "==> [4/5] Khởi động production stack"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" --profile caddy up -d

echo "==> [5/5] Trạng thái"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" --profile caddy ps

echo
echo "Deploy đã được khởi động."
echo "Theo dõi log: docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy logs -f --tail=100"
echo "Health URL: https://${domain}/health"
