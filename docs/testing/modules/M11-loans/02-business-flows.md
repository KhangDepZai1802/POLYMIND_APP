# M11 — Loans & Debt Collection · Business Flows

> Dựng từ source thật (`Loans.razor`, `LoanDialog.razor`, `DebtCollection.razor`, `CandidateDetail.razor` B20 gate). Không suy đoán ngoài code.

## BF-M11-01 — Tạo hồ sơ hỗ trợ vay (ngân hàng hoặc nợ công ty)

- **Actor/Role:** super_admin, accountant, RM, recruiter, consultant (có `loans:create` + `CanEditLoan`).
- **Preconditions:** ứng viên tồn tại và **chưa có** hồ sơ vay (SearchCandidates lọc bỏ ứng viên đã có loan).
- **Initial state:** không có `Loan` cho candidate.
- **Input:** candidate, Kind (Bank/Company), Status, Amount, TermMonths, BankName (Bank), InterestRate, DisbursedDate, (Company) MonthlyDeduction/DeductionStart + checkbox tạo lịch, Note.
- **Main flow:** mở LoanDialog → chọn ứng viên → chọn Kind → nhập số liệu → Lưu → re-check permission+role → tạo `Loan` (Code `VAY-yyyyMMdd-####`, CreatedBy=actor) → nếu Company+schedule → `RegenerateScheduleAsync` → SaveChanges → audit `create`.
- **Alternate:** Kind=Bank → xóa MonthlyDeduction/DeductionStart + repayments (nếu có).
- **Error flow:** chưa chọn ứng viên → cảnh báo; thiếu quyền → cảnh báo, không lưu.
- **Validation:** candidateId != empty; (schedule) Amount>0 & Term>0.
- **AuthZ:** `loans:create` + `CanEditLoan`.
- **DB changes:** insert `loans`; (Company+schedule) insert N `loan_repayments`.
- **Notification:** không.
- **Audit:** `create` loans (CandidateId, Kind, Status, Amount, TermMonths, BankName).
- **Final state:** Loan tồn tại; (Company) có lịch trả góp Pending.
- **Page/API:** `/loans` → LoanDialog. Không API.
- **Risk:** 2 loan/candidate nếu race (OBS-M11-01); Bank không sinh lịch (đúng).

## BF-M11-02 — Sinh lịch trả góp nợ công ty (gốc + lãi, trừ dần vào lương)

- **Actor/Role:** như BF-M11-01, Kind=Company, tick "Tạo lịch trả góp".
- **Input:** Amount (gốc), TermMonths, InterestRate (%/năm), DeductionStart.
- **Main flow (`RegenerateScheduleAsync`):**
  1. principal=Amount, term=TermMonths; nếu ≤0 → cảnh báo, không tạo.
  2. **lãi đơn** = round(principal × rate/100 × term/12).
  3. total = principal + interest.
  4. **giữ** các kỳ đã có `PaidAmount>0` (kept); **xóa** kỳ chưa thu (`PaidAmount<=0`).
  5. remaining = max(0, total − Σ kept.PaidAmount); monthsLeft = max(1, term − kept.Count); startNo = kept.Max(No)+1 (hoặc 1).
  6. per = round(remaining / monthsLeft); mỗi kỳ = per, **kỳ cuối** = remaining − per×(monthsLeft−1) (bù dư).
  7. DueDate = startDate.AddMonths(kept.Count + i).
- **Business rule:** tổng các kỳ = total (gốc+lãi); chia đều, kỳ cuối gánh phần dư làm tròn.
- **DB changes:** xóa repayment chưa thu + thêm repayment mới; giữ kỳ đã thu.
- **Risk:** tái sinh lịch không nhân đôi kỳ đã thu (đúng — kept giữ, unpaid thay).

## BF-M11-03 — Sửa hồ sơ vay

- **Actor/Role:** như BF-M11-01 với `loans:update`.
- **Main flow:** mở LoanDialog (candidate khóa) → load loan mới nhất → sửa field → Lưu → re-check `loans:update`+`CanEditLoan` → update loan + UpdatedAt → (Company+schedule) regen → audit `update`.
- **Đổi Kind Company→Bank:** xóa lịch trả góp + MonthlyDeduction/DeductionStart=null.
- **State:** Status set tự do qua dropdown (không state-machine) — xem OBS-M11-05.

## BF-M11-04 — Thu nợ từng kỳ (MarkPaid) + tự tất toán

- **Actor/Role:** super_admin, accountant, RM, recruiter, consultant (`loans:update`+`CanEditLoan`). **Chỉ nợ công ty có lịch.**
- **Preconditions:** Loan Kind=Company có `loan_repayments`; kỳ chưa Paid.
- **Main flow:** `/debt-collection` → "Thu kỳ tới" hoặc "Đã thu" kỳ cụ thể → re-check `_canUpdate` → load repayment → set PaidAmount=Amount, PaidDate=today(UTC), Status=Paid, UpdatedAt → nếu **tất cả** kỳ Paid → `loan.Status = Settled` → audit `update` loans (installment,PaidAmount) → SaveChanges.
- **Alternate:** kỳ không tồn tại → reload.
- **DB changes:** update repayment; (đủ kỳ) update loan.Status=Settled.
- **Audit:** `update` loans.
- **Final state:** kỳ Paid; khi đủ → loan Settled → mở gate B20.
- **Risk:** không guard "đã Paid" nhưng op idempotent; luôn full-amount (OBS-M11-04 không partial).

## BF-M11-05 — Xóa hồ sơ vay

- **Actor/Role:** super_admin, accountant (`loans:delete`+`CanDeleteLoan`).
- **Main flow:** `/loans` hoặc CandidateDetail → Xóa → confirm → re-check `loans:delete`+`CanDeleteLoan` (+ AgentScope ở Loans) → audit `delete` (snapshot loans) → `RemoveRange(loans)` theo candidate → SaveChanges.
- **DB changes:** xóa `loans` của candidate. **`loan_repayments` KHÔNG bị xóa** (không FK cascade) → orphan (OBS-M11-06).
- **Risk:** orphan repayment rows (data hygiene, Low).

## BF-M11-06 — Gate B20 (hoàn thành quy trình phụ thuộc tất toán)

- **Actor/Role:** người có quyền advance workflow bước cuối (OverseasSupport→Completed).
- **Preconditions:** cjo ở bước OverseasSupport (B19→B20).
- **Main flow (`AdvanceStep`, WorkflowStep.OverseasSupport):**
  1. `_hasOpenLoan` = latest loan tồn tại **và** Status != Settled.
  2. Nếu `_hasOpenLoan` → cảnh báo "Khoản vay chưa tất toán — chưa thể hoàn thành quy trình" → **return** (chặn).
  3. Ngược lại → confirm dialog → advance sang Completed.
- **State transition table (Loan → Gate B20):**

| Loan latest Status | Có loan? | `_hasOpenLoan` | Gate B20 |
|---|---|---|---|
| (không có loan) | Không | false | ✅ Cho hoàn thành (không vay) |
| Borrowing | Có | true | ❌ Chặn |
| Disbursed | Có | true | ❌ Chặn |
| Settled | Có | false | ✅ Cho hoàn thành |

- **Risk:** dùng **latest** loan → nếu tồn tại 2 loan (OBS-M11-01) và latest=Settled nhưng older chưa Settled → gate mở dù còn nợ (edge, cần race). Set Settled thủ công qua dialog cũng mở gate (OBS-M11-05).

## Kiểm tra state/transition tổng hợp

| Current | Action | Allowed Role | Condition | Next | DB | Notification | History |
|---|---|---|---|---|---|---|---|
| (none) | Create loan | edit roles | candidate chưa có loan | Borrowing/Disbursed | insert loans | — | audit create |
| Company loan | Generate schedule | edit roles | Amount>0, Term>0 | +repayments Pending | insert repayments | — | audit create/update |
| Repayment Pending | MarkPaid | edit roles | Kind=Company | Paid | update repayment | — | audit update |
| All repayments Paid | (auto) | — | thu đủ | Loan Settled | update loan | — | audit update |
| Any | Delete | delete roles | confirm | (removed) | delete loans (orphan repayments) | — | audit delete |
| Loan!=Settled | Advance B20 | workflow roles | — | **blocked** | — | — | — |
| Loan Settled/none | Advance B20 | workflow roles | confirm | Completed | cjo Completed | — | audit advance_step |
