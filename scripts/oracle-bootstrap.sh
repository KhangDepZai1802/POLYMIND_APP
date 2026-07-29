#!/usr/bin/env bash
#
# Chuẩn bị một VM Ubuntu ARM64 trên Oracle Cloud cho POLYMIND.
# Chạy một lần sau khi upload bundle:
#   bash scripts/oracle-bootstrap.sh
#
set -euo pipefail

SWAP_SIZE_GB="${SWAP_SIZE_GB:-4}"

if [ "$(id -u)" -eq 0 ]; then
    SUDO=()
    LOGIN_USER="${SUDO_USER:-ubuntu}"
else
    SUDO=(sudo)
    LOGIN_USER="${USER}"
    sudo -v
fi

echo "==> [1/5] Cài gói nền"
"${SUDO[@]}" apt-get update
"${SUDO[@]}" env DEBIAN_FRONTEND=noninteractive apt-get install -y \
    ca-certificates curl git iptables-persistent openssl

echo "==> [2/5] Mở firewall hệ điều hành cho SSH/HTTP/HTTPS"
for port in 22 80 443; do
    if ! "${SUDO[@]}" iptables -C INPUT -p tcp --dport "$port" -j ACCEPT 2>/dev/null; then
        "${SUDO[@]}" iptables -I INPUT 1 -p tcp --dport "$port" -j ACCEPT
    fi
done
"${SUDO[@]}" netfilter-persistent save

echo "==> [3/5] Cài Docker Engine + Compose plugin"
if ! command -v docker >/dev/null 2>&1; then
    installer="$(mktemp)"
    curl -fsSL https://get.docker.com -o "$installer"
    "${SUDO[@]}" sh "$installer"
    rm -f "$installer"
fi
"${SUDO[@]}" systemctl enable --now docker
"${SUDO[@]}" usermod -aG docker "$LOGIN_USER"

echo "==> [4/5] Tạo swap ${SWAP_SIZE_GB}GB nếu VM chưa có swap"
if [ -z "$(swapon --show --noheadings)" ]; then
    if [ ! -e /swapfile ]; then
        "${SUDO[@]}" fallocate -l "${SWAP_SIZE_GB}G" /swapfile
        "${SUDO[@]}" chmod 600 /swapfile
        "${SUDO[@]}" mkswap /swapfile
    fi
    "${SUDO[@]}" swapon /swapfile
    if ! grep -q '^/swapfile ' /etc/fstab; then
        echo '/swapfile none swap sw 0 0' | "${SUDO[@]}" tee -a /etc/fstab >/dev/null
    fi
fi

echo "==> [5/5] Kiểm tra"
docker_version="$("${SUDO[@]}" docker --version)"
compose_version="$("${SUDO[@]}" docker compose version)"
arch="$(uname -m)"
echo "    Kiến trúc: ${arch}"
echo "    ${docker_version}"
echo "    ${compose_version}"
echo
echo "HOÀN TẤT bootstrap."
echo "Đăng xuất SSH rồi đăng nhập lại để user '${LOGIN_USER}' nhận quyền Docker."
