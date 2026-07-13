# M11 — Loans & Debt Collection · Automation Report

## Framework & môi trường

- **Framework:** xUnit (`tests/Polymind.Tests/Polymind.Tests.csproj`, net10.0).
- **Reference:** `Polymind.Domain` (+ Infrastructure/Application). **KHÔNG** reference `Polymind.Web` → không unit-test được logic nằm trong razor component.
- **Môi trường:** Local. Không dùng production. Không cần secret.

## Test structure (M11)

- `tests/Polymind.Tests/M11_LoanRulesTests.cs` — contract + regression M11:
  - `LoanStatus_settled_is_the_terminal_settlement_state` (TC_M11_037)
  - `LoanKind_distinguishes_bank_and_company` (TC_M11_038)
  - `LoanRepaymentStatus_contains_lifecycle_states` (TC_M11_039)
  - `New_loan_defaults_to_bank_and_borrowing` (TC_M11_040)
  - `New_loan_repayment_defaults_to_pending` (TC_M11_041)
  - gate B20 Bank/Company (BUG_M11_01)
  - chặn Bank/Settled, chặn outstanding/Settled, chặn non-finance settlement
  - thu một kỳ, thu hết nhiều kỳ, thu hết loan không có lịch
  - migration Receipt source link/index contract

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore -p:OutputPath=<workspace>/.qa/m11-tests/
# Passed! Failed: 0, Passed: 82, Skipped: 0
```

| Loại | Số | Ghi chú |
|---|---|---|
| Pass | 82 | Toàn suite tại Codex handoff M11 |
| Fail | 0 | — |
| Skipped | 0 | — |
| Blocked | — | Runtime DB/UI + công thức tài chính (xem dưới) |

## Phân loại & lý do Blocked

- **Application Defect:** BUG_M11_01 + CR-M11-1/2/3 Fixed, chờ Claude verify.
- **Test Code Defect:** 0.
- **Environment Defect:** thiếu harness integration (WebApplicationFactory + DB test) và bUnit → mọi flow runtime tạo/sửa/thu/xóa/advance B20 **Blocked**.
- **Test Data Defect:** 0.
- **Requirement Ambiguity:** 0 trong phạm vi fix M11; user đã chốt U-M11-1/2/3.

## Vì sao công thức lãi/lịch chưa được unit-test

`RegenerateScheduleAsync` (lãi đơn `gốc×%/100×tháng/12`, chia đều `remaining/monthsLeft`, bù dư kỳ cuối, giữ kỳ đã thu) vẫn nằm trong `LoanDialog.razor` (Web). Fix này chỉ tách luật gate/thu/tất toán sang Domain; công thức schedule vẫn là backlog riêng TC_M11_003/004/027.

## Automation backlog

| Hạng mục | Layer | Điều kiện |
|---|---|---|
| Formula lãi + chia kỳ + bù dư | Unit | Tách `LoanScheduleRules` sang Domain |
| Create/edit/delete loan qua DB | Integration | WebApplicationFactory + DB test |
| MarkPaid + auto-settle | Integration | DB test |
| Gate B20 (loan chưa/đã tất toán) | Integration/E2E | DB + workflow harness |
| Role/scope matrix (agent/parent/student) | Integration | Auth harness |
| Concurrency 2 loan/candidate (OBS-M11-01) | Integration | DB probe (Testcontainers) |
| Orphan repayment sau delete (OBS-M11-06) | Integration | DB probe |
