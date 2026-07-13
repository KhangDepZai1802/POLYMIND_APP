# Module Fix Report

## Summary

- **Module ID:** M10
- **Module Name:** Finance (Payments & Expenses)
- **Bugs Received:** 1
- **Bugs Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Verification:** Chờ Claude xác minh độc lập; Codex không đánh dấu `Verified Fixed`.

## BUG_M10_01

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Đọc toàn bộ context package M10 (`01`–`06`) và kiểm kê mọi assignment runtime `Payment.Status = Paid`.
- Xác nhận ba đường khác nhau:
  - `Finance.MarkStagePaid`: có kiểm tuần tự + gọi CommissionEngine.
  - `Finance.ApprovePayment`: không kiểm tuần tự, không gọi CommissionEngine.
  - `PaymentDialog.Save`: gán Status trực tiếp; chỉ re-check create/update, chưa re-check approve khi chọn Paid.
- Kiểm các consumer: báo cáo doanh thu/công nợ, notification khoản thu, receipt, M09 CommissionEngine và U2 no-refund.
- Chọn thống nhất cả ba đường qua một posting service thay vì chặn chức năng ở tab Khoản thu/dialog; thu lẻ `Stage=null` vẫn duyệt bình thường.

### Root Cause

Transition Payment→Paid và side effects bị đặt trực tiếp trong UI component, không có một application boundary dùng chung. Vì vậy các caller phụ bỏ quên kiểm tuần tự, actor/date/audit và trigger commission; dialog còn có authorization gap tiềm ẩn cho action approve.

### Evidence

- Trước fix, `rg` cho thấy assignment Paid nằm ở `Finance.razor` và `PaymentDialog.ApplyTo`.
- Sau fix, runtime Web chỉ còn một assignment Paid tại `PaymentPostingService`.
- PostgreSQL integration probe:
  - ServiceFee trước Deposit bị chặn.
  - Paid tuần tự đủ 4 stage.
  - Sinh đúng 3 commission (Deposit/Selected/Departure).
  - Thu lẻ Paid thành công, không sinh commission.
  - 5 payment audits cho 4 stage + 1 thu lẻ.

### Files Inspected

- `docs/testing/modules/M10-finance/01-analysis.md` → `06-bug-report.md`
- `src/Polymind.Web/Components/Pages/Finance/Finance.razor`
- `src/Polymind.Web/Components/Pages/Finance/PaymentDialog.razor`
- `src/Polymind.Web/Components/Pages/Finance/ExpenseDialog.razor`
- `src/Polymind.Web/Display/PaymentSchedule.cs`
- `src/Polymind.Web/Display/FinanceEligibility.cs`
- `src/Polymind.Web/Commissions/CommissionEngine.cs`
- `src/Polymind.Web/Notifications/NotificationService.cs`
- `src/Polymind.Web/Reporting/CsvExportEndpoints.cs`
- `src/Polymind.Domain/Entities/Payment.cs`, `Expense.cs`, `Receipt.cs`
- `src/Polymind.Domain/Enums/Enums.cs`
- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Polymind.Infrastructure/Persistence/DemoDataSeeder.cs` (seed-only assignments)
- `tests/Polymind.Tests/M10_FinanceRulesTests.cs`

### Files Changed

- `src/Polymind.Domain/Finance/PaymentPostingRules.cs`
- `src/Polymind.Web/Finance/PaymentPostingService.cs`
- `src/Polymind.Web/Components/Pages/Finance/Finance.razor`
- `src/Polymind.Web/Components/Pages/Finance/PaymentDialog.razor`
- `tests/Polymind.Tests/M10_FinanceRulesTests.cs`

### Symbols Changed

- `PaymentPostingRules.HasUnpaidEarlierStage`
- `PaymentPostingService.MarkPaidAsync`
- `PaymentPostingResult`
- `Finance.ApprovePayment`
- `Finance.MarkStagePaid`
- `PaymentDialog.Save`
- `PaymentDialog.FormModel.ApplyTo`
- `M10_FinanceRulesTests.Posting_stage_*`

### Fix

- Thêm `PaymentPostingService` làm đường duy nhất chuyển Payment sang Paid ở runtime Web.
- Service kiểm mọi stage trước có Paid hay chưa, set `ApprovedBy`, `PaidDate`, `UpdatedAt`, ghi audit, lưu Payment, rồi gọi `CommissionEngine.EnsureAsync` nếu là stage payment.
- `ApprovePayment` và `MarkStagePaid` cùng gọi service và cùng hiển thị commission count.
- `PaymentDialog` tách việc apply các field khỏi việc apply Status. Khi có transition sang Paid, dialog re-check thêm `payments:approve` và gọi service; nếu kiểm tuần tự fail thì không lưu cả các edit đang pending.
- Khoản thu lẻ (`Stage=null`) vẫn được duyệt nhưng không kích hoạt hoa hồng.
- Demo seeder giữ nguyên vì là đường tạo dữ liệu seed, không phải runtime business action.

### Why This Fix Is Correct

- BF-M10-02/03/04 và TC_M10_009/010/011 yêu cầu mọi đường đưa stage payment sang Paid có cùng tuần tự + commission side effect.
- `PaymentPostingRules` dùng đúng thứ tự số của `PaymentStage` đã được contract test khóa.
- M09 invariant được tái sử dụng: mỗi mốc chỉ sinh một commission kể cả service bị gọi từ nhiều UI path.
- Permission create/update không bị dùng thay cho approve: dialog phải có `payments:approve` khi transition Paid.
- U2 không đổi: không thêm logic Refund/hoàn tiền khi đổi JobOrder.

### Alternatives Considered

- Chặn Paid trong tab Khoản thu/dialog đối với stage payment: nhỏ hơn nhưng tạo UX không nhất quán và vẫn để logic phân tán.
- Copy tuần tự + CommissionEngine vào từng caller: dễ tái phát khi có đường thứ tư.
- Chuyển cả Finance UI sang service lớn: ngoài phạm vi; bản sửa chỉ gom transition Paid.

### Impact

- **API impact:** không đổi endpoint/contract; module dùng Blazor Server.
- **Database impact:** không migration/schema change.
- **UI impact:** Duyệt ở tab Khoản thu và chọn Paid trong dialog giờ chặn vượt thứ tự và có thể báo commission mới.
- **Security impact:** tăng re-check `payments:approve` cho dialog transition Paid; không làm yếu quyền.
- **Backward compatibility:** thu lẻ, sửa khoản chưa Paid, receipt và reports giữ nguyên.
- **Data compatibility:** không đổi enum/entity columns; dữ liệu Paid cũ không bị viết lại.

### Regression Risks

- Caller mới chuyển Paid phải dùng service; kiểm kê hiện tại cho thấy chỉ còn một assignment runtime.
- Service lưu Payment trước commission; nếu lỗi không thuộc unique idempotency xảy ra ở CommissionEngine, Payment đã Paid và lỗi phải được xử lý/điều tra thay vì rollback âm thầm. Đây cũng là boundary đã dùng ở M09.
- Không thay đổi behavior chuyển Paid về trạng thái khác vì requirement hoàn/refund chưa được chốt.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `Posting_stage_is_blocked_when_an_earlier_stage_is_unpaid` | Unit | Passed | TC_M10_007/009 |
| `Posting_stage_is_allowed_when_earlier_stages_are_paid` | Unit | Passed | TC_M10_009..011 |
| `Posting_stage_ignores_unpaid_later_stages` | Unit | Passed | Deposit không bị stage sau chặn |
| PostgreSQL posting probe | Integration | Passed | out-of-order blocked; 4 Paid stages; 3 commissions; loose=0; 5 audits |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --no-restore --nologo` | Regression | Passed | 51 passed, 0 failed, 0 skipped |
| Web build output riêng | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** Domain rules, PostgreSQL posting flow, shared regression, Web build.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** bUnit/Playwright UI matrix chưa có harness; core posting path đã đo trực tiếp trên PostgreSQL.

### Verification Instructions for Claude

- Chạy lại full test suite và Web/solution build.
- Kiểm source: chỉ `PaymentPostingService` được assignment `PaymentStatus.Paid` trong runtime Web.
- Với lịch 4 bước, thử `ApprovePayment` bước 2 trước bước 1: phải cảnh báo và không đổi Payment/audit/commission.
- Paid tuần tự bằng cách trộn ba entry point: tab Tiến độ, tab Khoản thu, PaymentDialog. Mỗi entry point phải set actor/date/audit và sinh đúng mốc commission.
- Duyệt Settlement qua tab Khoản thu/dialog; xác nhận Departure commission phát sinh ngay.
- Tạo/duyệt thu lẻ `Stage=null`; xác nhận Paid nhưng không sinh commission.
- Role thiếu `payments:approve` nhưng có create/update (nếu tạo role test): dialog không được set Paid.
- Kiểm receipt idempotent, report doanh thu Paid và U2 reset JobOrder không refund.
