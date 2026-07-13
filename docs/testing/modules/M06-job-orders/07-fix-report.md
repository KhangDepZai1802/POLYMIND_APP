# Module Fix Report

## Summary

- **Module ID:** M06
- **Module Name:** Job Orders
- **Bugs Received:** 1
- **Bugs Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Verification:** Chờ Claude xác minh độc lập; Codex không đánh dấu `Verified Fixed`.

## BUG_M06_01

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Đọc toàn bộ context package M06 (`01`–`06`) và source list/detail/dialog/API/entity.
- Tái hiện bằng source path: sau authorization thành công, create branch không dùng principal mà lấy user đầu tiên không `OrderBy` từ DB.
- Đối chiếu pattern đúng ở M04 và các action delete M06: `AuthStateProvider.GetRequiredUserIdAsync(db)`.
- Sweep xác nhận M12 và audit fallback là scope/module khác; không sửa vì chưa có trạng thái Waiting/Returned trong queue.

### Root Cause

Create branch dùng query user table như một actor surrogate dù `AuthenticationStateProvider` đã được inject và principal đã qua permission + role gate.

### Evidence

- Source cũ: `CreatedBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()`.
- Source mới: resolve actor đã đăng nhập rồi truyền vào `JobOrderCreationRules.Create`.
- Regression test tạo hai Guid độc lập và khóa `CreatedBy` đúng actor truyền vào.

### Files Inspected

- `docs/testing/modules/M06-job-orders/01-analysis.md` → `06-bug-report.md`
- `src/Polymind.Web/Components/Pages/JobOrders/JobOrders.razor`
- `src/Polymind.Web/Components/Pages/JobOrders/JobOrderDetail.razor`
- `src/Polymind.Web/Components/Pages/JobOrders/JobOrderDialog.razor`
- `src/Polymind.Web/Api/ResourceEndpoints.cs` (Job Orders API)
- `src/Polymind.Domain/Entities/JobOrder.cs`
- `src/Polymind.Web/Auditing/AuditLogHelpers.cs`
- `src/Polymind.Web/Components/Pages/Leads/LeadDetail.razor` (pattern M04 đã fix)
- `src/Polymind.Web/Components/Pages/Visas/VisaDialog.razor`, `FlightDialog.razor` (sweep only; không đổi)
- `src/Polymind.Infrastructure/Persistence/DemoDataSeeder.cs` (seed-only; không đổi)

### Files Changed

- `src/Polymind.Web/Components/Pages/JobOrders/JobOrderDialog.razor`
- `src/Polymind.Domain/JobOrders/JobOrderCreationRules.cs`
- `tests/Polymind.Tests/M06_JobOrderCreationRulesTests.cs`

### Symbols Changed

- `JobOrderDialog.Save`
- `JobOrderCreationRules.Create`
- `M06_JobOrderCreationRulesTests.New_job_order_is_attributed_to_the_authenticated_actor`

### Fix

- Resolve `actorId` từ authenticated principal bằng helper bắt buộc hiện có.
- Tạo JobOrder qua Domain factory nhận actorId tường minh; không còn query user đầu DB trong M06 create path.
- Edit path giữ nguyên `CreatedBy` như trước.

### Why This Fix Is Correct

- BF-M06-02/TC_M06_005 quy định `CreatedBy` là người đang tạo job.
- Authorization/role validation vẫn chạy trước khi resolve actor và insert.
- Factory làm attribution trở thành tham số bắt buộc và cho phép regression test không cần Blazor harness.

### Alternatives Considered

- Gán trực tiếp helper vào initializer: đúng nhưng khó khóa regression từ test project không reference Web.
- Sửa luôn M12/shared fallback: ngoài module/queue hiện tại; để Claude QA/hand-off riêng.

### Impact

- **API/database/UI:** không đổi contract/schema/form.
- **Security:** không đổi quyền; loại bỏ attribution sang user không liên quan.
- **Backward compatibility:** job cũ giữ nguyên; chỉ job mới được ghi đúng actor.
- **Data compatibility:** không migration, không sửa dữ liệu lịch sử.

### Regression Risks

- Thấp; code generation vẫn giữ format cũ.
- Authenticated principal thiếu NameIdentifier sẽ đi theo behavior helper shared hiện tại; observation fallback vẫn chờ module shared/M19/M20.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `New_job_order_is_attributed_to_the_authenticated_actor` | Unit | Passed | BUG_M06_01 / TC_M06_005 |
| Shared test suite | Regression | Passed | 52 passed, 0 failed, 0 skipped |
| Web build output riêng | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** attribution unit, shared regression, Web compile.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** UI/DB create as RM chưa có bUnit/integration harness.

### Verification Instructions for Claude

- Chạy lại test suite và Web/solution build.
- Đăng nhập bằng RM không phải user seed đầu tiên; tạo JobOrder và query `job_orders.created_by` phải bằng RM id.
- Sửa JobOrder đã có; `CreatedBy` phải giữ nguyên, chỉ `UpdatedAt` đổi.
- Kiểm recruiter/consultant vẫn bị chặn create/edit theo permission + `BusinessRoleAccess`.
- Xác nhận M06 không còn `Users.Select(...Id).First*`; không coi M12/shared fallback là đã fix trong report này.
