# M01 — Authentication & Session · Bug Report

Chỉ ghi bug có bằng chứng source code. Status ban đầu: `Ready for Codex`.

---

## BUG_M01_01 — Khóa tài khoản không chấm dứt phiên đang đăng nhập

- **Bug ID:** BUG_M01_01
- **Module ID:** M01 (side effect từ hành động ở M03)
- **Title:** Khóa tài khoản (IsActive=false) không đổi security stamp → phiên cookie đang mở vẫn hoạt động tới khi hết hạn (tối đa 8h)
- **Severity:** High
- **Priority:** P1
- **Business Flow ID:** BF-M01-06
- **Test Case ID:** TC_M01_020
- **Automated Test ID:** — (cần integration; xem backlog automation)
- **Environment:** Local/Dev (áp dụng mọi môi trường)
- **Role:** admin khóa · nạn nhân: mọi role đang đăng nhập
- **Preconditions:** User X đang đăng nhập (có phiên cookie hoạt động). Admin khóa tài khoản X.
- **Test Data:** bất kỳ tài khoản demo (VD `recruiter@polymind.local`) đang mở phiên.
- **Steps to Reproduce:**
  1. Đăng nhập bằng X ở trình duyệt A (giữ phiên hoạt động).
  2. Ở trình duyệt B, đăng nhập admin → `/admin` → Khóa tài khoản X (`ToggleUserAsync`).
  3. Ở trình duyệt A tiếp tục thao tác/điều hướng trong ≤30 phút.
- **Expected Result:** Phiên của X bị vô hiệu ngay/khi revalidate (≤30'); X bị đá về `/login`. Tài khoản bị khóa không được tiếp tục dùng hệ thống.
- **Actual Result:** X tiếp tục truy cập bình thường tới khi cookie hết hạn (ExpireTimeSpan 8h sliding). Revalidation 30' chỉ so `security_stamp` — mà stamp KHÔNG đổi khi khóa. Khóa chỉ chặn ĐĂNG NHẬP MỚI.
- **UI Evidence:** `AccountManagerPanel.razor:369-384` `ToggleUserAsync` gọi `UserManager.UpdateAsync(user)` sau khi đặt `user.IsActive=false`, KHÔNG gọi `UpdateSecurityStampAsync`.
- **API Evidence:** —
- **Database Evidence:** sau khi khóa, `is_active=false` nhưng `security_stamp` giữ nguyên → so khớp với stamp trong cookie của X.
- **Log Evidence:** —
- **Suspected Source Area:**
  - `IdentityRevalidatingAuthenticationStateProvider.ValidateSecurityStampAsync` (chỉ kiểm stamp, không kiểm IsActive).
  - `AccountManagerPanel.ToggleUserAsync` (khóa mà không đổi stamp).
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor` (ToggleUserAsync)
  - `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
  - `src/Polymind.Web/Components/Pages/Admin/UserEditDialog.razor` (kiểm luồng khóa/sửa khác nếu có)
- **Dependencies:** không chặn module khác (luồng login/lockout/phân quyền tĩnh vẫn đúng).
- **Regression Risk:** Trung bình — nếu thêm `UpdateSecurityStampAsync` khi khóa, mọi phiên của user đó (kể cả trên nhiều thiết bị) sẽ bị revalidate; cần bảo đảm không vô tình đá user khi CHỈ sửa thông tin (họ tên/SĐT) qua UpdateAsync.
- **Confidence Level:** Cao (đọc source rõ ràng; hành vi Identity đã biết: `UpdateAsync` không đổi security stamp).
- **Status:** Fixed
- **Codex resolution:** Mọi caller chuyển `IsActive` sang `false` (`AccountManagerPanel`, gỡ link Học viên, gỡ link Phụ huynh) dùng `UpdateSecurityStampAsync` để lưu trạng thái và stamp mới trong cùng Identity update. Revalidation đồng thời kiểm trực tiếp `IsActive`, nên phiên cookie bị từ chối trong tối đa 30 phút kể cả khi store không hỗ trợ security stamp.
- **Gợi ý hướng sửa (không bắt buộc):** khi khóa tài khoản, gọi `UserManager.UpdateSecurityStampAsync(user)` (ngoài việc set IsActive) để buộc revalidation đá phiên; và/hoặc bổ sung kiểm `user.IsActive` trong `ValidateAuthenticationStateAsync`.

---

## BUG_M01_02 — Thông báo đăng nhập lộ tồn tại tài khoản (account enumeration)

- **Bug ID:** BUG_M01_02
- **Module ID:** M01
- **Title:** Login web và `/api/auth/login` trả thông báo/mã khác nhau cho "tài khoản bị khóa" vs "sai mật khẩu" vs "email không tồn tại" → suy ra tài khoản có tồn tại hay không
- **Severity:** Low
- **Priority:** P2
- **Business Flow ID:** BF-M01-02, BF-M01-03
- **Test Case ID:** TC_M01_006
- **Automated Test ID:** —
- **Environment:** mọi môi trường
- **Role:** ẩn danh (kẻ tấn công)
- **Preconditions:** biết một email hợp lệ trong hệ thống.
- **Test Data:** 1 email tồn tại & active; 1 email tồn tại nhưng IsActive=false; 1 email không tồn tại.
- **Steps to Reproduce:**
  1. `/login` với email không tồn tại + mật khẩu bất kỳ → "Email hoặc mật khẩu không đúng."
  2. `/login` với email tồn tại nhưng IsActive=false → "Tài khoản đang bị khóa..." (khác biệt → xác nhận email tồn tại).
  3. `POST /api/auth/login`: email không tồn tại → 401; IsActive=false → 403; sai mật khẩu → 401 → mã 403 lộ tài khoản tồn tại nhưng bị khóa.
- **Expected Result:** Thông báo/mã trạng thái đồng nhất cho các trường hợp thất bại xác thực (không phân biệt tồn tại/khóa/sai mật khẩu) — hoặc chấp nhận rủi ro có lý do nghiệp vụ ghi rõ.
- **Actual Result:** Thông báo và HTTP status khác nhau → account enumeration.
- **UI Evidence:** `Login.razor:82` ("...không đúng") vs `Login.razor:88` ("Tài khoản đang bị khóa").
- **API Evidence:** `AuthEndpoints.cs:26` (401) vs `:29` (403) vs `:36` (401).
- **Suspected Source Area:** `Login.razor.LoginUser`, `AuthEndpoints POST /login`.
- **Required Files for Codex to Inspect:** `src/Polymind.Web/Components/Account/Login.razor`, `src/Polymind.Web/Api/AuthEndpoints.cs`.
- **Dependencies:** không chặn module khác.
- **Regression Risk:** Thấp. Lưu ý UX: gộp thông báo có thể làm user khó biết mình bị khóa — cần cân nhắc nghiệp vụ (có thể giữ thông báo khóa nhưng chỉ SAU khi mật khẩu đúng).
- **Confidence Level:** Cao (source rõ ràng). Mức độ: Low (bề mặt nội bộ, đã có lockout chống brute-force).
- **Status:** Fixed
- **Codex resolution:** User chốt ưu tiên bảo mật. Web và REST API dùng chung thông báo `Email hoặc mật khẩu không đúng.` cho email không tồn tại, tài khoản `IsActive=false`, sai mật khẩu và Identity lockout; API dùng cùng HTTP 401 cho các trường hợp này. Validation thiếu field vẫn là 400.
- **QA note:** Quyết định mới của user thay thế UX cũ trong các test case từng mong đợi thông báo khóa/HTTP 403/423; Codex không sửa automated expected result để hợp thức hóa bug.

---

## BUG_M01_03 — `PartnerAccountDialog.Unlink` khóa tài khoản không xoay security stamp (consistency)

- **Bug ID:** BUG_M01_03
- **Module ID:** M01 (side effect từ hành động gỡ liên kết Agent/CTV ở M09)
- **Title:** Khi gỡ liên kết & khóa tài khoản Agent/Cộng tác viên, `PartnerAccountDialog.Unlink` đặt `IsActive=false` bằng `UserManager.UpdateAsync` — KHÔNG dùng `UpdateSecurityStampAsync` như 3 caller đã sửa ở BUG_M01_01.
- **Severity:** Low (consistency / defense-in-depth hardening)
- **Priority:** P3
- **Business Flow ID:** BF-M01-06 (revoke) · M09 unlink
- **Test Case ID:** TC_M01_020 (biến thể partner) — chưa có case riêng
- **Automated Test ID:** —
- **Environment:** mọi môi trường
- **Role:** admin/agent-manager gỡ liên kết · nạn nhân: tài khoản Agent/CTV
- **Preconditions:** Agent/CTV có tài khoản đăng nhập, đang mở phiên. Người quản lý gỡ liên kết qua `PartnerAccountDialog`.
- **Steps to Reproduce:**
  1. Agent/CTV đăng nhập (phiên cookie).
  2. Quản lý mở `PartnerAccountDialog` → "Gỡ liên kết & khóa".
  3. Quan sát store: `is_active=false` nhưng `security_stamp` KHÔNG đổi.
- **Expected Result:** Đồng nhất với 3 caller đã sửa — khóa nên xoay security stamp (`UpdateSecurityStampAsync`) để vô hiệu stamp trên mọi thiết bị.
- **Actual Result:** Stamp giữ nguyên. **Tuy nhiên** phiên vẫn bị revalidation từ chối ≤30' nhờ kiểm `IsActive` (defense chính của BUG_M01_01) → KHÔNG có defect "phiên sống tới 8h".
- **UI Evidence:** `src/Polymind.Web/Components/Pages/Agents/PartnerAccountDialog.razor:180` `if (user is not null) { user.IsActive = false; await UserManager.UpdateAsync(user); }`.
- **Suspected Source Area:** `PartnerAccountDialog.Unlink`.
- **Required Files for Codex to Inspect:** `src/Polymind.Web/Components/Pages/Agents/PartnerAccountDialog.razor`.
- **Dependencies:** Không chặn module nào. **Non-blocking** — chỉ là căn chỉnh pattern lock cho nhất quán; hành vi revoke đã đúng nhờ kiểm `IsActive`.
- **Regression Risk:** Thấp — đổi sang `UpdateSecurityStampAsync` cho nhánh khóa, cần giữ nhánh lỗi `Succeeded` như 3 dialog kia.
- **Confidence Level:** Cao (đọc source rõ ràng).
- **Status:** Verified Fixed (code-level) — Claude 2026-07-11 (`08-verification-report.md`); runtime partner multi-session pending harness
- **Gợi ý hướng sửa (không bắt buộc):** thay `UpdateAsync` bằng `UpdateSecurityStampAsync` ở nhánh khóa và thêm xử lý lỗi Identity như `Parent/StudentAccountDialog.Unlink`.

---

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M01_01 | High | TC_M01_020 | BF-M01-06 | ToggleUserAsync + Revalidate stamp | AccountManagerPanel.razor, IdentityRevalidatingAuthenticationStateProvider.cs | 5 session-policy regression cases + TC_M01_020 runtime | **Verified Fixed** (Claude 2026-07-10) |
| 2 | BUG_M01_02 | Low | TC_M01_006 | BF-M01-02/03 | Login.razor, AuthEndpoints | Login.razor, AuthEndpoints.cs | Shared failure-policy regression + runtime response matrix | **Verified Fixed** (Claude 2026-07-10) |
| 3 | BUG_M01_03 | Low | TC_M01_020 (partner) | BF-M01-06 | PartnerAccountDialog.Unlink | PartnerAccountDialog.razor | Align stamp-rotation w/ Parent/StudentAccountDialog | **Verified Fixed** (Claude 2026-07-11) |
