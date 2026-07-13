# M10 — Finance · 05 Automation Report

> QA: Claude · 2026-07-10.

## Framework & dependency

- `tests/Polymind.Tests` (xUnit, .NET 10). Ref Domain + Infrastructure — **KHÔNG** ref Web.
- Logic tài chính (`Finance.razor`, `PaymentSchedule.Split`, `FinanceEligibility`, dialog) nằm ở `Polymind.Web` → không unit-test trực tiếp. Cần bUnit + WebApplicationFactory/DB test (chưa dựng).

## Test phiên này

| File | Test | TC | Loại | Kết quả |
|---|---|---|---|---|
| `tests/Polymind.Tests/M10_FinanceRulesTests.cs` | `PaymentStage_is_ordered_1_to_4_deposit_to_settlement` | TC_M10_030 | unit (Domain enum) | **Pass** |
| | `New_payment_defaults_to_pending` | TC_M10_031 | unit | **Pass** |
| | `PaymentStatus_contains_lifecycle_states` | TC_M10_032 | unit | **Pass** |
| | `ReceiptType_distinguishes_income_and_expense` | TC_M10_033 | unit | **Pass** |

**Phạm vi:** chốt thứ tự `PaymentStage` (enforcement đóng tuần tự dùng `(int)stage`) + default/enum. **KHÔNG** phủ split 20/30/30/20, tuần tự, trigger hoa hồng (những phần đó ở razor/Web).

## Lệnh chạy

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả tổng

- **Passed: 41 · Failed: 0 · Skipped: 0** (37 trước + 4 M10). Build test 0/0.

## Phân loại & trạng thái

| Nhóm | Trạng thái | Ghi chú |
|---|---|---|
| Enum/entity contract | **Automated Pass** (4) | Phiên này |
| Split 20/30/30/20 bù dư | **Verified (code)** | `PaymentSchedule.Split` — bước cuối = total − running. Blocked automation (Web). |
| Đóng tuần tự 1→4 | **Verified (code)** | `MarkStagePaid` siblings check. |
| **Trigger hoa hồng đa đường** | **Application Defect → BUG_M10_01** | Chỉ MarkStagePaid gọi EnsureAsync; ApprovePayment/edit thì không. |
| Idempotency hoa hồng concurrency | **→ BUG_M09_01** | Cùng handoff M09. |
| Attribution actor | **Verified (code)** | `GetRequiredUserIdAsync` khắp nơi. |
| Authz + IDOR + self-scope | **Verified (code)** | page/dialog/action re-check; lọc OwnedCandidateId; PDF gated. |
| Receipt idempotent | **Verified (code)** | AnyAsync trước tạo. |
| U2 no-refund | **Verified (code)** | Không có refund logic. |
| Khoản chi duyệt | **Req → OBS-M10-01** | Không UI duyệt. |

## Environment / data issues

- Không DB test / bUnit / Playwright → split/tuần tự/commission/PDF runtime không đo (Blocked, không suy đoán).

## Automation backlog (đề xuất — KHÔNG làm ở QA)

1. **Integration (WebApplicationFactory + DB):** split 20/30/30/20 khớp tổng; tuần tự 1→4; MarkStagePaid trigger commission; **repro BUG_M10_01** (ApprovePayment/edit không trigger); **repro BUG_M09_01** (race); U2 no-refund; receipt idempotent.
2. **bUnit:** dialog re-check + validation.
3. **e2e:** self-scoped tiến độ + PDF gate + Director không ghi nhận.
4. **Refactor gợi ý (Codex):** tách `PaymentSchedule` sang Domain + gom set-Paid vào 1 hàm (ép tuần tự + EnsureAsync).
