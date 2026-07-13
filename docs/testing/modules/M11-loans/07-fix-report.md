# Module Fix Report

## Summary

- **Module ID:** M11
- **Module Name:** Loans & Debt Collection
- **Bugs/changes received:** BUG_M11_01, CR-M11-1, CR-M11-2, CR-M11-3
- **Fixed:** 4
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Verification:** Chờ Claude xác minh độc lập; Codex không đánh dấu `Verified Fixed`.

## BUG_M11_01 — Bank loan chặn sai gate B20

### Status

**Fixed — waiting for Claude verification.**

### Investigation / Root Cause

`CandidateDetail.Load` lấy loan mới nhất rồi đặt `_hasOpenLoan = Status != Settled`, không xét `LoanKind`. Vì vậy vay ngân hàng `Borrowing/Disbursed` bị xem như nghĩa vụ với công ty và chặn bước `OverseasSupport → Completed`. `LoanDialog` đồng thời dùng cùng một danh sách trạng thái cho Bank/Company nên cho chọn `Settled` với Bank.

### Evidence

- Source path tái hiện: `CandidateDetail.razor` loan load → `_hasOpenLoan`; gate ở `AdvanceStep` trả về khi `_hasOpenLoan=true`.
- Regression `Workflow_gate_only_blocks_unsettled_company_debt`: Bank/Borrowing và Bank/Disbursed không chặn; Company/Borrowing chặn; Company/Settled không chặn.
- Regression `Bank_loan_cannot_be_marked_settled`: domain rule từ chối `Bank → Settled`.

### Fix

- Thêm `LoanCollectionRules.BlocksWorkflowCompletion` và dùng trực tiếp trong `CandidateDetail`.
- Dropdown Bank chỉ còn `Borrowing/Disbursed`; đổi kind sang Bank tự đưa lựa chọn `Settled` về `Disbursed`.
- Server validation tiếp tục từ chối Bank/Settled, không chỉ dựa vào UI.

### Why This Fix Is Correct

Khớp BF-M11-06 và quyết định user 2026-07-11: chỉ nợ công ty chưa tất toán là nghĩa vụ với công ty và được phép gate B20. Không đổi state-machine workflow, authorization hoặc dữ liệu production.

## CR-M11-1 — Thu nợ/tất toán chỉ finance

### Status

**Fixed — waiting for Claude verification.**

### Investigation / Root Cause

`DebtCollection` trước đây dùng `loans:update + CanEditLoan`, mà `CanEditLoan` gồm RM/recruiter/consultant. Đây là quyền sửa hồ sơ, không phải quyền xác nhận tiền thực thu.

### Fix

- Thêm `BusinessRoleAccess.CanCollectDebt`, chỉ `super_admin` và `accountant`.
- Page initialization và mọi mutation đều re-check `loans:update`, `receipts:create` và `CanCollectDebt`.
- `LoanDialog` chỉ cho finance role thay đổi trạng thái liên quan `Settled`; domain rule vẫn kiểm tra server-side.

### Why This Fix Is Correct

Giữ nguyên quyền tạo/sửa thông tin loan của RM/recruiter/consultant, chỉ tách thao tác tài chính theo U-M11-1. Không làm yếu permission hiện có.

## CR-M11-2 — Thu nợ sinh phiếu thu

### Status

**Fixed — waiting for Claude verification.**

### Investigation / Root Cause

`MarkPaid` chỉ cập nhật `LoanRepayment` và audit loan; không tạo `Receipt`, nên tiền thu nợ không có chứng từ trong Finance/Reports.

### Fix

- Mọi lần thu một kỳ hoặc thu hết đều tạo `ReceiptType.Income` trong cùng DB transaction.
- Receipt gắn `CandidateId`, `LoanId`; thu một kỳ còn gắn `LoanRepaymentId`.
- Thêm migration `20260711123000_LinkLoanDebtCollectionReceipts`: hai cột nguồn, index loan và unique index nullable trên repayment để một kỳ không có hai phiếu thu.
- Ghi audit `create receipts` và `collect_debt loans` cùng receipt/amount/settlement result.

### Why This Fix Is Correct

Phiếu thu đi vào bảng `receipts` hiện có nên tự xuất hiện trong Finance/Reports theo `CandidateId`; không tạo hệ thống sổ phụ song song. Migration chỉ được tạo trong source, chưa áp vào database nào.

## CR-M11-3 — Không miễn nợ; chỉ tất toán khi thu đủ 100%

### Status

**Fixed — waiting for Claude verification.**

### Investigation / Root Cause

`LoanDialog` cho đặt `Settled` tự do dù các kỳ còn dư. `MarkPaid` chỉ hỗ trợ từng kỳ và không có đường thu hết cho khoản nợ không có lịch. Trạng thái sai còn làm gate B20 mở sớm.

### Fix

- `LoanCollectionRules.ValidateStatusChange` chặn `Settled` khi outstanding > 0 và báo đúng số tiền còn thiếu.
- `LoanCollectionRules.Collect` thu đúng phần còn lại (`Amount - PaidAmount`), chỉ auto-settle khi outstanding về 0.
- Thêm nút **Thu hết phần còn lại**, có confirm số tiền thực thu; hỗ trợ cả loan có nhiều kỳ lẫn loan chưa có lịch.
- Không triển khai write-off/miễn nợ. Dữ liệu legacy có `Settled` nhưng còn dư được xếp lại vào nhóm đang nợ và vẫn có thể thu đủ.

### Why This Fix Is Correct

Khớp quy tắc cứng U-M11-3 mới nhất: không có bất kỳ đường miễn nợ; `Settled` chỉ là kết quả của thu đủ tiền thật. Thu tiền, receipt, repayment, loan status và audit được lưu atomically.

## Files Inspected

- `CandidateDetail.razor`, `LoanDialog.razor`, `DebtCollection.razor`, `Loans.razor`
- `Loan.cs`, `LoanRepayment.cs`, `Receipt.cs`, `Enums.cs`
- `BusinessRoleAccess.cs`, `DbSeeder.cs`, `ApplicationDbContext.cs`
- `Finance.razor`, `PaymentPostingService.cs`, `CsvExportEndpoints.cs`
- M11 `01-analysis.md` → `06-bug-report.md`; board/checkpoint/worklog

## Files Changed

- `src/Polymind.Domain/Loans/LoanCollectionRules.cs`
- `src/Polymind.Domain/Entities/Receipt.cs`
- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260711123000_LinkLoanDebtCollectionReceipts.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`
- `src/Polymind.Web/Components/Pages/Loans/LoanDialog.razor`
- `src/Polymind.Web/Components/Pages/Loans/DebtCollection.razor`
- `src/Polymind.Web/Components/_Imports.razor`
- `src/Polymind.Web/Display/BusinessRoleAccess.cs`
- `tests/Polymind.Tests/M11_LoanRulesTests.cs`

## Symbols Changed

- `LoanCollectionRules.BlocksWorkflowCompletion`, `Outstanding`, `ValidateStatusChange`, `Collect`
- `BusinessRoleAccess.CanCollectDebt`
- `LoanDialog.AvailableLoanStatusOptions`, `SaveAsync`, `OnKindChanged`
- `DebtCollection.CollectRemaining`, `CollectAsync`, `MarkPaid`
- `Receipt.LoanId`, `Receipt.LoanRepaymentId`

## Impact

- **API:** none; M11 has no REST endpoint.
- **Database:** additive nullable receipt source columns + indexes; migration required before using new collection UI.
- **UI:** Bank hides Settled; debt page restricts finance actions and adds collect-all.
- **Security:** stronger segregation of duties; mutation re-check retained.
- **Backward compatibility:** existing receipts remain valid because new columns nullable. Existing bad Company/Settled rows with outstanding are shown as owing.
- **Data compatibility:** no production data modified; no automatic forgiveness/deletion/mass update.

## Regression Risks

- Runtime PostgreSQL transaction/receipt rendering still needs integration/UI verification because repository lacks DB/E2E harness.
- Receipt code generation keeps the existing `RC-yyyyMMdd-####` convention and its pre-existing rare collision risk.
- Concurrent collect-all on the same loan has no dedicated operation key; per-installment duplicate receipt is DB-protected by the unique index.

## Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --no-restore -p:OutputPath=...` | Unit/regression | **Passed 82/82** | 0 failed, 0 skipped; includes M11 gate/status/collection/migration tests |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore -p:OutputPath=...` | Build | **Passed** | 0 warning, 0 error |
| `git diff --check` | Static | **Passed** | No whitespace errors |

## Test Results

- **Passed:** 82 tests; Web build.
- **Failed:** 0 application tests/build errors.
- **Skipped:** 0.
- **Blocked:** runtime DB/UI verification (no integration harness; migration not applied).

## Verification Instructions for Claude

1. Đọc diff thật; không dựa vào kết quả Codex.
2. Chạy lại suite và Web build.
3. Apply migration trên DB test, không dùng production.
4. Với Bank/Borrowing và Bank/Disbursed ở B20: phải hoàn thành được; dropdown không có Settled.
5. Với Company còn dư: B20 phải chặn; save `Settled` phải báo số dư, kể cả accountant.
6. Recruiter/RM/consultant: vẫn sửa thông tin loan nhưng không thấy/không gọi được thu kỳ, thu hết hoặc đổi settlement.
7. Accountant/SuperAdmin: thu một kỳ sinh đúng một Income Receipt gắn Candidate/Loan/Repayment; double action không sinh phiếu thứ hai.
8. Thu hết: receipt amount bằng tổng outstanding; tất cả kỳ Paid; loan Settled; B20 mở. Lặp với loan không có lịch.
9. Kiểm tra Finance/receipt PDF thấy phiếu thu nợ; audit có `create receipts` và `collect_debt`.
10. Không đánh dấu Verified nếu chưa kiểm tra rule **không bao giờ miễn nợ**.
