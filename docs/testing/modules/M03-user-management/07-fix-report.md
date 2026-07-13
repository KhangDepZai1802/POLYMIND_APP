# Module Fix Report

## Summary

- **Module ID:** M03
- **Module Name:** User & Account Management
- **Bugs Received:** 1
- **Bugs Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0 bug; runtime Identity/PostgreSQL verification chờ integration harness
- **Needs Clarification:** 0 (user chốt gỡ link và giữ Candidate)

## BUG_M03_01

### Status

- Fixed

### Investigation

Đã đọc `DeleteUserAsync`, entity/mapping/migration Candidate và cả hai dialog tạo/gỡ tài khoản Học viên/Phụ huynh. `OwnerUserId` và `ParentUserId` chỉ có index, không có FK/cascade. Luồng xóa query riêng `OwnerUserId`, vì vậy parent link không được EF phát hiện hoặc dọn tự động.

### Root Cause

`ParentUserId` được bổ sung sau luồng xóa tài khoản; cleanup thủ công cũ chỉ biết `OwnerUserId`. Không có database foreign key nên xóa Identity user vẫn thành công và để GUID rác.

### Evidence

- Trước sửa: query `db.Candidates.Where(c => c.OwnerUserId == user.Id)` và chỉ gán `OwnerUserId=null`.
- `AddCandidateOwnerUser`/`AddCandidateParentUser` chỉ tạo nullable column + index, không tạo FK.
- `ParentAccountDialog` gắn user qua `Candidate.ParentUserId`; `StudentAccountDialog` dùng `OwnerUserId`.
- 4 regression case sau sửa phủ owner, parent, cùng user ở cả hai field và unrelated links.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor`
- `src/Polymind.Web/Components/Pages/Admin/ParentStudentAccounts.razor`
- `src/Polymind.Web/Components/Pages/Candidates/ParentAccountDialog.razor`
- `src/Polymind.Web/Components/Pages/Candidates/StudentAccountDialog.razor`
- `src/Polymind.Domain/Entities/Candidate.cs`
- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260706084728_AddCandidateOwnerUser.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260709061830_AddCandidateParentUser.cs`

### Files Changed

- `src/Polymind.Domain/Security/CandidateAccountLinkRules.cs`
- `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor`
- `tests/Polymind.Tests/M03_CandidateAccountLinkRulesTests.cs`

### Symbols Changed

- `CandidateAccountLinkRules.UnlinkUser`
- `AccountManagerPanel.DeleteUserAsync`

### Fix

Query một lần mọi Candidate liên kết bằng `OwnerUserId || ParentUserId`. Quy tắc thuần `UnlinkUser` xóa từng field nếu trùng, kể cả trường hợp dữ liệu bất thường cùng user nằm ở cả hai field. Candidate giữ nguyên và được cập nhật `UpdatedAt`; sau đó luồng xóa Identity/audit tiếp tục như cũ.

### Why This Fix Is Correct

Fix làm BF-M03-05 đối xứng cho TC_M03_015 và TC_M03_016, đúng quyết định user: xóa tài khoản chỉ gỡ quyền truy cập, không xóa hồ sơ ứng viên. Không thêm cascade/migration và không ảnh hưởng authorization.

### Alternatives Considered

- Thêm FK cascade từ Candidate sang Identity user: cần migration/rủi ro dữ liệu và cascade không phù hợp hai quan hệ tùy chọn.
- Xóa Candidate cùng user: trái quyết định nghiệp vụ.
- Chỉ thêm query riêng cho parent: hoạt động nhưng dễ bỏ sót khi một user xuất hiện ở cả hai field; predicate + rule chung rõ hơn.

### Impact

- **API:** không đổi.
- **Database:** update nullable link + `UpdatedAt`, rồi delete user; không migration.
- **UI:** không đổi layout; chi tiết Candidate không còn link tới user đã xóa.
- **Security:** AgentScope không resolve nhầm GUID user đã xóa.
- **Backward compatibility:** luồng xóa student cũ vẫn được giữ; parent nay đối xứng.
- **Data compatibility:** cleanup xử lý được cả dữ liệu bất thường trùng hai field.

### Regression Risks

- Cleanup Candidate và `UserManager.DeleteAsync` vẫn dùng hai DbContext/transaction như thiết kế cũ; nếu delete user thất bại sau cleanup thì user còn nhưng link đã gỡ. Đây là rủi ro cũ, không mở rộng trong bug nhỏ này.
- Cần DB integration để xác nhận tracking/save/delete thực tế.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `M03_CandidateAccountLinkRulesTests` | Unit regression | Passed 4/4 | BUG_M03_01 / TC_M03_015-016 |
| Toàn bộ `Polymind.Tests` | Regression | Passed 26/26 | Failed 0, Skipped 0 |
| Build `Polymind.Web` ra `C:\tmp\polymind-codex-build` | Compile | Passed | 0 warning, 0 error |
| Xóa parent/student trên PostgreSQL test | Integration | Blocked | Chưa có test DB/harness |

### Test Results

- **Passed:** 26 + build
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime Identity/DB delete

### Verification Instructions for Claude

1. Trên DB test, tạo Candidate gắn student qua `OwnerUserId`; xóa student ở `/admin/parents-students`; xác nhận Candidate còn và owner link null.
2. Lặp lại với parent qua `ParentUserId`; xác nhận parent link null (TC_M03_016).
3. Kiểm user đã bị xóa khỏi Identity và audit `delete/users` tồn tại.
4. Tạo hai Candidate khác nhau cùng tham chiếu user test (dữ liệu biên) và xác nhận mọi link đều được dọn.
5. Giả lập `DeleteAsync` thất bại nếu có harness để đánh giá residual transaction risk; không đánh dấu `Verified Fixed` chỉ từ unit test Codex.

## Cross-module hardening found during M03 investigation

Hai dialog `ParentAccountDialog.Unlink` và `StudentAccountDialog.Unlink` cũng khóa user bằng `UpdateAsync` cũ. Chúng đã được đổi sang `UpdateSecurityStampAsync` và kiểm `IdentityResult`, hoàn thiện impact radius của BUG_M01_01. Chi tiết đã bổ sung vào M01 `06-bug-report.md` và `07-fix-report.md`.
