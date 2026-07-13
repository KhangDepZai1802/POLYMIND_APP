# M20 — Security & Deployment · Analysis

## 1. Module Overview

- **Module ID:** M20
- **Module name:** Security & Deployment (bảo mật cấu hình + triển khai)
- **Business purpose:** Rà soát tư thế bảo mật xuyên suốt (auth cookie/JWT, security headers, CSRF, lockout, secret/env, IDOR tổng hợp, rate limit) và cấu hình triển khai (Docker, reverse proxy TLS, seed production). Module cross-cutting — hợp nhất kết luận bảo mật từ M01–M19.
- **Actor:** vận hành (deploy), attacker (mô hình đe dọa), mọi user (chịu tác động chính sách bảo mật).
- **Dependencies:** tất cả module (đặc biệt M01 Auth, M02 AuthZ đã Verified).
- **Entry point:** `Program.cs` (pipeline), `DependencyInjection.cs` (Identity), `Dockerfile`, `docker-compose*.yml`, `appsettings*.json`.
- **Bối cảnh triển khai (memory `project-polymind-deploy-plan`):** giai đoạn TEST = laptop + Cloudflare Tunnel; production Oracle/VPS VN để sau. Một số hardening (non-root, CSP, rate limit) hợp lý **hoãn tới giai đoạn production thật**.

## 2. Source Code Map

| File | Mục đích | Ghi chú bảo mật |
|---|---|---|
| `src/Polymind.Web/Program.cs` | Pipeline HTTP, auth, headers, seed | Xem mục 4 |
| `src/Polymind.Infrastructure/DependencyInjection.cs:26-37` | Identity options | Password 8+ upper/lower/digit; Lockout 5 lần/15 phút; `RequireConfirmedAccount=false` |
| `src/Polymind.Web/Authorization/HangfireDashboardAuthorizationFilter.cs` | Gate `/hangfire` | Chỉ authenticated + SuperAdmin/Director ✅ |
| `src/Polymind.Infrastructure/Identity/AuthenticationSecurityPolicy.cs` | Message chống enumeration | `InvalidCredentialsMessage` chung (M01) |
| `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs` | Revalidate cookie theo security-stamp | Khóa/đổi role → invalidate phiên cookie (M01/M02) |
| `appsettings.json` / `.Production.json` | Config | Jwt.Key rỗng (bắt buộc env prod); không secret trong file tracked |
| `appsettings.Development.json` | Config dev | **Chứa Gemini key** (memory) — tracked git, KHÔNG push public → OBS-M20-09 |
| `Dockerfile` | Build image | Multi-stage SDK→aspnet; Production; `UseAppHost=false`; **chạy root** (OBS-M20-08) |
| `docker-compose.production.yml` | Triển khai prod | Secret từ env (`JWT_KEY`, `POSTGRES_PASSWORD`, `SUPERADMIN_*`, `MINIO_*`, `SMTP_*`); Caddy/Nginx TLS; super_admin từ env (không Admin@123 prod) |
| `docker-compose.yml` | Dev stack | Postgres/MinIO local |

## 3. Cấu hình bảo mật đã kiểm (đối chiếu source)

### 3.1 Cookie (Program.cs:104-115)
- `HttpOnly=true` ✅ · `SameSite=Lax` ✅ · `SecurePolicy=Always` ở non-dev (SameAsRequest dev) ✅ · `ExpireTimeSpan=8h` + `SlidingExpiration=true` · `LoginPath=/login`, `AccessDeniedPath=/access-denied`.

### 3.2 JWT (Program.cs:119-152)
- `Key` bắt buộc từ env ở prod (**throw** nếu thiếu — dòng 125); dev có fallback key rõ ràng dev-only.
- Validation đầy đủ: `ValidateIssuer/Audience/Lifetime/IssuerSigningKey=true`, `ClockSkew=1min`, `MapInboundClaims=false` (giữ claim permission/role).
- `ExpiryMinutes=240` (4h, appsettings.json).
- **Hạn chế:** JWT KHÔNG kiểm security-stamp → không revoke được như cookie (OBS-M20-05).

### 3.3 Security headers (Program.cs:195-203)
- `X-Content-Type-Options: nosniff` ✅ · `X-Frame-Options: SAMEORIGIN` ✅ · `Referrer-Policy: strict-origin-when-cross-origin` ✅ · `Permissions-Policy: camera=(), microphone=(self), geolocation=()` ✅.
- **Thiếu:** `Content-Security-Policy` (OBS-M20-01).

### 3.4 Transport & pipeline
- `UseHsts()` + `UseHttpsRedirection()` (HSTS chỉ non-dev) ✅ · `UseAntiforgery()` (CSRF Blazor) ✅ · `UseForwardedHeaders()` (X-Forwarded-For/Proto).
- `UseExceptionHandler("/Error")` + `UseStatusCodePagesWithReExecute` ở prod → không lộ stack trace ✅.
- **ForwardedHeaders:** `KnownIPNetworks.Clear()` + `KnownProxies.Clear()` → tin mọi proxy (OBS-M20-02; an toàn sau Cloudflare/Caddy, rủi ro nếu expose trực tiếp).

### 3.5 Identity (DependencyInjection.cs:26-37)
- Password: `RequiredLength=8`, `RequireDigit/Lowercase/Uppercase=true`, `RequireNonAlphanumeric=false`.
- Lockout: `MaxFailedAccessAttempts=5`, `DefaultLockoutTimeSpan=15min`, `AllowedForNewUsers=true`.
- 2FA đã gỡ ở giai đoạn test (Session 62); bản final sẽ dùng SMS OTP (WORKLOG).

### 3.6 Secret & seed
- Không secret trong `appsettings.json`/`.Production.json` (Jwt.Key="", SMTP/Minio rỗng).
- Prod: mọi secret qua env; super_admin thật từ `SuperAdmin__Email/Password` (DbSeeder **throw** nếu prod thiếu).
- `DemoDataSeeder` chỉ chạy `IsDevelopment()` → không seed demo ở prod ✅.

## 4. Attack Surface / API Inventory (tổng hợp)

| Bề mặt | Auth | Ghi chú |
|---|---|---|
| Blazor UI (`/*`) | Cookie + `[Authorize]` + permission policies | Per-page, đã kiểm M01–M19 |
| REST `/api/auth/login` | Anonymous | Lockout Identity; message chung chống enumeration (M01) |
| REST `/api/leads,/api/candidates,/api/job-orders` | JWT Bearer + permission | Data-scope fail-closed (M02) |
| `/swagger` | **Public mọi env** | Info-disclosure schema (OBS-M20-03) |
| `/hangfire` | Cookie + SuperAdmin/Director | Gated ✅ |
| `/health` | Public | Trả trạng thái db/minio (thông tin hạ tầng, chấp nhận) |
| `/Account/Logout` | Authenticated POST | Xóa AI session (RB-5) + SignOut |
| CSV export | `reports:read` | Gated (M16) |
| MinIO presigned | Server-gen objectKey | Không path traversal (M18) |

## 5. Database / Deployment Impact

- Không schema riêng của M20. Deployment: Postgres 16 + MinIO + Redis (định nghĩa nhưng **chưa wire** vào app — dùng `AddMemoryCache`), web container, reverse proxy.
- **Data Protection keys:** mặc định in-memory → mất khi restart/multi-instance → cookie & antiforgery token invalid sau redeploy (OBS-M20-07).

## 6. Roles & Permissions
- Không thêm permission mới; M20 dựa toàn bộ vào ma trận RBAC đã kiểm ở M02.

## 7. Risk Analysis (tổng hợp — observations chi tiết ở 06)

| # | Rủi ro | Mức | Trạng thái |
|---|---|---|---|
| OBS-M20-01 | Thiếu CSP header | Low-Med | hardening prod |
| OBS-M20-02 | ForwardedHeaders tin mọi proxy | Med (direct) / Low (tunnel) | cấu hình KnownProxies ở prod |
| OBS-M20-03 | Swagger public ở Production | Low | gate `IsDevelopment()` |
| OBS-M20-04 | Không rate limit (API/login/Gemini) | Med | login có lockout; API/Gemini chưa |
| OBS-M20-05 | JWT không revoke (no stamp check) | Med | 4h expiry; cookie path OK |
| OBS-M20-06 | `AllowedHosts="*"` | Low | siết theo domain prod |
| OBS-M20-07 | Data Protection keys in-memory | Low-Med | persist khi lên prod/multi-instance |
| OBS-M20-08 | Container chạy root | Low | thêm USER non-root |
| OBS-M20-09 | Gemini key tracked ở appsettings.Development.json | Med (nếu push public) | không push repo public (memory) |
| OBS-M20-10 | Không CORS (API same-origin) | Info | restrictive = an toàn; note cho consumer |

## 8. Unknowns / Needs Requirement Clarification

- **U-M20-1:** Danh mục hardening nào **bắt buộc trước production thật** (CSP, rate limit, non-root, KnownProxies, Data Protection persist, JWT revoke, gate Swagger)? Hiện giai đoạn TEST (Cloudflare Tunnel) chấp nhận hoãn — cần user xác nhận checklist go-live.
- **U-M20-2:** JWT có cần revoke tức thì khi khóa user/đổi role (thêm security-stamp check cho Bearer) hay chấp nhận 4h expiry?
