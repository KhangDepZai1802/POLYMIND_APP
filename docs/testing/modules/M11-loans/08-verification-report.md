# M11 — Loans & Debt Collection · Verification Report

> **Người xác minh:** Claude (Independent Verification Engineer) — 2026-07-11 (phiên #6).
> **Phạm vi:** BUG_M11_01 + CR-M11-1 + CR-M11-2 + CR-M11-3 theo `07-fix-report.md`.
> **Nguyên tắc:** đọc diff thật, không dựa kết quả Codex; không sửa business logic; chạy lại test + build; **áp migration trên DB test riêng** (không đụng production/dev DB `polymind`).

## Kết luận tổng: **VERIFIED FIXED** (4/4) — có bằng chứng RUNTIME (migration + DB-level PoC)

| Hạng mục | Verdict | Mức bằng chứng |
|---|---|---|
| BUG_M11_01 — Bank loan không gate B20 | **Verified Fixed** | Code + unit + runtime schema |
| CR-M11-1 — Thu nợ/tất toán chỉ finance | **Verified Fixed** | Code + unit |
| CR-M11-2 — Thu nợ sinh phiếu thu + chống trùng | **Verified Fixed** | Code + unit + **DB unique index PoC** |
| CR-M11-3 — Không miễn nợ; chỉ tất toán khi thu đủ 100% | **Verified Fixed** | Code + unit |

Đây là lần đầu trong loạt QA áp được migration lên DB test (Docker/Postgres đang chạy), nên M11 có bằng chứng runtime mạnh hơn code-level của M06/M09/M10/M12.

---

## 1. Môi trường & lệnh chạy

| Bước | Lệnh | Kết quả |
|---|---|---|
| Suite | `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo` | **Passed 88, Failed 0, Skipped 0** |
| Web build | `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore -p:OutputPath=C:\tmp\polymind-m11-verify\` | **0 Warning, 0 Error** |
| Migration | `dotnet ef database update --connection "…Database=polymind_m11_verify…"` (DB test mới, fresh) | Áp sạch tới `20260711123000_LinkLoanDebtCollectionReceipts` → **Done** |
| Schema check | `\d receipts` trên DB test | Có `loan_id` (nullable, index thường), `loan_repayment_id` (nullable, **UNIQUE**) |
| DB PoC unique | 2 insert cùng `loan_repayment_id` | Insert 1 OK; insert 2 **fail** `duplicate key … ix_receipts_loan_repayment_id` |
| DB PoC null | 2 insert `loan_repayment_id = NULL` | **Cả 2 OK** (NULL distinct) — khớp residual "collect-all không có op-key" |

> DB test `polymind_m11_verify` đã **DROP** sau khi xác minh; không ghi vào `polymind`/production.

---

## 2. BUG_M11_01 — Bank loan không được gate B20 · **Verified Fixed**

- **Gate nguồn:** `CandidateDetail.razor:1143` nay `_hasOpenLoan = loan is not null && LoanCollectionRules.BlocksWorkflowCompletion(loan.Kind, loan.Status)`. Không còn `Status != Settled` bất kể Kind.
- **Domain rule:** `LoanCollectionRules.BlocksWorkflowCompletion` = `kind == Company && status != Settled`. → Bank (mọi status) **không bao giờ** chặn B20; chỉ Company chưa Settled chặn.
- **UI dropdown:** `LoanDialog.AvailableLoanStatusOptions` → Bank chỉ `Borrowing/Disbursed` (ẩn Settled); `OnKindChanged` tự đưa Settled→Disbursed khi đổi sang Bank.
- **Server guard:** `ValidateStatusChange` từ chối `Bank → Settled` kể cả gọi thẳng, không chỉ dựa UI.
- **Unit:** `Workflow_gate_only_blocks_unsettled_company_debt` (Bank/Borrowing=false, Bank/Disbursed=false, Company/Borrowing=true, Company/Settled=false) + `Bank_loan_cannot_be_marked_settled` — pass.

## 3. CR-M11-1 — Thu nợ/đổi tất toán chỉ finance · **Verified Fixed**

- `BusinessRoleAccess.CanCollectDebt` = **chỉ** SuperAdmin + Accountant.
- `DebtCollection`: `_canCollect` = `loans:update` ∧ `receipts:create` ∧ `CanCollectDebt`; **re-check lại** trong `CollectAsync` trước mọi mutation (không tin nút UI).
- `LoanDialog`: non-finance với Company chỉ thấy `Borrowing/Disbursed` (giữ Settled hiển thị chỉ khi current đã Settled, không cho set mới); `ValidateStatusChange` chặn non-finance đổi vào/ra khỏi Settled ("Chỉ Kế toán hoặc Super Admin…").
- **Không làm yếu quyền cũ:** RM/recruiter/consultant vẫn `CanEditLoan` → vẫn tạo/sửa thông tin hồ sơ vay, chỉ mất thao tác tài chính (thu tiền/tất toán). Đúng segregation-of-duties U-M11-1.
- **Unit:** `Non_finance_actor_cannot_change_settlement_status` — pass.

## 4. CR-M11-2 — Thu nợ sinh phiếu thu + chống trùng · **Verified Fixed**

- `CollectAsync` mở transaction: gọi `LoanCollectionRules.Collect` → tạo `Receipt{ ReceiptType.Income, CandidateId, LoanId, LoanRepaymentId (null khi thu hết) }` + 2 audit (`create receipts`, `collect_debt loans`) → `SaveChanges` → `Commit`.
- **Chống trùng (2 lớp):**
  - App: trước khi thu 1 kỳ, `AnyAsync(r => r.LoanRepaymentId == iid)` → nếu đã có phiếu thì báo và thoát.
  - DB: **unique index `ix_receipts_loan_repayment_id`** — đã chứng minh runtime: insert trùng `loan_repayment_id` bị Postgres từ chối. → double-click/2 request song song **không** tạo phiếu thứ hai cho cùng một kỳ.
- Receipt gắn `CandidateId` nên tự lên Finance/Reports theo ứng viên (không dựng sổ phụ song song). Migration additive, cột nullable → tương thích ngược receipts cũ.
- **Unit:** `Loan_receipt_migration_is_discoverable_and_adds_source_links` (2 cột + 2 index, unique đúng chỗ) + `Collecting_one_installment_keeps_loan_open_until_every_installment_is_paid` — pass.

## 5. CR-M11-3 — Không miễn nợ; chỉ tất toán khi thu đủ 100% · **Verified Fixed**

- `ValidateStatusChange`: `Settled` khi `outstanding > 0` → chặn, báo đúng số dư ("Khoản nợ còn N đ chưa thu; phải thu đủ 100%…") — **kể cả accountant** (canCollectDebt=true vẫn bị chặn khi còn dư).
- `Collect`: thu đúng `Amount - PaidAmount` mỗi kỳ; **chỉ** auto-settle khi `outstanding <= 0` (hoặc loan không có lịch, thu đủ `Amount`). Nếu đang Settled mà thu lẻ còn dư → hạ về Disbursed.
- **Không có đường write-off/miễn nợ** trong toàn bộ diff (grep `Collect`, `Settled`, `MarkPaid`, dialog): không có nút/nhánh nào set Settled bỏ qua tiền. Dữ liệu legacy Settled-còn-dư được `DebtCollection` xếp lại vào nhóm "đang nợ" (`_owing`) và vẫn thu được.
- Khớp memory `polymind-no-debt-forgiveness` + WORKLOG "🚫 KHÔNG BAO GIỜ MIỄN NỢ".
- **Unit:** `Settled_status_is_rejected_while_company_debt_is_outstanding`, `Collect_remaining_marks_all_installments_paid_and_settles_loan`, `Collect_remaining_without_schedule_collects_full_company_debt` — pass.

---

## 6. Kiểm tra chống né-test / hard-code / regression

- **Không sửa test để né bug:** các test M11 kiểm hành vi thật (gate theo Kind, chặn Settled khi còn dư, thu đủ mới settle), không nới assertion.
- **Không hard-code/workaround:** rule đặt ở `Domain/Loans/LoanCollectionRules` (thuần), UI + server cùng gọi một nguồn.
- **Regression:** suite chung 88/88, Web 0/0. Không đổi state-machine workflow M07, không đổi authorization core M02, không sửa dữ liệu production.

## 7. Residual / Observations (non-blocking — ghi rõ, KHÔNG chặn Verified)

| ID | Nội dung | Severity | Ghi chú |
|---|---|---|---|
| R-M11-A | **Collect-all song song** trên cùng loan có thể tạo 2 phiếu (`loan_repayment_id=NULL` không bị unique chặn — đã chứng minh runtime). App-level `Collect` lần 2 trả Fail khi tuần tự; race thì hở. | Low | Cùng lớp no-rowversion (OBS-M07-01/M08-01/M11-01). Đề xuất op-key nếu user coi trọng. |
| R-M11-B | Receipt Code `RC-yyyyMMdd-Random(1000,9999)` có nguy cơ trùng hiếm (unique index Code sẽ ném lỗi → thao tác fail, không tạo trùng). | Low | Pre-existing (cùng OBS-M10-02). |
| R-M11-C | Gate B20 tin `Status==Settled`; **dữ liệu legacy** Company/Settled nhưng còn dư → gate mở. Luồng mới đã chặn tạo hàng lỗi này; DebtCollection vẫn hiện là "đang nợ". | Low | Chỉ ảnh hưởng dữ liệu cũ; cân nhắc migration đối soát nếu có. |
| OBS-M11-01/04/06 | non-unique `loans.candidate_id`; không hỗ trợ `Partial`; xóa loan để lại orphan `loan_repayments`. | Low | Backlog ngoài phạm vi fix này (như bug report ghi). |

## 8. Chưa đo (khai báo trung thực)

- **E2E UI thật** (đăng nhập accountant/recruiter, bấm "Thu kỳ tới"/"Thu hết", xem phiếu thu PDF, kiểm gate B20 trên trình duyệt): chưa chạy — không có bUnit/Playwright harness. Đã thay bằng: đọc source đường mutation + unit rule + migration runtime + DB unique PoC.
- **PDF phiếu thu nợ hiển thị ở Finance:** xác nhận ở mức code (Receipt gắn CandidateId, `receipts:read` gate). Chưa render PDF thật.

## 9. Cập nhật trạng thái

- `06-bug-report.md`: BUG_M11_01 + CR-M11-1/2/3 → **Verified Fixed**.
- Board: `QA=Completed`, `Codex=Fixed`, `Verification=Verified` (code + runtime migration/DB PoC).
- M11 rời Verification Queue.
