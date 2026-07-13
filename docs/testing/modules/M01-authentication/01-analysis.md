# M01 — Authentication & Session · Phân tích

## 1. Module Overview

- **Module ID:** M01
- **Module name:** Authentication & Session (Đăng nhập / Đăng xuất / Phiên)
- **Business purpose:** Xác thực người dùng (email + mật khẩu), cấp phiên (cookie Blazor Server) hoặc JWT (REST API), khóa tạm khi sai mật khẩu nhiều lần, chấm dứt phiên khi đăng xuất, tự động kiểm tra lại phiên theo security stamp.
- **Actor:** Mọi người dùng nội bộ + đối tác (đại lý/CTV/phụ huynh/học viên) + tích hợp ngoài (mobile/đối tác qua JWT).
- **Role:** Không phân biệt (cổng vào chung); phân quyền là M02.
- **Dependencies:** ASP.NET Core Identity, PostgreSQL (bảng `asp_net_users`, `asp_net_user_roles`), `PermissionClaimsPrincipalFactory` (M02) để gắn claim khi đăng nhập cookie.
- **Entry point:** `GET /login` (web), `POST /api/auth/login` (API).
- **Exit point:** Redirect `/` (hoặc `ReturnUrl`) sau login web; `POST /Account/Logout` → `/login`; JWT hết hạn (240').

## 2. Source Code Map

| # | File | Symbol | Method | Mục đích | Dependency |
|---|---|---|---|---|---|
| 1 | [Login.razor](../../../../src/Polymind.Web/Components/Account/Login.razor) | `Login` | `LoginUser()` | Form đăng nhập web: kiểm IsActive → CheckPassword (lockout) → SignIn cookie → cập nhật LastLoginAt | `SignInManager`, `UserManager`, `NavigationManager` |
| 2 | [AuthEndpoints.cs](../../../../src/Polymind.Web/Api/AuthEndpoints.cs) | `AuthEndpoints` | `POST /api/auth/login`, `GET /api/auth/me` | Đăng nhập API cấp JWT; xem thông tin tài khoản theo JWT | `SignInManager`, `UserManager`, `JwtTokenService` |
| 3 | [JwtTokenService.cs](../../../../src/Polymind.Web/Api/JwtTokenService.cs) | `JwtTokenService` | `CreateAsync(user)` | Sinh JWT HS256 mang role + permission claim (240') | `UserManager`, `IDbContextFactory`, `JwtOptions` |
| 4 | [IdentityRevalidatingAuthenticationStateProvider.cs](../../../../src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs) | `IdentityRevalidatingAuthenticationStateProvider` | `ValidateAuthenticationStateAsync` | Định kỳ 30' kiểm security stamp → tự đăng xuất nếu stamp đổi | `UserManager`, `IOptions<IdentityOptions>` |
| 5 | [PermissionClaimsPrincipalFactory.cs](../../../../src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs) | `PermissionClaimsPrincipalFactory` | `GenerateClaimsAsync` | Khi đăng nhập cookie, nạp permission claim từ RolePermission (M02) | `ApplicationDbContext` |
| 6 | [Program.cs](../../../../src/Polymind.Web/Program.cs) | — | `ConfigureApplicationCookie`, `AddJwtBearer`, `/Account/Logout` | Cấu hình cookie (8h sliding, HttpOnly, Lax, Secure prod), JWT bearer, endpoint logout (xóa AI session RB-5) | Identity, JwtBearer |
| 7 | [DependencyInjection.cs](../../../../src/Polymind.Infrastructure/DependencyInjection.cs) | `AddInfrastructure` | — | Cấu hình Identity: password ≥8 (digit/upper/lower), unique email, lockout 5 lần/15', token providers | Identity, EF Core |
| 8 | [ApplicationUser.cs](../../../../src/Polymind.Infrastructure/Identity/ApplicationUser.cs) | `ApplicationUser` | — | `IdentityUser<Guid>` + `FullName`, `IsActive`, `LastLoginAt`, `CreatedAt` | — |

## 3. UI Inventory

- **Page:** `/login` (layout `EmptyLayout`, `[AllowAnonymous]`, `[ExcludeFromInteractiveRouting]` → SSR để set cookie).
- **Form:** `EditForm FormName="login"` method POST, `DataAnnotationsValidator`.
- **Field:** Email (`[Required]`,`[EmailAddress]`), Password (`[Required]`, type=password + nút hiện/ẩn bằng JS thuần).
- **Button:** "Đăng nhập →" (submit).
- **Error state:** `_error` hiển thị div `.login-error` (email/mật khẩu sai, tài khoản khóa, lockout).
- **Loading/empty state:** không có (form tĩnh SSR).
- **Logout:** form POST `/Account/Logout` (đặt trong MainLayout — cần verify ở M03).

## 4. API Inventory

| Method | Route | Request | Response | Auth | Authorization | Validation | DB side effect | Notification | Error |
|---|---|---|---|---|---|---|---|---|---|
| POST | `/api/auth/login` | `LoginRequest(Email,Password)` | `TokenResponse(AccessToken,Bearer,ExpiresAt,UserInfo)` | AllowAnonymous | — | Email+Password bắt buộc | Cập nhật `LastLoginAt`; tăng AccessFailedCount khi sai | — | 400 thiếu field; 401 sai; 403 IsActive=false; 423 locked |
| GET | `/api/auth/me` | — (JWT header) | `UserInfo` | JWT Bearer | RequireAuthorization(Bearer) | — | — | — | 401 |
| POST | `/Account/Logout` | cookie | Redirect `/login` | cookie | — | — | SignOut + xóa AiSessionStore theo UserId | — | — |
| GET | `/login` | — | HTML | AllowAnonymous | — | DataAnnotations | — | — | inline `_error` |

## 5. Database Impact

- **Bảng:** `asp_net_users` (Identity). Cột nghiệp vụ mở rộng: `full_name`, `is_active`, `last_login_at`, `created_at`.
- **Cột Identity liên quan phiên/khóa:** `password_hash`, `security_stamp`, `concurrency_stamp`, `access_failed_count`, `lockout_end`, `lockout_enabled`, `email_confirmed`.
- **Audit field:** `last_login_at` cập nhật mỗi lần login thành công (web + API).
- **Concurrency:** `concurrency_stamp` (Identity). `security_stamp` là trục của revalidation 30'.
- **Không có** bảng session riêng (phiên = cookie ký, JWT stateless).

## 6. Roles & Permissions

| Action | Role | UI Permission | API Permission | Business Condition | Source |
|---|---|---|---|---|---|
| Đăng nhập | mọi role | AllowAnonymous | AllowAnonymous | `IsActive=true` + đúng mật khẩu + không bị lockout | Login.razor, AuthEndpoints |
| Xem `/api/auth/me` | mọi role đã đăng nhập | — | JWT hợp lệ | token chưa hết hạn | AuthEndpoints |
| Đăng xuất | mọi role | — | cookie | — | Program.cs |

## 7. Risk Analysis (đã đối chiếu source)

1. **[XÁC NHẬN — High] Khóa tài khoản KHÔNG chấm dứt phiên đang đăng nhập.** `AccountManagerPanel.ToggleUserAsync` set `IsActive=false` + `UserManager.UpdateAsync` nhưng KHÔNG gọi `UpdateSecurityStampAsync`. Revalidation (30') chỉ so security stamp → stamp không đổi → user bị khóa vẫn dùng phiên cookie tới khi hết hạn (tối đa 8h sliding). Chỉ chặn ĐĂNG NHẬP MỚI. → **BUG_M01_01**.
2. **[XÁC NHẬN — Low] Account enumeration.** Web: user null → "Email hoặc mật khẩu không đúng"; `IsActive=false` → "Tài khoản đang bị khóa". API: 401 vs 403 phân biệt tồn tại tài khoản. → **BUG_M01_02**.
3. **[Theo dõi — Info] JWT không có cơ chế thu hồi.** Token sống 240'; đổi mật khẩu/khóa tài khoản không vô hiệu JWT đã cấp (stateless, không check security stamp). Đúng bản chất JWT nhưng cần ghi nhận rủi ro; hiện API bề mặt nhỏ (login + read).
4. **[Theo dõi — Info] Dev JWT key hard-code** trong Program.cs (chỉ môi trường Development, production ném lỗi nếu thiếu `Jwt__Key`). Không phải secret production → chấp nhận cho dev.
5. **[Theo dõi — Low] `SignInAsync(isPersistent:true)`** luôn tạo cookie bền + `ExpireTimeSpan=8h` sliding → phiên có thể sống rất lâu nếu hoạt động liên tục. Không có "remember me" tùy chọn.
6. **Timezone:** `LastLoginAt = DateTimeOffset.UtcNow` (UTC offset 0) — đúng quy ước Postgres của dự án.
7. **Lockout:** cả web (`LoginUser`) và API (`/api/auth/login`) đều dùng `CheckPasswordSignInAsync(..., lockoutOnFailure:true)` → nhất quán 5 lần/15'. Tốt.

## 8. Unknowns / Cần làm rõ nghiệp vụ

- **U1:** Khi khóa tài khoản, nghiệp vụ có yêu cầu đá phiên đang mở NGAY không? (Giả định QA: có — vì đây là hành vi bảo mật kỳ vọng. Nếu Vietgroup chấp nhận trễ tới hết phiên thì hạ severity.) → `Needs Requirement Clarification` cho mức độ, nhưng thiếu sót kỹ thuật là rõ.
- **U2:** Chính sách "remember me" và thời lượng phiên mong muốn (8h?) chưa có tài liệu nghiệp vụ.
- **U3:** REST API JWT dùng cho đối tác nào, có cần refresh token / thu hồi không — chưa có tài liệu.
