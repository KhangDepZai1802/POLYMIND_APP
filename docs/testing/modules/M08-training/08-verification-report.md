# Verification Report — M08 Training

- **Module:** M08 — Training
- **Verifier:** Claude (Independent Verification) — 2026-07-11 phiên #8
- **Fix under review:** CR-M08-1 (U-M08-1) — `07-fix-report.md`
- **Verification Status:** **Verified (code-level)**

## Scope

CR-M08-1: mở `training:read` cho Recruiter, DocumentStaff, VisaStaff, Accountant (read-only), KHÔNG cấp mutation.

## Evidence Reviewed

- Diff `src/Polymind.Infrastructure/Persistence/DbSeeder.cs` `RolePermissionMap`:
  - Recruiter (`DbSeeder.cs:59`): `Read("dashboard","job_orders","agents","collaborators","notifications","training")` → có `training:read`, không có `Crud/Actions("training",...)`.
  - DocumentStaff (`DbSeeder.cs:74`): `Read(...,"training")`.
  - VisaStaff (`DbSeeder.cs:81`): `Read(...,"training")`.
  - Accountant (`DbSeeder.cs:90`): `Read(...,"training")`.
- RM (`:51`) và Consultant (`:67`) vẫn `Crud("training")` — quyền quản lý không đổi.
- `RoleHasPermission` (`:115-117`) tra bảng, case-insensitive; SuperAdmin xử lý riêng bởi authorization layer.
- Seeder idempotent: chỉ AddRange permission mới (`:141-150`); không migration schema, không sửa data nghiệp vụ.

## Bug-by-bug Verdict

| Item | Verdict | Bằng chứng |
|---|---|---|
| CR-M08-1 | **Verified Fixed (code-level)** | 4 role có `training:read`, không có create/update/delete/approve; dialog mutation re-check `training:create/update` giữ nguyên (không sửa `.razor`). |

## Tests / Regression

- `M08_TrainingRulesTests.Related_staff_can_read_training_but_cannot_mutate_it` (Theory ×4 role): asserts `training:read==true` và create/update/delete/approve `==false`. **Passed.**
- Full suite `dotnet test tests/Polymind.Tests`: **Passed 122, Failed 0, Skipped 0**.
- Web build (output riêng `.qa/build/session8-web`): **0 Warning, 0 Error**.
- Không phát hiện Codex sửa test/expected để né lỗi; không hard-code; không làm yếu authorization/scope/mutation.

## Residual / Not Measured

- Runtime menu/route/CandidateDetail card render theo role và refresh claim sau re-login: chưa có bUnit/WebApplicationFactory harness (OBS ngoài phạm vi CR).
- OBS-M08-01 (concurrency no-rowversion), OBS-M08-03/04: ngoài phạm vi CR-M08-1.

## Conclusion

CR-M08-1 **Verified Fixed (code-level)**. → `QA=No Confirmed Bugs`, `Codex=Fixed`, `Verification=Verified (code)`.
