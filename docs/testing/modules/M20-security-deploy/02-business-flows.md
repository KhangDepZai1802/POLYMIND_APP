# M20 — Security & Deployment · Business Flows

> Module cross-cutting: mô tả các "luồng kiểm soát bảo mật" (security control flows) thay vì CRUD nghiệp vụ. Nguồn: `Program.cs`, `DbSeeder.cs`, `DependencyInjection.cs`, `IdentityRevalidatingAuthenticationStateProvider.cs`, `docker-compose*.yml`.

## BF-M20-01 — Đăng nhập web + lockout
- **Actor:** mọi user. **Precondition:** có tài khoản active.
- **Main:** POST cookie sign-in → Identity kiểm password → phát cookie `HttpOnly`, `SameSite=Lax`, `SecurePolicy=Always` (non-dev).
- **Error:** sai 5 lần → lockout 15 phút (`DependencyInjection.cs`); message chung `InvalidCredentialsMessage` chống enumeration.
- **AuthZ:** không. **Audit:** login log qua Serilog request logging.
- **State cuối:** phiên cookie 8h sliding.
- **Risk:** không rate limit riêng cho login (OBS-M20-04) — lockout Identity là hàng phòng thủ chính.

## BF-M20-02 — Revalidate phiên theo security-stamp
- **Actor:** user có cookie. **Trigger:** mỗi ~30 phút hoặc thay đổi bảo mật.
- **Main:** `IdentityRevalidatingAuthenticationStateProvider` so `SecurityStamp`; đổi role / khóa `IsActive=false` / đổi mật khẩu → stamp xoay → cookie bị vô hiệu (M01/M02 đã Verified).
- **Risk:** JWT Bearer KHÔNG kiểm stamp → không revoke tức thì (OBS-M20-05 → U-M20-2). Cookie path đóng.

## BF-M20-03 — Cấp & xác thực JWT (REST API)
- **Actor:** consumer ngoài (mobile/đối tác). **Entry:** `POST /api/auth/login`.
- **Main:** xác thực → phát JWT (`ExpiryMinutes=240`), claim `permission`/role giữ nguyên (`MapInboundClaims=false`).
- **Validate:** Issuer/Audience/Lifetime/SigningKey = true; `ClockSkew=1min`.
- **AuthZ:** endpoint `/api/*` yêu cầu permission + data-scope fail-closed (M02).
- **Config gate:** thiếu `Jwt:Key` ở production → **throw** khi khởi động (`Program.cs:125`), chặn deploy với key rỗng. Dev dùng key dev-only rõ ràng.
- **Risk:** token sống 4h không revoke được (OBS-M20-05).

## BF-M20-04 — Từ chối truy cập trái quyền
- **Main:** request thiếu permission → `PermissionAuthorizationHandler` fail → 403 (`/access-denied` cho cookie, `Forbid` cho API).
- **Bất biến seed (test M20):** partner (đại lý/CTV) + portal (phụ huynh/học viên) KHÔNG có quyền ghi tài chính/user/role/audit/hoa hồng hay `financial_reports:read` → không leo thang dọc qua seed.
- **Surface đặc thù:** `/hangfire` chỉ SuperAdmin/Director; `/swagger` public mọi env (OBS-M20-03); `/health` public (hạ tầng); CSV export gated `reports:read` + financial slug re-check (M16).

## BF-M20-05 — Pipeline HTTP & headers
- **Main:** `UseForwardedHeaders` → `UseHttpsRedirection` → security headers (`X-Content-Type-Options`, `X-Frame-Options=SAMEORIGIN`, `Referrer-Policy`, `Permissions-Policy`) → `UseAuthentication/Authorization` → `UseAntiforgery` (CSRF Blazor).
- **Prod-only:** `UseExceptionHandler("/Error")` + `UseHsts()` → không lộ stack trace.
- **Thiếu:** CSP (OBS-M20-01); `ForwardedHeaders.KnownProxies.Clear()` tin mọi proxy (OBS-M20-02 — an toàn sau Cloudflare/Caddy).

## BF-M20-06 — Seed khởi động & môi trường
- **Main:** `DbSeeder.SeedAsync` áp migration → seed roles/permissions/role-map → tài khoản.
- **Dev:** seed 8 tài khoản mẫu (`Admin@123`) + `DemoDataSeeder` (chỉ `IsDevelopment()`).
- **Production:** KHÔNG seed mẫu; chỉ tạo 1 super_admin thật từ env `SuperAdmin__Email/Password`. **Thiếu env → `LogError` + KHÔNG tạo tài khoản nào** (`DbSeeder.cs:177-181`) — không lộ credential mặc định (lưu ý: là *log + skip*, không phải *throw*; hệ quả: app boot không có super_admin → cần đặt env rồi khởi động lại).
- **Secret:** `appsettings.json`/`.Production.json` không chứa secret (Jwt.Key rỗng, SMTP/Minio rỗng); mọi secret prod qua env (`docker-compose.production.yml`).
- **Risk:** `appsettings.Development.json` chứa Gemini key (tracked git) — không push repo public (memory `project-polymind-gemini-ai`) → OBS-M20-09.

## BF-M20-07 — Triển khai container
- **Main:** `Dockerfile` multi-stage SDK→aspnet, `Production`. `docker-compose.production.yml`: Postgres/MinIO + reverse proxy TLS; secret từ env.
- **Risk:** container chạy root (OBS-M20-08); Data Protection keys in-memory → cookie/antiforgery invalid sau redeploy/multi-instance (OBS-M20-07); Redis khai báo nhưng app dùng `AddMemoryCache` (chưa wire).
- **Bối cảnh:** giai đoạn TEST = laptop + Cloudflare Tunnel; hardening trên hoãn tới production thật (memory `project-polymind-deploy-plan`) → U-M20-1 checklist go-live.
