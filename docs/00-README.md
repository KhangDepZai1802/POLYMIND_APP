# POLYMIND OLMS — Hướng Dẫn Dự Án

POLYMIND là web quản lý xuất khẩu lao động: Lead CRM, ứng viên, đơn hàng, workflow 17 bước, tài chính, hoa hồng đại lý, visa/xuất cảnh, báo cáo, thông báo và quản trị phân quyền.

## Tech Stack Thực Tế

| Layer | Công nghệ |
|---|---|
| Web | .NET 10, Blazor Web App Interactive Server |
| UI | MudBlazor 9.5 |
| Auth | ASP.NET Core Identity + Cookie + RBAC permission claims |
| Database | PostgreSQL 16 + EF Core 10 |
| File Storage | MinIO/S3-compatible |
| Jobs | Hangfire + PostgreSQL storage |
| PDF/Excel | QuestPDF + ClosedXML |
| Logging | Serilog console + rolling file |
| Deploy | Docker Compose + Nginx reverse proxy |

## Chạy Development

```powershell
cd "C:\Users\khang\OneDrive\Documents\POLYMIND APP"
docker compose up -d
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/Polymind.Web --urls http://localhost:5177
```

Mở: `http://localhost:5177`

Kiểm tra:

```powershell
dotnet build Polymind.slnx
powershell -ExecutionPolicy Bypass -File scripts/smoke-test.ps1 -BaseUrl http://localhost:5177
```

## Tài Khoản Demo Theo Vai Trò

Tất cả tài khoản dùng chung mật khẩu: `Admin@123`.

| Vai trò | Email | Quyền chính |
|---|---|---|
| `super_admin` | `admin@polymind.local` | Toàn quyền mọi module |
| `director` | `director@polymind.local` | Xem tất cả, duyệt thu/chi/hoa hồng, báo cáo |
| `recruitment_manager` | `recruitment.manager@polymind.local` | CRUD Lead, ứng viên, xem đơn hàng/đại lý/báo cáo |
| `recruiter` | `recruiter@polymind.local` | Tạo/sửa Lead và ứng viên, xem đơn hàng/đại lý |
| `document_staff` | `document.staff@polymind.local` | Hồ sơ ứng viên, xem Lead/đơn hàng/visa |
| `visa_staff` | `visa.staff@polymind.local` | Visa và vé máy bay, xem/sửa ứng viên |
| `accountant` | `accountant@polymind.local` | Thu/chi/phiếu/hoa hồng, báo cáo tài chính |
| `agent` | `agent@polymind.local` | Portal đại lý: chỉ ứng viên/hoa hồng của mình |

Tài khoản `agent@` được seed gắn với đại lý `AG-000001`.

## Cấu Hình Secret

Không đặt secret production trong `appsettings.json`. Dùng biến môi trường theo dạng ASP.NET Core:

```powershell
$env:ConnectionStrings__Default='Host=...;Port=5432;Database=polymind;Username=...;Password=...'
$env:Minio__Endpoint='...'
$env:Minio__AccessKey='...'
$env:Minio__SecretKey='...'
$env:Notifications__Email__Enabled='true'
$env:Notifications__Email__Host='smtp.example.com'
```

Development local dùng `src/Polymind.Web/appsettings.Development.json` với cấu hình Docker mặc định.

## Production Deploy

1. Tạo file env và đổi MỌI secret (đặc biệt `SUPERADMIN_EMAIL`/`SUPERADMIN_PASSWORD` — production KHÔNG tạo tài khoản mẫu `Admin@123`, nếu để trống sẽ không có tài khoản nào để đăng nhập):

```powershell
Copy-Item .env.production.example .env.production
notepad .env.production
```

2. Chọn 1 trong 2 reverse proxy:

**Phương án A — Caddy + DuckDNS (khuyến nghị, tự cấp/gia hạn Let's Encrypt):**

- Đặt `DOMAIN`, `ACME_EMAIL` trong `.env.production`.
- Bản ghi A của `DOMAIN` phải trỏ về public IP công ty và đã port-forward 80/443. IP động thì thêm `--profile duckdns` (cần `DUCKDNS_SUBDOMAIN`/`DUCKDNS_TOKEN`).

```powershell
docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy up -d --build
# IP động:
# docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy --profile duckdns up -d --build
```

**Phương án B — Nginx cert thủ công (fallback/legacy):**

Đặt `fullchain.pem` + `privkey.pem` vào `deploy/nginx/certs/`, rồi:

```powershell
docker compose --env-file .env.production -f docker-compose.production.yml --profile nginx up -d --build
```

3. Kiểm tra:

```powershell
docker compose --env-file .env.production -f docker-compose.production.yml ps
curl https://<DOMAIN>/health   # Caddy: cert thật; Nginx self-signed: thêm -k
```

Ứng dụng tự chạy migration khi startup. Demo data + 8 tài khoản mẫu chỉ seed ở môi trường `Development`; `Production` chỉ tạo duy nhất super admin từ `SUPERADMIN_EMAIL`/`SUPERADMIN_PASSWORD`.

## Backup Và Restore

Backup PostgreSQL + MinIO:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/backup.ps1 -EnvFile .env.production
```

Restore từ một thư mục backup:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/restore.ps1 -BackupDir db-backups\YYYYMMDD-HHMMSS -EnvFile .env.production -ConfirmRestore
```

Restore là thao tác phá dữ liệu hiện tại, chỉ chạy khi đã xác nhận đúng backup.

## Endpoint Vận Hành

| Endpoint | Mục đích |
|---|---|
| `/health` | Health check DB + MinIO |
| `/hangfire` | Dashboard job nền, chỉ `super_admin`/`director` |
| `/admin` | Quản trị user/role/permission/audit |
| `/notifications` | Thông báo và tùy chọn kênh nhận |

## Tài Liệu Nền

| File | Nội dung |
|---|---|
| [01-business-analysis.md](01-business-analysis.md) | Phân tích nghiệp vụ, actor, module |
| [02-database-design.md](02-database-design.md) | Schema PostgreSQL, ERD, index |
| [03-workflow.md](03-workflow.md) | Workflow 17 bước, thông báo, KPI |
| [04-system-architecture.md](04-system-architecture.md) | Kiến trúc gốc, một số phần còn theo stack cũ |
| [05-handoff-codex.md](05-handoff-codex.md) | Bàn giao kỹ thuật, bẫy, backlog |
