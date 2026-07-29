#!/usr/bin/env bash
#
# Tạo .env.production với secret ngẫu nhiên; không ghi secret vào Git.
# Chạy tại thư mục gốc dự án trên VM:
#   bash scripts/init-production-env.sh
#
set -euo pipefail

ENV_FILE="${ENV_FILE:-.env.production}"

if [ -e "$ENV_FILE" ]; then
    echo "DỪNG: ${ENV_FILE} đã tồn tại; không ghi đè secret production." >&2
    echo "Nếu muốn tạo lại, hãy tự đổi tên file cũ trước." >&2
    exit 1
fi

read -r -p "Domain (không gồm https://): " domain
read -r -p "Email nhận thông báo Let's Encrypt: " acme_email
read -r -p "Email super admin: " admin_email
read -r -p "Tên super admin [Super Admin]: " admin_name
admin_name="${admin_name:-Super Admin}"

if ! [[ "$domain" =~ ^[A-Za-z0-9.-]+\.[A-Za-z]{2,}$ ]]; then
    echo "Domain không hợp lệ: ${domain}" >&2
    exit 1
fi
if ! [[ "$acme_email" =~ ^[^[:space:]@]+@[^[:space:]@]+\.[^[:space:]@]+$ ]]; then
    echo "ACME email không hợp lệ." >&2
    exit 1
fi
if ! [[ "$admin_email" =~ ^[^[:space:]@]+@[^[:space:]@]+\.[^[:space:]@]+$ ]]; then
    echo "Super admin email không hợp lệ." >&2
    exit 1
fi

read -r -s -p "Mật khẩu super admin (tối thiểu 12 ký tự): " admin_password
echo
read -r -s -p "Nhập lại mật khẩu: " admin_password_confirm
echo
if [ "$admin_password" != "$admin_password_confirm" ]; then
    echo "Hai mật khẩu không khớp." >&2
    exit 1
fi
if [ "${#admin_password}" -lt 12 ]; then
    echo "Mật khẩu phải có ít nhất 12 ký tự." >&2
    exit 1
fi
if [[ "$admin_password" == *'$'* || "$admin_password" == *'#'* ]]; then
    echo "Để tránh Docker Compose diễn giải sai, mật khẩu không được chứa ký tự \$ hoặc #." >&2
    exit 1
fi

db_password="$(openssl rand -hex 24)"
minio_password="$(openssl rand -hex 24)"
jwt_key="$(openssl rand -hex 48)"

umask 077
cat > "$ENV_FILE" <<EOF
SUPERADMIN_EMAIL=${admin_email}
SUPERADMIN_PASSWORD=${admin_password}
SUPERADMIN_FULLNAME=${admin_name}

DOMAIN=${domain}
ACME_EMAIL=${acme_email}

DUCKDNS_SUBDOMAIN=
DUCKDNS_TOKEN=

POSTGRES_DB=polymind
POSTGRES_USER=polymind
POSTGRES_PASSWORD=${db_password}

MINIO_ROOT_USER=polymind-minio
MINIO_ROOT_PASSWORD=${minio_password}
MINIO_BUCKET=polymind-documents

JWT_KEY=${jwt_key}

SMTP_ENABLED=false
SMTP_HOST=
SMTP_PORT=587
SMTP_USE_SSL=true
SMTP_USERNAME=
SMTP_PASSWORD=
SMTP_FROM_EMAIL=no-reply@${domain}
SMTP_FROM_NAME=POLYMIND
EOF

chmod 600 "$ENV_FILE"
echo "Đã tạo ${ENV_FILE} với quyền chỉ owner đọc/ghi."
