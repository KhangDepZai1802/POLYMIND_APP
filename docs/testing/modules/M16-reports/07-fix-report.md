# Module Fix Report

## Summary

- **Module ID:** M16
- **Module Name:** Reports & Export
- **Bugs/Changes Received:** BUG_M16_01, CR-M16-1
- **Bugs/Changes Fixed:** 2
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## BUG_M16_01

### Status

**Fixed — Waiting for Claude Verification**

### Investigation

`Reports.razor` có `_from/_to` và lọc biểu đồ, nhưng 24 link (8 slug × 3 format) là URL tĩnh. `CsvExportEndpoints.Register` cũng chỉ truyền `ApplicationDbContext` vào builder, nên endpoint không thể biết range.

Mỗi báo cáo cần trường ngày nghiệp vụ khác nhau:

- payment revenue: `PaidDate`, fallback `CreatedAt`;
- expense: `ExpenseDate`;
- commission: `CreatedAt`;
- overdue: `DueDate`, fallback `CreatedAt`;
- lead: `CreatedAt`;
- funnel: `CandidateJobOrder.CreatedAt`;
- top agent: candidate/commission `CreatedAt`.

### Root Cause

Date range chỉ là UI state cục bộ, không thuộc contract export. Builders không nhận range và không có validation `from <= to`.

### Evidence

- Trước sửa: `Href=/export/{slug}.{ext}`; builder signature `(db)`.
- Sau sửa: `ExportHref` nối `?from=yyyy-MM-dd&to=yyyy-MM-dd`; all-time giữ URL không query.
- Endpoint bind `DateOnly? from/to`, reject reversed range 400 trước khi tạo DbContext.
- Tất cả 8 builders nhận `ReportDateRange` và có filter theo trường ngày nêu trên.

### Files Inspected

- Toàn bộ `docs/testing/modules/M16-reports/01→06`
- `src/Polymind.Web/Components/Pages/Reports/Reports.razor`
- `src/Polymind.Web/Reporting/CsvExportEndpoints.cs`
- `src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs`
- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`
- `src/Polymind.Web/Authorization/PermissionAuthorization.cs`
- `tests/Polymind.Tests/M02_PermissionRegistryTests.cs`

### Files Changed

- `src/Polymind.Domain/Reporting/ReportAccessRules.cs`
- `src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs`
- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`
- `src/Polymind.Web/Reporting/CsvExportEndpoints.cs`
- `src/Polymind.Web/Components/Pages/Reports/Reports.razor`
- `tests/Polymind.Tests/M02_PermissionRegistryTests.cs`
- `tests/Polymind.Tests/M16_ReportRulesTests.cs`
- `docs/testing/modules/M16-reports/03-test-cases.md`
- `docs/testing/modules/M16-reports/04-traceability.md`
- `docs/testing/modules/M16-reports/05-automation-report.md`
- `docs/testing/modules/M16-reports/06-bug-report.md`
- `docs/testing/modules/M16-reports/07-fix-report.md`
- `docs/testing/MODULE_QA_BOARD.md`
- `docs/testing/SESSION_CHECKPOINT.md`

### Symbols Changed

- `ReportDateRange`
- `ReportAccessRules`
- `CsvExportEndpoints.Register`
- `CsvExportEndpoints.Export`
- 8 `Build*Async` report builders
- `Reports.ExportHref`, `VisibleExports`, `LoadData`

### Fix

1. Domain `ReportDateRange` cung cấp inclusive bounds, validation và stable query string.
2. UI dùng một danh sách export chung cho Excel/PDF/CSV; mọi link lấy range đã Apply.
3. Endpoint nhận range optional; không có range giữ hành vi all-time cũ.
4. Tám builders lọc đúng ngày nghiệp vụ; finance-monthly tạo bucket đúng các tháng trong range.
5. Range đảo trả 400 và không query DB.

### Why This Fix Is Correct

- Khớp BF-M16-02 / TC_M16_012: file dùng cùng `_from/_to` với biểu đồ.
- Inclusive hai biên ngày; custom 01→31 không bỏ mất ngày cuối.
- Không tham số vẫn cho client/bookmark cũ tải all-time.
- Cả CSV/XLSX/PDF đi qua cùng `Export` + builder nên không lệch format.
- Không sửa expected result để né lỗi và không ghi DB.

### Alternatives Considered

- Truyền `_rangeKey` rồi tính lại server: loại vì custom range cần ngày cụ thể và timezone dễ lệch.
- Chỉ sửa finance-monthly: loại vì menu kỳ vọng range áp cho mọi export.
- Lưu range trong session: loại vì làm endpoint phụ thuộc state và khó bookmark/retry.

### Impact

- **API:** thêm query optional `from/to`; backward-compatible.
- **Database:** chỉ thay đổi query/read filtering; không migration.
- **UI:** link tải phản ánh range đã áp dụng.
- **Security:** invalid range fail-fast; authorization không suy yếu.
- **Data compatibility:** không mutation.

### Regression Risks

- Chưa có integration test giải mã nội dung CSV/XLSX/PDF với DB seed.
- Builders vẫn tải một số tập dữ liệu rồi lọc in-memory (OBS-M16-02 perf cũ).
- State reports như overdue/funnel dùng ngày due/created phù hợp thay vì snapshot lịch sử trạng thái; cần Claude đối chiếu kỳ vọng runtime.

## CR-M16-1

### Status

**Fixed — Waiting for Claude Verification**

### Investigation / Root Cause

Một permission `reports:read` đang đồng thời mở page, toàn bộ financial UI và cả 8 export. Chỉ ẩn menu không đủ vì RM có thể gọi URL tài chính trực tiếp.

### Fix

1. Thêm resource RBAC `financial_reports` → permission động `financial_reports:read`.
2. Seed quyền mới cho Director và Accountant; SuperAdmin tự có mọi permission; RM chỉ giữ `reports:read`.
3. Phân loại slug:
   - recruitment: `lead-by-province`, `recruitment-funnel`;
   - financial: finance-monthly, commissions, overdue-payments, revenue country/job, top-agents (có cột hoa hồng).
4. Export group vẫn yêu cầu `reports:read`; từng slug tài chính re-check `financial_reports:read` server-side và trả 403 nếu thiếu.
5. Page resolve policy; RM không thấy menu/card/chart/table tài chính và `LoadData` không query Payments/Expenses/Commissions cho RM.

### Why This Fix Is Correct

- Khớp U-M16-1: RM dùng funnel/lead recruitment nhưng không thấy doanh thu/lợi nhuận/hoa hồng.
- UI và endpoint cùng dùng `ReportAccessRules`, giảm nguy cơ phân loại lệch.
- Server guard ngăn direct URL, không dựa vào ẩn UI.
- Dynamic policy provider nhận permission mới vì registry có resource `financial_reports` và action `read`.
- Director/Accountant/SuperAdmin giữ quyền tài chính.

### Impact / Risks

- Permission registry tăng 20→21 resource, 100→105 permission; seed idempotent thêm permission và cập nhật role claims.
- User đang có cookie/claim cũ có thể cần đăng nhập lại sau seed/security-stamp refresh để nhận permission mới.
- Không migration schema; permission rows được seeder tạo theo cơ chế hiện hữu.
- Top-agents bị xếp financial vì chứa tổng/đã chi hoa hồng; RM vẫn có KPI tuyển dụng staff/funnel/lead trên page.

## Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `dotnet test ... --filter FullyQualifiedName~M16_ReportRulesTests` | Unit | Passed 6/6 | range, invalid bounds, access matrix, registry |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore` | Full regression | Passed 112/112 | Failed 0, Skipped 0 |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=.../.qa/build/m16-final/` | Build | Passed | 0 warning, 0 error |

Session tiếp theo đã chạy lại tuần tự cùng ngày 2026-07-11 (sau khi loại trừ một lần tranh chấp DLL do ba lệnh .NET chạy song song): M16 **6/6**, toàn suite **112/112**, Web build output `.qa/build/m16-resume` **0 warning / 0 error**. Không sửa test hay expected result.

## Test Results

- **Passed:** 112
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime HTTP/DB/file-content verification

## Verification Instructions for Claude

1. Chạy 6 M16 tests, toàn suite và Web build.
2. Kiểm registry/seeder:
   - RM có `reports:read`, không `financial_reports:read`;
   - Director/Accountant có cả hai; SuperAdmin bypass đúng.
3. HTTP probe RM:
   - `/export/lead-by-province.csv?from=...&to=...` và funnel → 200;
   - sáu slug tài chính × ít nhất một format → 403.
4. UI RM: không render/query card, chart, top-agent, commission, revenue, expense, receivable; vẫn thấy lead/funnel/candidate/visa/flight/KPI tuyển dụng.
5. UI finance role: đủ 8 export và toàn financial UI.
6. Chọn month/quarter/year/custom, tải CSV/XLSX/PDF; đối chiếu file chỉ chứa record trong inclusive range.
7. Gọi không query → hành vi all-time cũ; gọi `from > to` → 400.
8. Đăng xuất/đăng nhập lại sau seeding nếu test account chưa có claim `financial_reports:read`.
9. Không đánh fail do OBS-M16-01/02/04/05 ngoài phạm vi; ghi residual riêng.
