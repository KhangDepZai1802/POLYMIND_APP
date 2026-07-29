# Triển khai POLYMIND lên VPS Việt Nam (phương án trả phí dự phòng)

> **Đã được thay thế ngày 29/07/2026:** production hiện chốt **Oracle Cloud Always Free** theo
> `06-deploy-oracle.md`. Giữ tài liệu này làm phương án trả phí dự phòng khi Oracle hết capacity
> hoặc chất lượng kết nối Singapore không đạt yêu cầu.
>
> Quyết định cũ ngày 21/07/2026 chọn VPS Việt Nam vì app dùng **Blazor Interactive Server** —
> mỗi thao tác là một round-trip mạng, nên độ trễ tới người dùng quyết định cảm giác mượt.
> VN→VPS trong nước ~5–15ms, VN→Singapore ~40–80ms.

## Tổng quan

| Giai đoạn | Ai làm | Thời gian |
|---|---|---|
| GĐ1 — Mua hạ tầng | **Bạn** | ~1 giờ + 1–3 ngày chờ duyệt (.vn) |
| GĐ2 — Cài server + deploy | AI (qua SSH) | 2–3 giờ |
| GĐ3 — Backup + giám sát | AI | 2 giờ |
| GĐ4 — Mở cho người dùng thật | Bạn + AI | 1 tuần, 3 đợt role |

---

## GIAI ĐOẠN 1 — Mua hạ tầng (bạn làm)

### 1.1 VPS **(tốn phí ~700.000đ/tháng)**
- Nhà cung cấp gợi ý: **Vietnix / AZDIGI / TinoHost** (đều hỗ trợ tiếng Việt, thanh toán nội địa).
- Cấu hình: **4 vCPU / 8GB RAM / 100GB SSD NVMe**, hệ điều hành **Ubuntu 22.04**, datacenter **HCM**.
- Chọn gói có **IPv4 riêng**. Trả theo năm thường giảm 10–20%.
- ⚠️ Đừng lấy gói 2GB RAM: build image .NET trong Docker cần ~2–3GB lúc build.

**➡️ Báo lại cho AI:** IP public, user + mật khẩu root (hoặc SSH key), tên nhà cung cấp.

### 1.2 Tên miền **(tốn phí 300.000–750.000đ/năm)**
- `.com` (Namecheap/Cloudflare, ~300k/năm): mua xong dùng ngay.
- `.vn` (Nhân Hòa/Mắt Bão, ~750k/năm): uy tín hơn tại VN nhưng **cần CCCD + giấy phép kinh doanh**, duyệt 1–3 ngày.

**➡️ Báo lại cho AI:** tên miền đã mua.

### 1.3 Cloudflare Free (0đ)
- Tạo tài khoản → **Add a site** → nhập tên miền → chọn gói **Free**.
- Cloudflare cho 2 nameserver → vào trang quản lý tên miền đổi nameserver sang 2 địa chỉ đó.
- Chờ propagate ~30 phút (Cloudflare gửi email khi xong).
- ⚠️ **Chưa bật proxy (đám mây cam) vội** — xem GĐ2, phải để DNS-only cho lần cấp chứng chỉ đầu tiên.

### 1.4 Backblaze B2 — kho backup ngoài VPS (0đ dưới 10GB)
- Tạo tài khoản tại backblaze.com → **B2 Cloud Storage** → **Create a Bucket**.
- Tên bucket: `polymind-backup`, để **Private**.
- Vào **Application Keys** → **Add a New Application Key**, giới hạn đúng bucket này.

**➡️ Báo lại cho AI:** keyID, applicationKey, tên bucket. (Ghi lại ngay — applicationKey chỉ hiện 1 lần.)

### 1.5 Email gửi thông báo (0đ)
Chọn **một** trong hai:
- **Gmail**: bật xác minh 2 bước → tạo **App Password** 16 ký tự. Nhanh nhất.
- **Zoho Mail Free**: gắn tên miền vừa mua → có `no-reply@polymind.vn`, chuyên nghiệp hơn, cấu hình lâu hơn ~30 phút.

**➡️ Báo lại cho AI:** SMTP host, port, username, password.

---

## GIAI ĐOẠN 2 — Cài server + deploy (AI làm qua SSH)

```bash
# 1) Tài khoản thường + tường lửa (KHÔNG chạy service bằng root)
adduser polymind && usermod -aG sudo polymind
ufw allow 22,80,443/tcp && ufw enable
# Trong /etc/ssh/sshd_config: PermitRootLogin no, PasswordAuthentication no (sau khi đã cài SSH key)

# 2) Docker + compose plugin
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker polymind && newgrp docker

# 3) Swap 4GB — đệm RAM lúc build .NET, tránh bị OOM kill giữa chừng
sudo fallocate -l 4G /swapfile && sudo chmod 600 /swapfile
sudo mkswap /swapfile && sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab

# 4) Mã nguồn + cấu hình
git clone <REPO_URL> polymind && cd polymind
cp .env.production.example .env.production && nano .env.production
#    Sinh secret mạnh: openssl rand -base64 48

# 5) Chạy
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy config -q
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy up -d --build
```

### `.env.production` — khác gì so với bản mẫu
| Biến | Giá trị production |
|---|---|
| `DOMAIN` | tên miền thật (vd `polymind.vn`) — **không** còn DuckDNS |
| `ACME_EMAIL` | email nhận cảnh báo hết hạn chứng chỉ |
| `SUPERADMIN_EMAIL` / `SUPERADMIN_PASSWORD` | tài khoản thật, mật khẩu mạnh. **Để trống = app không tạo tài khoản nào và bạn không đăng nhập được** |
| `JWT_KEY` | `openssl rand -base64 48` |
| `POSTGRES_PASSWORD`, `MINIO_ROOT_PASSWORD` | mỗi cái một chuỗi ngẫu nhiên riêng |
| `SMTP_ENABLED` | `true` + thông tin SMTP từ mục 1.5 |
| `Ai__Gemini__ApiKey` | key Gemini (model `gemini-2.5-flash`) |

**Bỏ hẳn** `DUCKDNS_*` và profile `duckdns` — DNS nay quản ở Cloudflare.

### Thứ tự bật Cloudflare proxy (làm sai là không cấp được HTTPS)
1. Tạo bản ghi `A @ → IP VPS`, để **DNS only (đám mây xám)**.
2. Chạy compose, đợi Caddy xin xong chứng chỉ Let's Encrypt (`docker logs polymind-prod-caddy`).
3. Truy cập `https://<domain>/health` thấy `Healthy`.
4. **Lúc này mới** bật **Proxied (đám mây cam)**, và đặt SSL/TLS mode = **Full (strict)**.

> Let's Encrypt cần chạm trực tiếp vào server ở port 80. Bật proxy trước khi có cert sẽ làm bước xác thực thất bại.

---

## GIAI ĐOẠN 3 — Backup + giám sát

```bash
# Cài rclone và tạo remote "b2" (nhập keyID + applicationKey từ mục 1.4)
curl https://rclone.org/install.sh | sudo bash
rclone config

# Backup thử ngay lần đầu
./scripts/backup.sh

# Cron hằng đêm 2h sáng
crontab -e
# 0 2 * * * cd /home/polymind/polymind && ./scripts/backup.sh >> /var/log/polymind-backup.log 2>&1
```

- **Test restore** (`scripts/restore.sh`) vào một stack tạm — **backup chưa test = không có backup**.
- **UptimeRobot** (free): ping `https://<domain>/health` mỗi 5 phút, cảnh báo email khi sập.
- Log Docker đã giới hạn sẵn 10MB × 3 file mỗi service trong `docker-compose.production.yml`.

---

## GIAI ĐOẠN 4 — Mở cho người dùng thật (3 đợt)

1. **Tuần 1 — Nhân sự nội bộ.** Tạo tài khoản thật theo `POLYMIND - Danh sach tai khoan he thong.xlsx`, mật khẩu riêng từng người. Hướng dẫn qua trang `/guide`.
2. **Tuần 2 — Đại lý & CTV.** ⚠️ Trước khi mở: đăng nhập bằng một tài khoản đại lý thật và xác nhận **không thấy** dữ liệu của đại lý khác ở **mọi** đường — danh sách, báo cáo, export CSV, Trợ lý AI, tin nhắn. Đây là rủi ro rò rỉ dữ liệu lớn nhất khi mở ra ngoài công ty.
3. **Tuần 3 — Ứng viên/Học viên.** Mở cổng `/me`. Nhóm đông nhất → theo dõi RAM/CPU (`docker stats`); chật thì nâng RAM VPS (nâng nóng, không mất dữ liệu).

---

## Checklist go-live (bắt buộc trước khi mời người dùng thật)

- [ ] `https://<domain>/health` → `Healthy`
- [ ] `http://` tự chuyển sang `https://`, chứng chỉ hợp lệ
- [ ] `admin@polymind.local / Admin@123` **đăng nhập THẤT BẠI**
- [ ] `/hangfire` chặn với tài khoản không phải Super Admin / Giám đốc
- [ ] Đăng nhập từng role thật, chạy 1 ứng viên qua vài bước trong quy trình 20 bước
- [ ] Upload ảnh CCCD → tải lại được (kiểm luôn **CSP** không chặn xem trước ảnh)
- [ ] Ghi âm + nghe lại tin nhắn thoại (kiểm **CSP** không chặn `blob:`)
- [ ] Trợ lý AI trả lời được (kiểm CSP không chặn kết nối)
- [ ] Nhận được email thông báo thật
- [ ] Duyệt khoản chi: chưa duyệt thì **không** xuất được phiếu chi
- [ ] `sudo reboot` → mọi container tự lên lại
- [ ] `./scripts/backup.sh` → thấy file trên B2 → **restore thử thành công vào stack tạm**

> Nếu CSP chặn nhầm thứ gì, không cần build lại: đặt biến môi trường
> `Security__ContentSecurityPolicy` với chuỗi nới hơn (hoặc rỗng để tắt) rồi `docker compose up -d`.

## Gotchas

- **Hai lớp cần mở port**: `ufw` trên OS. VPS VN thường không có firewall đám mây riêng như Oracle.
- **Bật Cloudflare proxy quá sớm** → Let's Encrypt cấp cert thất bại. Xem đúng thứ tự ở GĐ2.
- **Mất private key SSH** = mất quyền vào VPS (phải dùng console cứu hộ của nhà cung cấp).
- **RAM 4GB build sẽ sát nút** — nếu chọn gói nhỏ hơn thì bắt buộc bật swap hoặc build image ở nơi khác rồi push.
- **Backup nằm cùng VPS là không phải backup** — bắt buộc đẩy ra B2.
