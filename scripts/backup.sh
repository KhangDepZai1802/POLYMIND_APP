#!/usr/bin/env bash
#
# Backup POLYMIND trên VPS Linux: dump PostgreSQL + dữ liệu MinIO, nén lại, đẩy ra
# kho ngoài VPS (Backblaze B2 qua rclone). Bản đối ứng Linux của scripts/backup.ps1.
#
# Cách dùng:
#   ./scripts/backup.sh                      # backup + đẩy lên remote nếu đã cấu hình rclone
#   BACKUP_DIR=/mnt/backup ./scripts/backup.sh
#
# Cron hằng đêm 2h sáng (crontab -e), ghi log để soi khi có sự cố:
#   0 2 * * * cd /home/polymind/polymind && ./scripts/backup.sh >> /var/log/polymind-backup.log 2>&1
#
# Chuẩn bị rclone MỘT LẦN (xem docs/06-deploy-oracle.md):
#   sudo -v ; curl https://rclone.org/install.sh | sudo bash
#   rclone config      # tạo remote tên "b2" kiểu Backblaze B2, nhập keyID + applicationKey
#
set -euo pipefail

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.production.yml}"
ENV_FILE="${ENV_FILE:-.env.production}"
BACKUP_DIR="${BACKUP_DIR:-db-backups}"
# Đích trên kho ngoài. Để rỗng = chỉ backup tại chỗ (KHÔNG khuyến nghị: mất VPS là mất luôn backup).
RCLONE_REMOTE="${RCLONE_REMOTE:-b2:polymind-backup}"
# Giữ bản local bao nhiêu ngày (bản trên remote giữ lâu hơn, xem RETENTION_REMOTE_DAYS).
RETENTION_LOCAL_DAYS="${RETENTION_LOCAL_DAYS:-7}"
RETENTION_REMOTE_DAYS="${RETENTION_REMOTE_DAYS:-30}"

timestamp="$(date +%Y%m%d-%H%M%S)"
target="${BACKUP_DIR}/${timestamp}"
mkdir -p "$target"

compose() {
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" "$@"
}

echo "==> [1/4] Dump PostgreSQL → ${target}/polymind-db.sql.gz"
# pg_dump ghi ra stdout rồi nén ngay, không tạo file tạm trong container.
compose exec -T postgres sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' | gzip > "${target}/polymind-db.sql.gz"

# File rỗng = dump hỏng (DB chưa sẵn sàng, sai mật khẩu...). Dừng ngay thay vì đẩy rác lên kho backup.
if [ ! -s "${target}/polymind-db.sql.gz" ]; then
    echo "LỖI: dump PostgreSQL rỗng — huỷ backup." >&2
    rm -rf "$target"
    exit 1
fi

echo "==> [2/4] Backup dữ liệu MinIO (ảnh CCCD, CV, giấy tờ) → ${target}/polymind-minio.tar.gz"
remote_minio="/tmp/polymind-minio-${timestamp}.tar.gz"
compose exec -T minio sh -c "tar -czf ${remote_minio} -C /data ."
compose cp "minio:${remote_minio}" "${target}/polymind-minio.tar.gz"
compose exec -T minio sh -c "rm -f ${remote_minio}"

size="$(du -sh "$target" | cut -f1)"
echo "==> Backup tại chỗ xong: ${target} (${size})"

echo "==> [3/4] Đẩy ra kho ngoài VPS"
if [ -z "$RCLONE_REMOTE" ]; then
    echo "    BỎ QUA: chưa đặt RCLONE_REMOTE — backup CHỈ nằm trên VPS này."
elif ! command -v rclone >/dev/null 2>&1; then
    echo "    BỎ QUA: chưa cài rclone — backup CHỈ nằm trên VPS này." >&2
else
    rclone copy "$target" "${RCLONE_REMOTE}/${timestamp}" --progress
    echo "    Đã đẩy lên ${RCLONE_REMOTE}/${timestamp}"
fi

echo "==> [4/4] Dọn bản cũ (local > ${RETENTION_LOCAL_DAYS} ngày, remote > ${RETENTION_REMOTE_DAYS} ngày)"
find "$BACKUP_DIR" -mindepth 1 -maxdepth 1 -type d -mtime "+${RETENTION_LOCAL_DAYS}" -exec rm -rf {} +
if [ -n "$RCLONE_REMOTE" ] && command -v rclone >/dev/null 2>&1; then
    rclone delete "$RCLONE_REMOTE" --min-age "${RETENTION_REMOTE_DAYS}d"
    rclone rmdirs "$RCLONE_REMOTE" --leave-root
fi

echo "==> HOÀN TẤT: ${target}"
echo "    NHẮC: backup chưa test restore = KHÔNG CÓ backup. Chạy thử scripts/restore.sh vào DB tạm mỗi quý."
