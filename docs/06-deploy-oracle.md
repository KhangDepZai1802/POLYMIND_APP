# Triển khai POLYMIND lên Oracle Cloud Always Free (24/7)

> Mục tiêu: web sống 24/7, miễn phí, độc lập điện/mạng công ty. Chạy nguyên bộ
> `docker-compose.production.yml` trên 1 VM Always Free (ARM Ampere), DuckDNS trỏ
> domain về public IP của VM, Caddy tự cấp HTTPS Let's Encrypt.

## Tổng quan 4 giai đoạn
1. **Tạo tài khoản Oracle + VM Always Free** (bạn làm trên web console).
2. **Domain**: DuckDNS subdomain → public IP của VM.
3. **Cài server + deploy** (SSH; có thể để Claude điều khiển từ máy local của bạn).
4. **Verify + vận hành** (auto-start khi reboot, backup hằng ngày).

---

## GIAI ĐOẠN 1 — Tài khoản + VM (bạn làm)

### 1.1 Đăng ký
- Vào https://www.oracle.com/cloud/free/ → **Start for free**.
- Cần: email, **số điện thoại** (nhận OTP), **thẻ Visa/Mastercard** để xác minh (không bị trừ tiền với Always Free; có thể bị tạm giữ ~1 USD rồi hoàn).
- **Home Region: CHỌN KỸ — KHÔNG đổi được sau này.** Cho VN nên chọn **Singapore (ap-singapore-1)** (gần nhất, độ trễ thấp). Phương án khác: Tokyo/Osaka (Nhật).

### 1.2 Tạo VM
- Console → **Compute → Instances → Create instance**.
- **Image & shape → Change shape → Ampere (ARM)**: shape `VM.Standard.A1.Flex`.
  - OCPU: **2**, RAM: **12 GB** (đủ dư cho cả stack; tối đa Always Free là 4 OCPU/24 GB).
  - Nếu báo **"Out of capacity"** → giảm còn 1 OCPU/6 GB, đổi Availability Domain (AD-1/2/3), hoặc thử lại sau (giờ thấp điểm). Đây là lỗi hay gặp, cứ kiên nhẫn thử lại.
- **Image**: Canonical **Ubuntu 22.04** (ARM build — aarch64).
- **Networking**: tạo VCN mới (mặc định OK), **Assign a public IPv4 address = Yes**.
- **SSH keys**: chọn **Generate a key pair for me** → **TẢI private key** (`.key`) về máy NGAY (không tải lại được). Lưu vào nơi nhớ được, vd `C:\Users\khang\.ssh\oracle-polymind.key`.
- Create. Đợi state = **Running**, ghi lại **Public IP address**.

### 1.3 (Khuyến nghị) Public IP tĩnh + chống thu hồi
- Public IP mặc định có thể đổi khi stop/start. Vào instance → IP address → reserve public IP để cố định (free).
- Để KHÔNG bị thu hồi VM idle: cân nhắc **Upgrade to Pay As You Go** (vẫn $0 nếu chỉ dùng trong hạn mức Always Free). App chạy + Hangfire mỗi 5' thường đã đủ "bận".

### 1.4 Mở port ở firewall ĐÁM MÂY (Security List)
- Console → VCN → Subnet → **Security List** → Add **Ingress Rules**:
  - Source `0.0.0.0/0`, TCP, **port 80**.
  - Source `0.0.0.0/0`, TCP, **port 443**.
  - (Port 22 SSH thường đã mở sẵn.)

> ⚠️ **GOTCHA Oracle Ubuntu:** ngoài Security List, **OS Ubuntu còn iptables chặn sẵn** mọi port trừ 22. Phải mở thêm ở GIAI ĐOẠN 3, nếu không 80/443 vẫn không vào được dù Security List đã mở.

**➡️ Báo lại cho Claude khi xong GĐ1:** (a) Public IP, (b) đường dẫn file private key trên máy, (c) version Ubuntu.

---

## GIAI ĐOẠN 2 — Domain (DuckDNS)

- Vào https://www.duckdns.org → đăng nhập (Google/GitHub) → tạo subdomain
  `polymindolms` (fallback `polymindolmsvn`, `polymindolms2026`).
- Đặt ô **current ip = Public IP của VM** → **update ip**. Ghi lại **token**.
- Kết quả: `polymindolms.duckdns.org` trỏ về VM. (IP tĩnh thì set 1 lần là xong;
  nếu không tĩnh, container `duckdns` trong compose sẽ tự cập nhật.)

---

## GIAI ĐOẠN 3 — Cài server + deploy (qua SSH)

> Có thể để Claude chạy các lệnh này qua SSH từ máy local của bạn (máy đang code),
> hoặc bạn tự dán. `KEY` = đường dẫn private key, `IP` = public IP VM.

```bash
# 0) SSH vào (Ubuntu user mặc định là 'ubuntu')
ssh -i KEY ubuntu@IP

# 1) Mở iptables OS cho 80/443 và lưu vĩnh viễn
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save

# 2) Cài Docker + compose plugin
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER && newgrp docker

# 3) Lấy mã nguồn (git clone repo) hoặc scp từ máy local
git clone <REPO_URL> polymind && cd polymind

# 4) Tạo .env.production (secret mạnh) — xem .env.production.example
#    BẮT BUỘC: SUPERADMIN_EMAIL/PASSWORD, DOMAIN, ACME_EMAIL, JWT_KEY, mật khẩu DB/MinIO
cp .env.production.example .env.production && nano .env.production

# 5) Kiểm tra cấu hình rồi chạy (profile caddy = auto HTTPS)
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy config -q
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy up -d --build
```

> Build image .NET trên ARM cần vài phút + RAM (vì vậy chọn ≥ 6 GB). `web` lắng nghe
> nội bộ 8080, chỉ Caddy expose 80/443 ra ngoài.

---

## GIAI ĐOẠN 4 — Verify + vận hành

- **Auto-start khi reboot:** Docker service bật sẵn (`sudo systemctl enable docker`);
  các container đã có `restart: unless-stopped` → tự lên lại sau reboot.
- **Verify:**
  - `docker logs polymind-prod-caddy` → thấy cấp cert thành công.
  - `https://polymindolms.duckdns.org/health` → `Healthy` (hoặc `Degraded` nếu bucket
    MinIO chưa tạo, KHÔNG được `Unhealthy`).
  - `scripts/smoke-test.ps1 -BaseUrl https://polymindolms.duckdns.org -Password <admin-prod-pw>`.
  - Đăng nhập bằng super admin thật; xác nhận `admin@polymind.local / Admin@123` **không** đăng nhập được.
- **Backup hằng ngày:** cron chạy `scripts/backup.ps1` (hoặc bản bash tương đương), copy ra nơi khác.
- **Tạo user thật theo role** trong `/admin`, mật khẩu mạnh.

## Gotchas tổng hợp
- Region home KHÔNG đổi được → chọn đúng từ đầu.
- Out-of-capacity ARM → giảm OCPU/RAM, đổi AD, thử lại.
- Hai lớp firewall: Security List (cloud) **và** iptables (OS Ubuntu) — phải mở cả hai.
- Let's Encrypt không cấp cert cho IP trần → bắt buộc có domain (DuckDNS).
- Mất private key = mất quyền SSH (phải tạo lại qua console/recovery).
