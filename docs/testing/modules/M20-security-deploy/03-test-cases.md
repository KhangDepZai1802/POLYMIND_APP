# M20 — Security & Deployment · Test Cases

> Quy ước: `TC_M20_<NNN>`. Automation Layer: **Static** (đọc source/config), **Unit** (test project), **Manual/Runtime** (cần harness HTTP/deploy — Blocked). Không chạy destructive test; không production.

| TC | Tên | Type | Priority | Sev-if-fail | Layer | Expected | Actual/Status |
|---|---|---|---|---|---|---|---|
| TC_M20_001 | Cookie `HttpOnly` + `SameSite=Lax` + `Secure=Always` (non-dev) | Config | High | Med | Static | `Program.cs:110-114` set đủ | **Pass** (đọc source) |
| TC_M20_002 | Cookie hết hạn 8h sliding | Config | Med | Low | Static | `ExpireTimeSpan=8h`, `SlidingExpiration=true` | **Pass** |
| TC_M20_003 | JWT validate Issuer/Audience/Lifetime/SigningKey | Config | High | High | Static | `Program.cs:141-151` tất cả `true`, ClockSkew 1min | **Pass** |
| TC_M20_004 | Production thiếu `Jwt:Key` → throw khi khởi động | Config | High | High | Static | `Program.cs:125` throw | **Pass** |
| TC_M20_005 | Dev dùng JWT key dev-only rõ ràng | Config | Low | Low | Static | fallback key có chú thích dev-only | **Pass** |
| TC_M20_006 | Security headers nosniff/X-Frame/Referrer/Permissions | Config | Med | Med | Static | `Program.cs:197-201` | **Pass** |
| TC_M20_007 | Thiếu CSP header | Config | Med | Low-Med | Static | không set CSP | **Observation OBS-M20-01** |
| TC_M20_008 | HSTS + HttpsRedirection (non-dev) | Config | Med | Med | Static | `UseHsts()` non-dev + `UseHttpsRedirection()` | **Pass** |
| TC_M20_009 | CSRF antiforgery bật (Blazor) | Config | High | Med | Static | `UseAntiforgery()` | **Pass** |
| TC_M20_010 | Exception handler ẩn stack trace (prod) | Config | Med | Med | Static | `UseExceptionHandler("/Error")` prod | **Pass** |
| TC_M20_011 | Lockout 5 lần/15 phút | Config | High | Med | Static | `DependencyInjection.cs` lockout | **Pass** (khớp M01) |
| TC_M20_012 | Partner/portal KHÔNG có quyền nhạy cảm (finance/user/role/audit/commission) | Security | Critical | High | **Unit** | tất cả `false` | **Pass** (M20 test) |
| TC_M20_013 | Non-finance staff không có `financial_reports:read` | Security | High | Med | **Unit** | `false` | **Pass** (M20 test) |
| TC_M20_014 | Finance roles giữ `financial_reports:read` | Security | High | Med | **Unit** | `true` | **Pass** (M20 test) |
| TC_M20_015 | user/role admin không cấp cho non-admin | Security | High | High | **Unit** | `false` | **Pass** (M20 test) |
| TC_M20_016 | Role không tồn tại → fail-closed | Security | Med | Med | **Unit** | `false` | **Pass** (M20 test) |
| TC_M20_017 | `/hangfire` chỉ SuperAdmin/Director | AuthZ | High | High | Static | `HangfireDashboardAuthorizationFilter` | **Pass** (đọc source) |
| TC_M20_018 | `/swagger` public ở Production | Info-disc | Med | Low | Static | `UseSwagger` ngoài dev-guard | **Observation OBS-M20-03** |
| TC_M20_019 | REST `/api/*` fail-closed data-scope | AuthZ | High | High | Static | M02 đã Verified | **Pass** (tham chiếu M02) |
| TC_M20_020 | Không rate limit API/login/Gemini | DoS | Med | Med | Static | không `AddRateLimiter` | **Observation OBS-M20-04** |
| TC_M20_021 | Production KHÔNG seed tài khoản mẫu | Deploy | High | High | Static | `DbSeeder.cs:160-190` dev-only mẫu | **Pass** |
| TC_M20_022 | Production thiếu SuperAdmin env → không tạo account | Deploy | High | Med | Static | `LogError` + skip | **Pass** (không lộ default cred) |
| TC_M20_023 | Không secret trong appsettings tracked (trừ Gemini dev) | Secret | High | High | Static | Jwt/SMTP/Minio rỗng | **Pass** / OBS-M20-09 cho Gemini |
| TC_M20_024 | `DemoDataSeeder` chỉ chạy Development | Deploy | High | Med | Static | `Program.cs:281` gate | **Pass** |
| TC_M20_025 | ForwardedHeaders tin mọi proxy | Config | Med | Med(direct)/Low(tunnel) | Static | `KnownProxies.Clear()` | **Observation OBS-M20-02** |
| TC_M20_026 | `AllowedHosts="*"` | Config | Low | Low | Static | appsettings | **Observation OBS-M20-06** |
| TC_M20_027 | Data Protection keys in-memory | Deploy | Med | Low-Med | Static | mặc định | **Observation OBS-M20-07** |
| TC_M20_028 | Container non-root | Deploy | Low | Low | Static | Dockerfile chạy root | **Observation OBS-M20-08** |
| TC_M20_029 | JWT revoke khi khóa user/đổi role | AuthZ | Med | Med | Manual/Runtime | không kiểm stamp | **Observation OBS-M20-05 → U-M20-2** |
| TC_M20_030 | IDOR tổng hợp (web + REST + MinIO) | Security | High | High | Static | M02/M05/M18 đã Verified fail-closed | **Pass** (tham chiếu) |

## Ghi chú phạm vi
- TC static: xác minh bằng đọc source/config thực (không phỏng đoán).
- TC unit: `M20_SecurityInvariantsTests` (16 case) — chốt bất biến RBAC seed.
- TC Manual/Runtime (JWT revoke, header runtime thật, deploy prod): **Blocked** — chưa có WebApplicationFactory/HTTP harness + chưa deploy production thật. Ghi rõ ở 05.
