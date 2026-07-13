# Module Fix Report

## Summary

- **Module ID:** M17
- **Module Name:** Dashboard
- **Bugs/Changes Received:** CR-M17-1
- **Bugs/Changes Fixed:** 1
- **Cannot Reproduce / Blocked / Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## CR-M17-1

### Status

**Fixed — Waiting for Claude Verification**

### Investigation

`Home.razor` chỉ yêu cầu `dashboard:read`, permission mà mọi staff có. Component luôn render bốn thẻ tài chính và hai bảng doanh thu/đại lý, đồng thời luôn query `FinanceEligibility`, Payments, Agents, Candidates và AgentCommissions. Portal `/me`, partner redirect và KPI tuyển dụng không có lỗi nên được giữ nguyên.

### Root Cause

Dashboard chưa có capability guard riêng cho dữ liệu tài chính; quyền vào trang bị dùng như quyền đọc mọi KPI.

### Evidence

- Trước sửa: finance query chạy vô điều kiện ngay sau khi tạo DbContext; UI tài chính render cho mọi `dashboard:read`.
- Sau sửa: resolve `financial_reports:read` trước khi query; mọi query Payments/Commissions/Agents cho nhóm tài chính nằm trong `_canReadFinance`; bốn StatCard và grid doanh thu/top đại lý dùng cùng guard.
- Permission từ M16: Director/Accountant có `financial_reports:read`, SuperAdmin có toàn quyền; RM và staff còn lại không có.

### Files Inspected

- `docs/testing/modules/M17-dashboard/01-analysis.md` → `06-bug-report.md`
- `src/Polymind.Web/Components/Pages/Home.razor`
- `src/Polymind.Web/Components/Pages/Portal/Overview.razor`
- `src/Polymind.Web/Display/FinanceEligibility.cs`
- `src/Polymind.Domain/Reporting/ReportAccessRules.cs`
- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`
- `src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs`

### Files Changed / Symbols Changed

- `src/Polymind.Web/Components/Pages/Home.razor`
  - `_canReadFinance`
  - `OnInitializedAsync`
  - finance StatCard/grid conditional rendering
- M17 test/traceability/automation/bug/fix reports; QA board; session checkpoint.

### Fix

1. Authorize authenticated principal bằng `ReportAccessRules.FinancialPermission` (`financial_reports:read`).
2. Chỉ Director/Accountant/SuperAdmin render công nợ, quá hạn, doanh thu tháng, quốc gia doanh thu cao, dashboard doanh thu và top đại lý/hoa hồng.
3. Staff thiếu permission không chạy `FinanceEligibility`, Payments, revenue hoặc commission/agent queries; không giữ dữ liệu nhạy cảm trong component state.
4. Lead, candidate, workflow, visa/flight và các KPI tuyển dụng vẫn load/render cho mọi staff có `dashboard:read`.

### Why This Fix Is Correct

- Khớp U-M17-1 và CR-M16-1: RM chỉ xem dữ liệu tuyển dụng.
- Guard capability fail-closed và dùng cùng policy với báo cáo tài chính, không phụ thuộc ẩn CSS/UI.
- Không đổi authorization của Home/Portal, không làm yếu scope, không ghi DB và không thay expected test.

### Alternatives Considered

- Chỉ dùng `AuthorizeView Roles=...`: không chọn vì query nhạy cảm vẫn chạy và role list có thể lệch permission seed.
- Chỉ ẩn số tiền hoa hồng trong Top đại lý: không chọn vì user chốt ẩn cả KPI/top đại lý tài chính.
- Tạo permission dashboard tài chính mới: không cần vì `financial_reports:read` đã biểu diễn đúng cùng capability và role matrix.

### Impact

- **API/Database:** không đổi contract/schema; giảm read query cho non-finance staff.
- **UI:** non-finance staff chỉ còn KPI tuyển dụng; finance roles giữ giao diện cũ.
- **Security:** giảm data exposure; fail-closed nếu claim mới chưa có.
- **Backward compatibility:** finance users cần claim `financial_reports:read` từ seeder hiện tại; có thể cần đăng nhập lại sau seed.

### Regression Risks

- Chưa có bUnit/WebApplicationFactory để assert render và đếm SQL query theo role.
- Cookie/claim cũ trước M16 có thể chưa chứa permission mới cho tới khi seed + re-login/security-stamp refresh.
- OBS-M17-02 perf của các KPI tuyển dụng vẫn giữ nguyên.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| M16 access/permission regression | Unit | Passed 6/6 | Policy/registry dùng chung với M17 |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore` | Full regression | Passed 112/112 | Failed 0, Skipped 0 |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=.../.qa/build/m17-check/` | Build | Passed | 0 warning, 0 error |
| `git diff --check -- Home.razor` | Static | Passed | Không whitespace error |

### Test Results

- **Passed:** 112
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime role-render/query probe do chưa có Web integration harness

### Verification Instructions for Claude

1. Chạy toàn suite và Web build.
2. Đăng nhập RM/recruiter/consultant/document/visa: xác nhận không có bốn thẻ tài chính, dashboard doanh thu hoặc top đại lý; vẫn có lead/candidate/funnel/visa/flight KPI.
3. Bật EF query log hoặc interceptor: xác nhận các role trên không query Payments, AgentCommissions, Agents/Candidates cho nhóm top đại lý.
4. Đăng nhập Director/Accountant/SuperAdmin: xác nhận đủ KPI/bảng tài chính và số liệu không đổi.
5. Kiểm RM không có `financial_reports:read`; finance roles có claim sau seed/re-login.
6. Không đánh fail do OBS-M17-02 perf ngoài phạm vi.
