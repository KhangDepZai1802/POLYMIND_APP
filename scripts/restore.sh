#!/usr/bin/env bash
#
# Khôi phục POLYMIND từ một thư mục backup do scripts/backup.sh tạo ra.
# THAO TÁC PHÁ HỦY: xóa sạch schema public hiện tại và toàn bộ file trong MinIO rồi ghi đè.
#
# Cách dùng:
#   ./scripts/restore.sh db-backups/20260721-020000 --confirm
#
# Lấy bản backup từ kho ngoài về trước khi restore:
#   rclone copy b2:polymind-backup/20260721-020000 ./db-backups/20260721-020000 --progress
#
# TEST RESTORE ĐỊNH KỲ (khuyến nghị mỗi quý): tạo stack tạm với COMPOSE_PROJECT_NAME khác,
# restore vào đó rồi đếm số ứng viên — KHÔNG chạy thử trên stack production đang phục vụ người dùng.
#
set -euo pipefail

BACKUP_PATH="${1:-}"
CONFIRM="${2:-}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.production.yml}"
ENV_FILE="${ENV_FILE:-.env.production}"

if [ -z "$BACKUP_PATH" ]; then
    echo "Cách dùng: $0 <thư-mục-backup> --confirm" >&2
    exit 1
fi

if [ "$CONFIRM" != "--confirm" ]; then
    echo "Restore là thao tác PHÁ HỦY (xóa sạch DB + file hiện tại)." >&2
    echo "Kiểm tra kỹ '${BACKUP_PATH}' rồi chạy lại kèm --confirm." >&2
    exit 1
fi

db_file="${BACKUP_PATH}/polymind-db.sql.gz"
minio_file="${BACKUP_PATH}/polymind-minio.tar.gz"

[ -s "$db_file" ] || { echo "Thiếu/rỗng bản dump DB: $db_file" >&2; exit 1; }
[ -s "$minio_file" ] || { echo "Thiếu/rỗng bản backup MinIO: $minio_file" >&2; exit 1; }

compose() {
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" "$@"
}

echo "==> [1/3] Khôi phục PostgreSQL từ ${db_file}"
compose exec -T postgres sh -c 'psql -U "$POSTGRES_USER" "$POSTGRES_DB" -c "drop schema public cascade; create schema public;"'
gunzip -c "$db_file" | compose exec -T postgres sh -c 'psql -U "$POSTGRES_USER" "$POSTGRES_DB"'

echo "==> [2/3] Khôi phục dữ liệu MinIO từ ${minio_file}"
remote_minio="/tmp/polymind-minio-restore.tar.gz"
compose cp "$minio_file" "minio:${remote_minio}"
compose exec -T minio sh -c "rm -rf /data/* && tar -xzf ${remote_minio} -C /data && rm -f ${remote_minio}"

echo "==> [3/3] Khởi động lại web"
compose restart web

echo "==> HOÀN TẤT. Kiểm chứng ngay: mở /health và đếm số ứng viên trong app xem có khớp bản backup không."
