# Triển khai POLYMIND lên Oracle Cloud Always Free

> Trạng thái quyết định: **đã chốt Oracle Cloud Always Free**.
>
> Kiến trúc: một VM ARM64 chạy Docker Compose gồm PostgreSQL, MinIO, POLYMIND Web và
> Caddy HTTPS. Backup hằng ngày phải được đẩy ra kho nằm ngoài VM.

## Hạn mức hiện hành và cấu hình chọn

Tài liệu Oracle cập nhật ngày 29/06/2026 quy định Always Free Ampere A1 tổng cộng:

- 2 OCPU;
- 12 GB RAM;
- 200 GB Block Volume, tính cả boot disk;
- 20 GB Object Storage.

Không dùng số cũ 4 OCPU/24 GB.

Cấu hình khuyến nghị cho POLYMIND:

- Home Region: Singapore;
- Shape: `VM.Standard.A1.Flex` (ARM);
- 2 OCPU, 8 GB RAM;
- Ubuntu 24.04 LTS ARM64; dùng Ubuntu 22.04 LTS nếu console chưa có 24.04;
- Boot volume 100 GB;
- Public IPv4: bật;
- Swap: script bootstrap tạo thêm 4 GB.

Oracle có thể thu hồi VM Always Free bị xem là idle. Không lưu bản duy nhất của dữ liệu
nghiệp vụ trên VM; bắt buộc có backup ngoài máy và giám sát uptime.

## Phân công

### Người dùng bắt buộc thực hiện

1. Tạo/xác minh tài khoản Oracle bằng email, điện thoại và thẻ.
2. Chọn Home Region và tạo VM từ Oracle Console.
3. Tải private SSH key về máy.
4. Mở ingress 80/443 trên Oracle Cloud.
5. Tạo hoặc cấu hình domain/DuckDNS trỏ về public IP.
6. Gửi lại IP, đường dẫn private key và domain.

### Codex thực hiện sau khi nhận thông tin

1. Kiểm tra SSH và fingerprint máy chủ.
2. Upload bundle mã nguồn hiện tại, không phụ thuộc GitHub/private repository.
3. Chạy bootstrap Docker/firewall/swap.
4. Tạo secret production; người dùng chỉ nhập mật khẩu super admin.
5. Build ARM64, chạy migration, PostgreSQL, MinIO, Caddy và Web.
6. Kiểm tra HTTPS, health, đăng nhập, upload/tải hồ sơ và restart.
7. Cấu hình backup, cron và bàn giao lệnh vận hành.

## Giai đoạn 1 — Người dùng tạo tài khoản và VM

### 1. Tạo tài khoản

1. Mở <https://www.oracle.com/cloud/free/>.
2. Chọn **Start for free**.
3. Xác minh email và số điện thoại.
4. Nhập thẻ Visa/Mastercard để xác minh danh tính.
5. Tại **Home Region**, chọn **Singapore**.

Home Region không đổi được sau khi tạo account. Nếu Oracle từ chối thẻ hoặc báo lỗi tài
khoản, chụp nguyên màn hình lỗi nhưng che số thẻ trước khi gửi.

### 2. Tạo VM

Trong Oracle Console:

1. Vào **Compute → Instances → Create instance**.
2. Name: `polymind-production`.
3. Image: **Canonical Ubuntu 24.04 Minimal aarch64**; fallback Ubuntu 22.04 aarch64.
4. Shape:
   - Ampere;
   - `VM.Standard.A1.Flex`;
   - 2 OCPU;
   - 8 GB RAM;
   - phải thấy nhãn **Always Free eligible**.
5. Networking:
   - tạo VCN/subnet mặc định nếu chưa có;
   - **Assign a public IPv4 address: Yes**.
6. SSH key:
   - chọn **Generate a key pair for me**;
   - tải private key ngay;
   - lưu ví dụ tại `C:\Users\khang\.ssh\oracle-polymind.key`.
7. Boot volume:
   - 100 GB;
   - không chọn hiệu năng trả phí;
   - kiểm tra tổng chi phí dự kiến hiển thị 0 trong Always Free.
8. Chọn **Create**, chờ state `Running`.

Nếu báo `Out of host capacity`, thử Availability Domain khác hoặc thử lại sau. Không tạo
shape không có nhãn Always Free chỉ để vượt lỗi capacity.

### 3. Mở firewall Oracle Cloud

Vào VCN của VM → Subnet → Security List → Add Ingress Rules:

| Source | Protocol | Port | Mục đích |
|---|---|---:|---|
| `0.0.0.0/0` | TCP | 80 | HTTP/Let's Encrypt |
| `0.0.0.0/0` | TCP | 443 | HTTPS |
| IP hiện tại của quản trị viên `/32` | TCP | 22 | SSH |

Nếu chưa biết IP quản trị viên hoặc IP thường xuyên thay đổi, có thể tạm để SSH
`0.0.0.0/0`; sau khi deploy xong phải siết lại.

Không mở các cổng PostgreSQL 5432, MinIO 9000/9001 hoặc Web 8080 ra Internet.

### 4. Domain

Ưu tiên domain chính thức do đơn vị sở hữu. Nếu chưa có:

1. Mở <https://www.duckdns.org/>.
2. Đăng nhập và tạo một subdomain, ví dụ `polymindolms`.
3. Cập nhật IP bằng public IPv4 của Oracle VM.
4. Kết quả là `polymindolms.duckdns.org`.

Domain phải phân giải về đúng IP trước khi Caddy xin chứng chỉ HTTPS.

### 5. Thông tin gửi lại cho Codex

Không gửi nội dung private key, mật khẩu thẻ hoặc OTP trong chat. Chỉ gửi:

```text
ORACLE_PUBLIC_IP=...
SSH_PRIVATE_KEY_PATH=C:\...\oracle-polymind.key
DOMAIN=...
UBUNTU_VERSION=24.04
```

Private key vẫn nằm trên máy local; Codex sử dụng đường dẫn đó để gọi `ssh.exe`.

## Giai đoạn 2 — Chuẩn bị và upload bundle

Thực hiện từ máy đang chứa source:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/New-OracleDeployBundle.ps1
scp -i C:\path\oracle-polymind.key `
  artifacts\polymind-oracle-deploy.tar.gz `
  ubuntu@ORACLE_PUBLIC_IP:/home/ubuntu/
```

Trên VM:

```bash
mkdir -p /home/ubuntu/polymind
tar -xzf /home/ubuntu/polymind-oracle-deploy.tar.gz -C /home/ubuntu/polymind
cd /home/ubuntu/polymind
bash scripts/oracle-bootstrap.sh
```

Đăng xuất và SSH lại sau bootstrap để group Docker có hiệu lực.

## Giai đoạn 3 — Secret và deploy

```bash
cd /home/ubuntu/polymind
bash scripts/init-production-env.sh
bash scripts/deploy-oracle.sh
```

`init-production-env.sh` chỉ yêu cầu domain, email và mật khẩu super admin; toàn bộ password
PostgreSQL/MinIO/JWT được sinh ngẫu nhiên. File `.env.production` có quyền `600` và bị Git
bỏ qua.

Ứng dụng tự chạy migration khi startup. Production chỉ tạo super admin từ biến môi trường,
không seed tài khoản/mật khẩu demo.

## Giai đoạn 4 — Kiểm tra trước go-live

```bash
cd /home/ubuntu/polymind
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy ps
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy logs --tail=150
curl -fsS https://DOMAIN/health
```

Checklist bắt buộc:

- `/health` trả HTTP 200 và PostgreSQL/MinIO đều `Healthy`;
- HTTP chuyển sang HTTPS;
- đăng nhập được bằng super admin production;
- tài khoản demo `admin@polymind.local / Admin@123` đăng nhập thất bại;
- upload và tải lại một file thử;
- restart VM xong toàn bộ container tự lên lại;
- cookie đăng nhập/Data Protection keys tồn tại qua restart;
- không truy cập được 5432, 9000, 9001, 8080 từ Internet;
- backup PostgreSQL và MinIO chạy thành công;
- thử restore backup vào môi trường tạm trước khi mở người dùng thật.

## Lệnh vận hành

```bash
# Trạng thái
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy ps

# Log
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy logs -f --tail=100

# Deploy lại bundle/source mới
bash scripts/deploy-oracle.sh

# Backup thủ công
bash scripts/backup.sh

# Dung lượng và RAM
df -h
free -h
docker stats --no-stream
```

## Nguồn hạn mức

- Oracle Free Tier: <https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier.htm>
- Always Free Resources: <https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm>
