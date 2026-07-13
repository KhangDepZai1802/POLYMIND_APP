# M01 — Authentication & Session · Business Flows

## BF-M01-01 — Đăng nhập web thành công

- **Actor/Role:** mọi role · **Preconditions:** tài khoản tồn tại, `IsActive=true`, không bị lockout · **Initial state:** chưa đăng nhập.
- **Input:** Email hợp lệ + mật khẩu đúng.
- **Main flow:** `/login` → submit → `FindByEmailAsync` → kiểm `IsActive` → `CheckPasswordSignInAsync(lockoutOnFailure:true)` Succeeded → `SignInAsync(isPersistent:true)` → set `LastLoginAt=UtcNow` → `UpdateAsync` → redirect `ReturnUrl ?? "/"` (forceLoad).
- **Validation:** DataAnnotations (Required, EmailAddress). **Authorization:** AllowAnonymous.
- **DB changes:** `last_login_at`, reset `access_failed_count=0` (Identity khi thành công). **Notification:** không. **Audit/history:** không có audit đăng nhập (chỉ LastLoginAt).
- **Final state:** có cookie phiên, claim permission gắn qua `PermissionClaimsPrincipalFactory`.
- **Page/API:** `/login`. **Source:** Login.razor:77-108.
- **Risk:** không ghi audit login (không phát hiện đăng nhập bất thường). **Unknown:** —

## BF-M01-02 — Đăng nhập sai mật khẩu / khóa tạm (lockout)

- **Main flow:** sai mật khẩu → `CheckPasswordSignInAsync` tăng `access_failed_count`. Đủ 5 lần → `lockout_end = now+15'`, các lần sau `IsLockedOut` → hiển thị "Tài khoản tạm khóa ... thử lại sau 15 phút".
- **Alternate:** sau 15' hết lockout, đăng nhập đúng → reset count.
- **Error flow:** email không tồn tại → "Email hoặc mật khẩu không đúng" (không tăng count vì user null, không tới CheckPassword).
- **Risk:** BF trả thông báo khác nhau cho "khóa vĩnh viễn IsActive=false" vs "sai mật khẩu" → enumeration (BUG_M01_02).
- **Source:** Login.razor:92-107, DependencyInjection.cs:35-37.

## BF-M01-03 — Đăng nhập tài khoản bị vô hiệu hóa (IsActive=false)

- **Preconditions:** admin đã khóa (IsActive=false).
- **Main flow:** `/login` submit → `FindByEmailAsync` OK → `if (!user.IsActive)` → "Tài khoản đang bị khóa..." → KHÔNG gọi CheckPassword.
- **Final state:** không cấp phiên. **Đúng.**
- **Rủi ro:** thông báo phân biệt với sai mật khẩu (enumeration).

## BF-M01-04 — Đăng nhập REST API cấp JWT

- **Main flow:** `POST /api/auth/login` → validate field → FindByEmail → `!IsActive` → 403 → `CheckPasswordSignInAsync` → IsLockedOut→423 / !Succeeded→401 / Succeeded → `JwtTokenService.CreateAsync` (role + permission claims, exp 240') → cập nhật LastLoginAt → 200 `TokenResponse`.
- **DB changes:** `last_login_at`, access_failed_count. **Source:** AuthEndpoints.cs:15-47, JwtTokenService.cs.
- **Risk:** JWT không thu hồi được trong 240'.

## BF-M01-05 — Đăng xuất

- **Main flow:** `POST /Account/Logout` → xóa `AiSessionStore` theo UserId (RB-5) → `SignInManager.SignOutAsync()` → redirect `/login`.
- **DB changes:** không (cookie bị xóa). **Source:** Program.cs:244-252.
- **Final state:** cookie xóa; các circuit Blazor mất auth ở lần revalidate.

## BF-M01-06 — Tự động kiểm tra lại phiên (security stamp)

- **Main flow:** mỗi 30' `IdentityRevalidatingAuthenticationStateProvider` gọi `ValidateSecurityStampAsync`. Nếu `security_stamp` cookie ≠ DB → phiên bị coi là không hợp lệ → đăng xuất.
- **Kích hoạt đổi stamp:** đổi mật khẩu, đổi role (AddToRole/RemoveFromRole gọi `UpdateSecurityStampInternal`). **KHÔNG kích hoạt:** `UserManager.UpdateAsync` đơn thuần (đổi IsActive) → **lỗ hổng BUG_M01_01**.

## Bảng trạng thái phiên (session state)

| Current State | Action | Allowed | Condition | Next State | DB Change | Notification | History |
|---|---|---|---|---|---|---|---|
| Anonymous | Login đúng | any | IsActive & !locked | Authenticated (cookie) | last_login_at | — | — |
| Anonymous | Login sai ×5 | any | — | Locked 15' | access_failed_count, lockout_end | — | — |
| Authenticated | Admin khóa (IsActive=false) | admin | — | **VẪN Authenticated tới hết cookie (SAI — kỳ vọng: Revoked)** | is_active | — | audit lock (M03) |
| Authenticated | Admin đổi role | admin | — | Re-validated ≤30' (stamp đổi) | user_roles, security_stamp | — | audit update_role |
| Authenticated | Đổi mật khẩu | self/admin | — | Re-validated ≤30' (stamp đổi) | password_hash, security_stamp | — | — |
| Authenticated | Logout | self | — | Anonymous | — | — | — |

**Kiểm tra vấn đề điển hình:**
- Trạng thái không thể đi tới: `Locked 15'` tự phục hồi sau 15' — OK.
- Thao tác trái quyền: N/A cấp module này (thuộc M02).
- Hai người đổi cùng lúc: đổi mật khẩu đồng thời → concurrency_stamp Identity xử lý.
- Refresh/double click submit: form SSR POST — double submit tạo 2 lần SignIn (vô hại, cùng cookie).
- Notification sai người / badge: N/A.
- Lịch sử thiếu: **KHÔNG có audit cho đăng nhập/đăng xuất** (chỉ LastLoginAt) — rủi ro truy vết, không phải bug chức năng.
