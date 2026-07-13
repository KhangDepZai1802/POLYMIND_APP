# Module Fix Report

## Summary

- **Module ID:** M01
- **Module Name:** Authentication & Session
- **Bugs Received:** 3
- **Bugs Fixed:** 3
- **Cannot Reproduce:** 0
- **Blocked:** 0 bug; runtime multi-session/API verification chờ integration harness
- **Needs Clarification:** 0 (user đã chốt cả hai quyết định)

## BUG_M01_01

### Status

- Fixed

### Investigation

Đã kiểm tra luồng khóa ở `AccountManagerPanel`, Identity user store, cookie config 8 giờ, provider revalidation 30 phút, các luồng đổi role/password và endpoint logout. `ToggleUserAsync` trước đây chỉ gọi `UpdateAsync`, còn provider chỉ so stamp; vì vậy `IsActive=false` không tác động cookie đã cấp.

### Root Cause

Trạng thái khóa ứng dụng (`ApplicationUser.IsActive`) không thuộc validation mặc định của security stamp. Hành động khóa không đổi stamp và provider tùy chỉnh cũng không đọc `IsActive`.

### Evidence

- Trước sửa: `ToggleUserAsync` đảo `IsActive` rồi gọi `UserManager.UpdateAsync`.
- `IdentityRevalidatingAuthenticationStateProvider` trước sửa trả kết quả chỉ từ phép so security stamp.
- Cookie có sliding expiry 8 giờ; revalidation chạy mỗi 30 phút.
- Source và cấu hình đủ tái hiện root cause dù chưa có harness hai phiên.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor`
- `src/Polymind.Web/Components/Pages/Candidates/ParentAccountDialog.razor`
- `src/Polymind.Web/Components/Pages/Candidates/StudentAccountDialog.razor`
- `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
- `src/Polymind.Web/Components/Pages/Admin/UserEditDialog.razor`
- `src/Polymind.Web/Components/Account/Login.razor`
- `src/Polymind.Web/Program.cs`
- `src/Polymind.Infrastructure/Identity/ApplicationUser.cs`
- `src/Polymind.Infrastructure/DependencyInjection.cs`

### Files Changed

- `src/Polymind.Infrastructure/Identity/AuthenticationSecurityPolicy.cs`
- `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor`
- `src/Polymind.Web/Components/Pages/Candidates/ParentAccountDialog.razor`
- `src/Polymind.Web/Components/Pages/Candidates/StudentAccountDialog.razor`
- `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
- `tests/Polymind.Tests/M01_AuthenticationSecurityPolicyTests.cs`

### Symbols Changed

- `AuthenticationSecurityPolicy.IsSessionValid`
- `AccountManagerPanel.ToggleUserAsync`
- `IdentityRevalidatingAuthenticationStateProvider.ValidateSecurityStampAsync`

### Fix

Khi khóa từ panel hoặc từ hai dialog gỡ liên kết, `UpdateSecurityStampAsync` lưu đồng thời `IsActive=false` và stamp mới bằng một Identity update. Provider revalidation dùng quy tắc chung: tài khoản inactive luôn invalid; tài khoản active cần stamp khớp nếu store hỗ trợ stamp. Lỗi Identity ở dialog gỡ link được hiển thị và không tiếp tục bỏ liên kết giả thành công.

### Why This Fix Is Correct

Fix nối hành động BF-M03-03 với cơ chế revoke BF-M01-06 sẵn có. Session bị vô hiệu trong chu kỳ đã chốt ≤30 phút; mở khóa không hồi sinh cookie cũ vì stamp đã đổi. Không ảnh hưởng đổi thông tin thông thường và không làm yếu authorization.

### Alternatives Considered

- Chỉ kiểm `IsActive`: đúng sau 30 phút nhưng thiếu invalidation stamp trên mọi thiết bị.
- Chỉ đổi stamp: hoạt động với store hiện tại nhưng bỏ qua defense-in-depth của trạng thái khóa.
- Giảm revalidation interval toàn hệ thống: tăng DB load và vượt phạm vi bug.

### Impact

- **API:** JWT đã cấp vẫn stateless tới expiry; bug này sửa phiên cookie được mô tả trong TC_M01_020.
- **Database:** cập nhật `is_active`, `security_stamp`, `concurrency_stamp`; không migration.
- **UI:** hành động khóa/mở và audit giữ nguyên.
- **Security:** cookie của tài khoản bị khóa không còn hợp lệ quá 30 phút.
- **Backward compatibility:** mọi thiết bị của user bị khóa phải đăng nhập lại sau khi mở.
- **Data compatibility:** không đổi schema hoặc dữ liệu nghiệp vụ.

### Regression Risks

- User bị khóa trên nhiều thiết bị đều bị revoke — đúng quyết định nghiệp vụ.
- Runtime timing vẫn phụ thuộc chu kỳ revalidation 30 phút; không tuyên bố revoke tức thời.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `Inactive_account_session_is_invalid_even_when_security_stamp_matches` | Unit regression | Passed | BUG_M01_01 / TC_M01_020 |
| Session policy active/stamp matrix | Unit regression | Passed 4/4 | Active/no-stamp, match, mismatch, missing claim |
| Toàn bộ `Polymind.Tests` | Regression | Passed 22/22 | Failed 0, Skipped 0 |
| Build `Polymind.Web` ra `C:\tmp\polymind-codex-build` | Compile | Passed | 0 warning, 0 error |
| Hai phiên cookie + DB stamp | Integration/manual | Blocked | Chưa có harness/test DB |

### Test Results

- **Passed:** 22
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime revalidation nhiều phiên

### Verification Instructions for Claude

1. Đăng nhập user X ở trình duyệt A; ở B dùng super admin khóa X.
2. Xác nhận `is_active=false` và `security_stamp` đổi sau đúng một thao tác khóa.
3. Trong tối đa 30 phút, phiên A bị từ chối/đưa về login; đăng nhập mới bị chặn.
4. Mở khóa X và xác nhận cookie cũ không tự sống lại; đăng nhập mới thành công.
5. Sửa riêng họ tên user khác và xác nhận không bị revoke ngoài ý muốn.

## BUG_M01_02

### Status

- Fixed

### Investigation

Đã đối chiếu mọi nhánh thất bại của login SSR và REST: missing user, inactive, sai password, Identity lockout, validation thiếu field. Các nhánh trước sửa trả chuỗi và mã 401/403/423 khác nhau.

### Root Cause

Web và API tự định nghĩa phản hồi theo từng trạng thái nội bộ thay vì dùng một public authentication failure contract.

### Evidence

- Web trước sửa có ba chuỗi riêng cho missing/wrong, inactive và lockout.
- API trước sửa trả 401, 403 hoặc 423 tương ứng.
- Sau sửa, `rg` không còn chuỗi/mã tiết lộ khóa trong hai entry point.

### Files Inspected

- `src/Polymind.Web/Components/Account/Login.razor`
- `src/Polymind.Web/Api/AuthEndpoints.cs`
- `src/Polymind.Web/Api/JwtTokenService.cs`
- `src/Polymind.Infrastructure/DependencyInjection.cs`

### Files Changed

- `src/Polymind.Infrastructure/Identity/AuthenticationSecurityPolicy.cs`
- `src/Polymind.Web/Components/Account/Login.razor`
- `src/Polymind.Web/Api/AuthEndpoints.cs`
- `tests/Polymind.Tests/M01_AuthenticationSecurityPolicyTests.cs`

### Symbols Changed

- `AuthenticationSecurityPolicy.InvalidCredentialsMessage`
- `Login.LoginUser`
- `AuthEndpoints.AuthenticationFailed`

### Fix

Web và API dùng cùng một thông báo chung. Missing user, inactive, bad password và lockout đều không tiết lộ trạng thái; API trả 401 đồng nhất. Dữ liệu đầu vào thiếu vẫn trả 400 vì đây là validation contract, không phải kết quả tra cứu tài khoản.

### Why This Fix Is Correct

Đúng quyết định bảo mật user đã chốt cho BF-M01-02/BF-M01-03 và TC_M01_006. Lockout Identity vẫn được kích hoạt/lưu nội bộ qua `CheckPasswordSignInAsync(lockoutOnFailure:true)`; chỉ phản hồi công khai được chuẩn hóa.

### Alternatives Considered

- Báo khóa sau khi password đúng: UX tốt hơn nhưng vẫn tiết lộ tài khoản/trạng thái; user đã chọn phản hồi chung.
- Giữ 403/423 và chỉ đổi message: status code vẫn là oracle.

### Impact

- **API:** inactive/lockout đổi từ 403/423 sang 401; success và validation 400 giữ nguyên.
- **Database:** lockout counter/end vẫn hoạt động; không migration.
- **UI:** chỉ còn lỗi xác thực chung.
- **Security:** loại bỏ oracle trực tiếp bằng message/status.
- **Backward compatibility:** client dựa vào 403/423 cần chuyển sang xử lý 401 chung theo quyết định mới.
- **Data compatibility:** không đổi dữ liệu.

### Regression Risks

- Người dùng không còn biết trực tiếp đang bị admin khóa hay lockout; liên hệ quản trị theo dòng hỗ trợ sẵn trên login.
- Timing side-channel không nằm trong bug này; cần rate limiting/constant-time hardening ở M20 nếu yêu cầu.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `Public_failure_message_does_not_reveal_lock_state` | Unit regression | Passed | BUG_M01_02 / TC_M01_006 |
| Toàn bộ `Polymind.Tests` | Regression | Passed 22/22 | M01 config/lockout policy vẫn pass |
| Build Web | Compile | Passed | 0 warning, 0 error |
| HTTP response matrix | Integration | Blocked | Chưa có WebApplicationFactory + DB test |

### Test Results

- **Passed:** 22 + build
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime SSR/API matrix

### Verification Instructions for Claude

1. Gửi cùng mật khẩu sai cho email không tồn tại, active, inactive và đang lockout.
2. Web phải hiển thị đúng cùng một chuỗi; API phải trả cùng 401/body.
3. Xác nhận lần sai trên tài khoản active vẫn tăng `access_failed_count` và đủ 5 lần vẫn set `lockout_end` dù phản hồi chung.
4. Xác nhận request thiếu email/password vẫn 400 và login đúng vẫn cấp cookie/JWT, cập nhật `LastLoginAt` UTC.
5. Đối chiếu quyết định user ngày 2026-07-10 khi cập nhật các test case UX cũ; Codex không đánh dấu Verified Fixed.

## BUG_M01_03

### Status

- Fixed — chờ Claude xác minh độc lập.

### Investigation

Đã đọc lại đầy đủ M01 `01`–`08`, đặc biệt verification sweep của Claude. Đối chiếu `PartnerAccountDialog.Unlink` với ba caller đã được Claude xác minh trong BUG_M01_01 (`AccountManagerPanel`, ParentAccountDialog, StudentAccountDialog). Partner path vẫn dùng `UpdateAsync` và bỏ qua `IdentityResult`, trong khi các path đúng dùng `UpdateSecurityStampAsync` và dừng unlink nếu lock thất bại.

### Root Cause

Partner unlink được thêm sau batch fix M01 ban đầu và sao chép pattern khóa cũ: chỉ lưu `IsActive=false`, không rotate security stamp và không xử lý lỗi Identity.

### Evidence

- `08-verification-report.md` xác nhận defense chính `IsActive` vẫn revoke phiên ≤30 phút, nên bug là hardening/consistency Low chứ không mở lại BUG_M01_01.
- Source trước sửa: `user.IsActive = false; await UserManager.UpdateAsync(user);`.
- Source sau sửa: `UpdateSecurityStampAsync(user)` + kiểm `Succeeded`; lỗi được hiển thị và return trước `db.SaveChangesAsync` unlink.
- Sweep sau sửa: bốn runtime lock caller đều rotate stamp; demo seeder không phải user action.

### Files Inspected

- `docs/testing/modules/M01-authentication/01-analysis.md` → `08-verification-report.md`
- `src/Polymind.Web/Components/Pages/Agents/PartnerAccountDialog.razor`
- `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor`
- `src/Polymind.Web/Components/Pages/Candidates/ParentAccountDialog.razor`
- `src/Polymind.Web/Components/Pages/Candidates/StudentAccountDialog.razor`
- `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
- `src/Polymind.Infrastructure/Identity/AuthenticationSecurityPolicy.cs`
- `tests/Polymind.Tests/M01_AuthenticationSecurityPolicyTests.cs`

### Files Changed

- `src/Polymind.Web/Components/Pages/Agents/PartnerAccountDialog.razor`

### Symbols Changed

- `PartnerAccountDialog.Unlink`

### Fix

Khi Agent/CTV có linked user, Unlink đặt `IsActive=false` rồi gọi `UserManager.UpdateSecurityStampAsync`. Nếu Identity trả lỗi, dialog hiển thị lỗi và không lưu việc gỡ liên kết/audit giả thành công. Nếu thành công, liên kết mới được null và audit được commit như trước.

### Why This Fix Is Correct

- Khớp BF-M01-06/TC_M01_020 biến thể partner và pattern Parent/Student đã được Claude verify.
- Security stamp mới invalidates cookie stamp trên mọi thiết bị; provider vẫn kiểm trực tiếp `IsActive` làm defense chính.
- Không thay đổi permission, role link hay behavior tạo/reset password.

### Alternatives Considered

- Giữ `UpdateAsync` vì provider đã kiểm IsActive: hành vi cơ bản đúng nhưng bỏ defense-in-depth và không nhất quán.
- Chỉ đổi method mà bỏ kiểm `IdentityResult`: có thể báo unlink thành công dù account lock thất bại.
- Refactor bốn caller sang service chung: ngoài phạm vi bug Low hiện tại.

### Impact

- **API/database schema:** không đổi; cập nhật `is_active`, `security_stamp`, `concurrency_stamp` qua Identity.
- **UI:** chỉ thêm hiển thị lỗi nếu lock thất bại.
- **Security:** hardening revoke đa thiết bị; không làm yếu authorization.
- **Backward/data compatibility:** không migration, không sửa user lịch sử.

### Regression Risks

- Giống Parent/Student: Identity update và unlink business row dùng hai DbContext/transaction; nếu business save thất bại sau lock, user có thể bị khóa trong khi link còn. Đây là rủi ro hai-transaction pre-existing, không mở rộng trong bug Low.
- Runtime revoke timing vẫn ≤30 phút theo revalidation interval, không tuyên bố tức thời.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| M01 session/stamp policy matrix | Unit regression | Passed | Inactive invalid; active requires matching stamp |
| Shared test suite | Regression | Passed | 52 passed, 0 failed, 0 skipped |
| Web build output riêng | Build | Passed | 0 warning, 0 error |
| Partner multi-session unlink | UI/integration | Blocked | Chưa có bUnit/WebApplicationFactory multi-session harness |

### Test Results

- **Passed:** 52 + Web build.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** runtime partner cookie/stamp verification.

### Verification Instructions for Claude

1. Chạy lại test suite và Web/solution build.
2. Tạo/gắn tài khoản Agent hoặc CTV, ghi lại `security_stamp`, đăng nhập tài khoản đó.
3. Admin bấm “Gỡ liên kết & khóa”; xác nhận `is_active=false`, `security_stamp` đổi và Agent/CTV `UserId=null`.
4. Phiên partner bị invalid trong chu kỳ revalidation ≤30 phút; login mới bị chặn.
5. Giả lập lỗi Identity store; xác nhận dialog báo lỗi và không commit unlink/audit thành công.
6. Không kiểm lại/đổi verdict BUG_M01_01/02 trừ khi có regression mới; BUG_M01_03 cần verdict riêng.
