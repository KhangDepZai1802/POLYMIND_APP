# M06 — Job Orders · Automation Report

## Framework & môi trường
- `tests/Polymind.Tests` (xUnit, net10.0). Ref Domain + Infrastructure, không ref Web.
- **Lệnh:** `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo` → `Passed 29, Failed 0, Skipped 0`.
- **Web compile:** `dotnet build src/Polymind.Web/Polymind.Web.csproj` → 0/0.

## Automated tests M06
- **Không có test mới.** Logic M06 (CRUD job, validation, attribution) nằm trong `.razor` (Web) → không unit-test được từ test project (không ref Web). Không có class Domain thuần cho job order.
- `CreatedBy` attribution: BUG_M06_01 phát hiện qua **source review**, không qua test (cần integration harness để chạy `TC_M06_005`).

## Verified bằng source review
- AuthZ list/detail: `[Authorize job_orders:read]` (`JobOrders.razor:3`, `JobOrderDetail.razor:3`).
- AuthZ create/edit: `JobOrderDialog.Save` dòng 125-132 (permission + `CanEditJobOrder`).
- AuthZ delete: `DeleteJobOrder` dòng 230 (re-check permission + `CanDeleteJobOrder`).
- Validation Country: dòng 119-123. Duplicate-submit guard: `_saving` dòng 134.
- **Defect:** `JobOrderDialog.razor:154` `CreatedBy = db.Users.Select(u=>u.Id).FirstOrDefaultAsync()` → BUG_M06_01.

## Pass / Fail / Blocked
- **Pass (code review):** TC_002, 006, 007, 009, 011, 013, 015, 016.
- **Fail (confirmed defect):** TC_005 → BUG_M06_01.
- **Blocked (no harness):** TC_001, 003, 004, 008, 010, 012, 014, 017.

## Automation backlog
1. Integration harness (WebApplicationFactory + DB test) → chạy TC_005 (attribution), TC_012 (cascade), TC_014 (REST gate).
2. Tách logic validation + attribution job-order sang Domain/Application để unit-test.
3. Regression sweep test cho anti-pattern "first user attribution" (bao phủ M04/M06/M12 sau khi Codex sửa).
