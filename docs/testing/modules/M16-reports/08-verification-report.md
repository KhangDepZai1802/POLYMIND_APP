# Verification Report — M16 Reports & Export

- **Module:** M16 — Reports & Export
- **Verifier:** Claude (Independent Verification) — 2026-07-11 phiên #8
- **Fixes under review:** BUG_M16_01 + CR-M16-1 — `07-fix-report.md`
- **Verification Status:** **Verified (code-level)**

## Scope

- **BUG_M16_01 (Low):** export Excel/PDF/CSV bỏ qua khoảng thời gian đang chọn (link tĩnh → file luôn toàn kỳ).
- **CR-M16-1 (U-M16-1):** RM chỉ báo cáo tuyển dụng; bỏ báo cáo tài chính khỏi RM (UI + direct export đều guard server-side).

## Evidence Reviewed

- `src/Polymind.Domain/Reporting/ReportAccessRules.cs`:
  - `ReportDateRange`: `TryCreate` reject reversed (`from<=to`); `Includes` inclusive hai biên; `ToQueryString` stable; `All` → chuỗi rỗng (backward-compatible).
  - `RecruitmentSlugs` = {lead-by-province, recruitment-funnel}; `FinancialSlugs` = {finance-monthly, commissions, overdue-payments, revenue-by-country, revenue-by-job-order, top-agents}; `RequiresFinancialPermission`, `CanExport` (financial cần cả recruitment + finance).
- `src/Polymind.Web/Reporting/CsvExportEndpoints.cs`:
  - Group `/export` `RequireAuthorization("reports:read")` (`:25`).
  - `Register` bind `DateOnly? from/to` cho .csv/.xlsx/.pdf (`:54-62`).
  - `Export` (`:65-88`): (1) `TryCreate` sai range → `Results.BadRequest` **400**; (2) `RequiresFinancialPermission` → `AuthorizeAsync(financial_reports:read)` fail → `Results.Forbid` **403**; (3) truyền `range` cho builder.
  - 8 builder nhận `ReportDateRange`, lọc theo trường ngày nghiệp vụ (PaidDate/ExpenseDate/CreatedAt/DueDate/CJO.CreatedAt) inclusive.
- `src/Polymind.Web/Components/Pages/Reports/Reports.razor`:
  - `_canReadFinance = AuthorizeAsync(FinancialPermission).Succeeded` (`:563-565`).
  - `ExportHref` nối `ReportDateRange(_from,_to).ToQueryString()` (`:603-604`).
  - `VisibleExports` lọc bỏ financial slug khi `!_canReadFinance` (`:499-500`).
  - `LoadData`: financial state reset về 0 (`:700-708`); mọi query Payments/Expenses/JobOrders revenue/commission/top-agent nằm trong `if (_canReadFinance)` (`:710+`); lead/staff KPI query vô điều kiện.
- `PermissionRegistry`: resource `financial_reports` + `financial_reports:read` (test `Financial_permission_is_registered_for_dynamic_policy_provider` xác nhận).
- `DbSeeder`: Director/Accountant có `financial_reports` (`:41,:90`); RM chỉ `reports` (`:52`); SuperAdmin bypass.

## Bug-by-bug Verdict

| Item | Verdict | Bằng chứng |
|---|---|---|
| BUG_M16_01 | **Verified Fixed (code-level)** | Link export mang `?from&to`; endpoint bind + validate + truyền range; builder lọc inclusive; all-time backward-compatible. |
| CR-M16-1 | **Verified Fixed (code-level)** | RM thiếu `financial_reports:read` → UI ẩn export/card/chart tài chính + `LoadData` không query finance; direct URL financial slug → 403 server-side. Director/Accountant/SuperAdmin giữ nguyên. |

## Tests / Regression

- `M16_ReportRulesTests` (**6/6**): inclusive range, reversed reject, all-time URL, RM export recruitment nhưng không financial, finance role export toàn bộ, registry financial_reports.
- Full suite: **Passed 122, Failed 0, Skipped 0**.
- Web build: **0 Warning, 0 Error**.
- Không sửa test/expected để né lỗi; authorization không suy yếu (server re-check, không chỉ ẩn UI).

## Residual / Not Measured

- Runtime HTTP probe (RM 200 recruitment / 403 financial; finance role 8/8) và giải mã nội dung file CSV/XLSX/PDF theo range: chưa có Web integration harness → pending.
- OBS-M16-01 (receipt PDF latent-IDOR, không khai thác với seed), OBS-M16-02 (perf in-memory filter), OBS-M16-04/05: ngoài phạm vi.
- State report (overdue/funnel) dùng ngày due/created thay vì snapshot lịch sử trạng thái — cần đối chiếu kỳ vọng runtime nếu nghiệp vụ yêu cầu snapshot.

## Conclusion

BUG_M16_01 + CR-M16-1 **Verified Fixed (code-level)**. → `QA=Completed`, `Codex=Fixed`, `Verification=Verified (code)`.
