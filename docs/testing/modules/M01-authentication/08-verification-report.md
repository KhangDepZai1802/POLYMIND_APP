# M01 — Authentication & Session · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic; chỉ đọc source, chạy test, đánh giá.
> **Ngày:** 2026-07-10 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit; runtime multi-session pending harness).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M01_01 High, BUG_M01_02 Low) | ✔ |
| `07-fix-report.md` | ✔ |
| `AuthenticationSecurityPolicy.cs` (mới) | ✔ |
| `IdentityRevalidatingAuthenticationStateProvider.cs` | ✔ |
| `AccountManagerPanel.razor` `ToggleUserAsync` | ✔ |
| `ParentAccountDialog.razor` / `StudentAccountDialog.razor` `Unlink` | ✔ |
| `Login.razor` `LoginUser` | ✔ |
| `AuthEndpoints.cs` `/api/auth/login` | ✔ |
| `M01_AuthenticationSecurityPolicyTests.cs` | ✔ |
| **Rà soát toàn source** `IsActive = false` (mọi caller) | ✔ |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 29, Skipped: 0
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M01_01 — Khóa tài khoản không chấm dứt phiên đang mở

**Kết luận: Verified Fixed (code-level).** Runtime multi-session HTTP còn chờ harness — không tuyên bố revoke tức thời, chỉ ≤30' theo chu kỳ revalidation đã chốt.

### Bằng chứng đã kiểm

1. **Defense chính (bao phủ MỌI đường khóa):** `IdentityRevalidatingAuthenticationStateProvider.ValidateSecurityStampAsync` nay đọc `user.IsActive` tươi từ DB rồi gọi `AuthenticationSecurityPolicy.IsSessionValid(user.IsActive, …)`. Hàm trả `isActive && (…stamp…)` → `IsActive=false` ⇒ phiên invalid bất kể stamp. Xác minh bằng unit test `Inactive_account_session_is_invalid_even_when_security_stamp_matches` (Passed).
2. **Hardening đa thiết bị:** `AccountManagerPanel.ToggleUserAsync` khi khóa dùng `UpdateSecurityStampAsync` (lưu đồng thời `IsActive=false` + stamp mới); mở khóa dùng `UpdateAsync`. Có nhánh lỗi `result.Succeeded` và audit `lock`/`unlock`.
3. Hai dialog gỡ liên kết `ParentAccountDialog`/`StudentAccountDialog` cũng `IsActive=false` + `UpdateSecurityStampAsync`, có xử lý lỗi Identity (không tiếp tục nếu lock fail).
4. **Không revoke oan khi sửa thường:** đổi họ tên/SĐT/role không đi qua đường đổi stamp; unit matrix stamp-match vẫn `valid`.
5. `IsSessionValid` giữ đúng trường hợp store không hỗ trợ stamp: `isActive && true` — active vẫn hợp lệ (test `Active_account_without_security_stamp_support_remains_valid` Passed).

### Không tìm thấy hành vi né bug
- Codex **không** sửa expected result để hợp thức hóa; test kiểm ĐÚNG bản chất (inactive ⇒ invalid).
- Không hard-code, không tắt validation/authorization.

### Residual risk (đo lường được)
- Runtime chưa chứng minh mốc ≤30' bằng hai phiên cookie thật (cần WebApplicationFactory + DB test). Logic đủ căn cứ ở source + unit; timing phụ thuộc `RevalidationInterval = 30'`.
- JWT đã cấp vẫn stateless tới hạn — nằm ngoài phạm vi BUG_M01_01 (ghi ở M20/M01 backlog).

---

## BUG_M01_02 — Account enumeration qua thông báo/HTTP status

**Kết luận: Verified Fixed (code-level).** Đúng quyết định bảo mật user chốt 2026-07-10.

### Bằng chứng đã kiểm
1. `AuthenticationSecurityPolicy.InvalidCredentialsMessage = "Email hoặc mật khẩu không đúng."` — không chứa "khóa"/"tồn tại" (test `Public_failure_message_does_not_reveal_lock_state` Passed).
2. `Login.LoginUser`: cả nhánh `user is null || !user.IsActive` (dòng 80-84) và nhánh sai mật khẩu (dòng 96) đều gán CÙNG `InvalidCredentialsMessage`. Không còn chuỗi "Tài khoản đang bị khóa".
3. `AuthEndpoints`: email không tồn tại/`!IsActive` (dòng 25-26) và `!result.Succeeded` (dòng 30-31) đều trả `AuthenticationFailed()` = HTTP **401** đồng nhất. Không còn 403/423 phân biệt.
4. **Không làm yếu lockout:** cả web và API vẫn `CheckPasswordSignInAsync(…, lockoutOnFailure: true)` → `access_failed_count`/`lockout_end` vẫn tăng/set nội bộ dù phản hồi chung.
5. Thiếu field vẫn **400** (`BadRequest` "Email và mật khẩu là bắt buộc.") — đúng vì là validation contract, không phải oracle tra cứu tài khoản.

### Residual risk
- Timing side-channel (constant-time / rate-limit) chưa xử lý — chuyển M20 theo ghi chú fix report. Không thuộc phạm vi bug này.
- Runtime HTTP matrix (401 body giống nhau cho 4 trường hợp thất bại) chờ harness; logic đã xác minh ở source + unit.

---

## Phát hiện mới khi rà soát (regression sweep)

### BUG_M01_03 — `PartnerAccountDialog.Unlink` khóa tài khoản không xoay security stamp (consistency, Low, **non-blocking**)

- Rà `IsActive = false` toàn source phát hiện `src/Polymind.Web/Components/Pages/Agents/PartnerAccountDialog.razor:180` khi gỡ liên kết Agent/CTV vẫn `user.IsActive = false; await UserManager.UpdateAsync(user);` — **không** dùng `UpdateSecurityStampAsync` như 3 caller Codex đã sửa.
- **Vì sao KHÔNG mở lại BUG_M01_01 (High):** defense chính (revalidation kiểm `IsActive`) bao phủ luôn đường này → phiên cookie của partner/agent/CTV bị khóa vẫn bị vô hiệu ≤30'. Không có defect "phiên sống tới 8h". Phần thiếu chỉ là hardening xoay stamp đa thiết bị (dư thừa khi đã có kiểm `IsActive`).
- **Severity:** Low (consistency/hardening). Đã đưa vào `06-bug-report.md` + Codex Handoff Queue, **không chặn** verify M01.

---

## Xác minh bản sửa BUG_M01_03 (Codex Fixed → Claude verify) — 2026-07-11

**Kết luận: Verified Fixed (code-level).** Runtime partner multi-session (đăng nhập Agent/CTV rồi gỡ liên kết) còn chờ harness — không tuyên bố revoke tức thời, chỉ ≤30' theo chu kỳ revalidation.

### Bằng chứng đã kiểm

1. **Partner unlink nay xoay stamp:** `PartnerAccountDialog.Unlink` (dòng 177-190) khi có linked user set `user.IsActive = false` rồi `var lockResult = await UserManager.UpdateSecurityStampAsync(user);` — KHÔNG còn `UpdateAsync`. Khớp đúng pattern `ParentAccountDialog`/`StudentAccountDialog` đã Claude-verify ở BUG_M01_01.
2. **Xử lý lỗi Identity trước commit:** nếu `!lockResult.Succeeded` → hiển thị lỗi + `return` TRƯỚC `db.SaveChangesAsync()` (dòng 191). Vì `agent.UserId=null`/`ctv.UserId=null` + audit chỉ được stage trong `db` (chưa lưu), early-return + `await using db` dispose ⇒ unlink/audit KHÔNG commit giả thành công.
3. **Defense chính vẫn phủ:** revalidation provider kiểm `IsActive` tươi → phiên partner bị khóa vô hiệu ≤30' bất kể stamp; stamp rotation là hardening đa thiết bị (đồng nhất 4 caller khóa runtime).
4. **Không mở lại BUG_M01_01/02:** không đổi provider/policy/login; chỉ sửa 1 file `PartnerAccountDialog.razor`. Suite 52/52, Web build 0/0.

### Residual risk (đo lường được)
- Two-transaction pre-existing: `UpdateSecurityStampAsync` (context UserManager) commit ngay, nếu business `SaveChangesAsync` fail sau đó user có thể bị khóa trong khi link còn — rủi ro Low pre-existing, cùng lớp Parent/Student, ngoài phạm vi bug Low.
- Runtime partner cookie/stamp chưa đo bằng harness multi-session.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M01_01 | High | **Verified Fixed** (code-level; runtime ≤30' pending harness) |
| BUG_M01_02 | Low | **Verified Fixed** (code-level; runtime HTTP matrix pending harness) |
| BUG_M01_03 | Low | **Verified Fixed** (code-level; partner stamp rotation + error handling; runtime multi-session pending harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed (BUG_M01_01/02/03)
- **Verification Status:** Verified (code-level) — runtime multi-session/HTTP matrix ghi rõ là chưa đo, không tuyên bố 100%.
