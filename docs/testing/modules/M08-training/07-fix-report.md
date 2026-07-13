# Module Fix Report

## Summary

- **Module ID:** M08
- **Module Name:** Training
- **Bugs/Changes Received:** CR-M08-1 (U-M08-1)
- **Bugs/Changes Fixed:** 1
- **Cannot Reproduce / Blocked / Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## CR-M08-1

### Status

**Fixed — Waiting for Claude Verification**

### Investigation / Root Cause

Page training yêu cầu `training:read`; dialog mutation re-check `training:create/update`. Seed thiếu toàn bộ permission training cho Recruiter, DocumentStaff, VisaStaff và Accountant, nên bốn role không thể xem dù U-M08-1 đã chốt read-only access.

### Evidence

- Trước sửa: bốn nhánh `RolePermissionMap` không chứa `training`.
- Sau sửa: mỗi nhánh thêm `training` qua `Read(...)`; regression xác nhận `training:read=true` và create/update/delete/approve đều false.

### Files Inspected

- Toàn bộ `docs/testing/modules/M08-training/01→06`
- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`
- `src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs`
- `src/Polymind.Web/Components/Pages/Training/*.razor`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`
- `tests/Polymind.Tests/M08_TrainingRulesTests.cs`

### Files Changed / Symbols Changed

- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`: `RolePermissionMap`, `RoleHasPermission`.
- `tests/Polymind.Tests/M08_TrainingRulesTests.cs`: `Related_staff_can_read_training_but_cannot_mutate_it`.
- M08 reports, QA board và session checkpoint.

### Fix / Why This Fix Is Correct

- Chỉ thêm `training:read`, đúng U-M08-1; không cấp mutation.
- Page/menu/card dùng dynamic permission policy nên không hard-code role hay sửa UI.
- Agent/self scope và dialog server re-check giữ nguyên; authorization không bị làm yếu.
- Seeder idempotent; không migration schema hay sửa data nghiệp vụ.

### Alternatives Considered

- `Crud("training")`: loại vì vượt quyền read-only.
- Hard-code role trong Razor: loại vì làm lệch RBAC/menu/policy.
- Permission mới: không cần vì `training:read` là contract chính xác.

### Impact / Regression Risks

- **API/DB schema:** không đổi; role claims được seeder cập nhật.
- **UI:** menu/trang/thẻ training xuất hiện sau refresh claim.
- **Security:** mutation và data-scope không đổi.
- Cookie cũ có thể cần đăng nhập lại; runtime role/menu chưa có integration harness.
- OBS-M08-01/03/04 ngoài phạm vi.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `M08_TrainingRulesTests` | Unit | Passed 8/8 | gồm 4 role cases CR-M08-1 |
| Full `Polymind.Tests` | Regression | Passed 116/116 | Failed 0, Skipped 0 |
| Web build `.qa/build/m08-cr1` | Build | Passed | 0 warning, 0 error |

### Verification Instructions for Claude

1. Chạy M08 tests, toàn suite và Web build.
2. Kiểm seed: bốn role có `training:read`, không có mutation permissions.
3. Seed/re-login từng role: menu + `/training` + thẻ CandidateDetail đọc được; detail không có nút Edit/Thêm phiếu.
4. Tamper mutation: dialog re-check vẫn từ chối.
5. Xác nhận RM/Consultant vẫn manage; Agent/CTV/Parent/Student scope cũ không đổi.
