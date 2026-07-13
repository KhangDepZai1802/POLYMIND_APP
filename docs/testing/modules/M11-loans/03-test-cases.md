# M11 — Loans & Debt Collection · Test Cases

> Quy ước `TC_M11_<NN>`. Expected result bám hành vi ĐÚNG theo spec (không sửa để pass). Nhiều case runtime **Blocked** vì chưa có bUnit/WebApplicationFactory + DB test harness — ghi rõ ở cột Status.

Ký hiệu Status: **Pass (code)** = xác minh đúng ở source; **Blocked (harness)** = cần runtime; **Obs** = tạo observation.

## Functional — Tạo/Sửa hồ sơ vay

| TC | Tên | Flow | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M11_001 | Tạo vay ngân hàng | BF-M11-01 | accountant | /loans → Thêm → Kind=Bank, chọn ứng viên, Amount, Bank, Lưu | Loan Bank tạo, Code `VAY-…`, CreatedBy=actor, KHÔNG có lịch trả góp | E2E | Blocked (harness) |
| TC_M11_002 | Tạo nợ công ty + lịch | BF-M11-01/02 | accountant | Kind=Company, Amount, Term, Rate, tick tạo lịch, Lưu | Loan Company + N kỳ Pending, tổng kỳ = gốc+lãi | E2E | Blocked (harness) |
| TC_M11_003 | Lãi đơn đúng công thức | BF-M11-02 | — | gốc=100tr, rate=12, term=12 | interest=round(100tr×0.12×12/12)=12tr; total=112tr | Unit (extract) | Blocked (logic ở Web) |
| TC_M11_004 | Chia đều + bù dư kỳ cuối | BF-M11-02 | — | total=112tr, 12 kỳ | 11 kỳ = round(112tr/12), kỳ cuối = 112tr − per×11 | Unit (extract) | Blocked (logic ở Web) |
| TC_M11_005 | Sửa loan giữ CreatedBy | BF-M11-03 | accountant | Sửa loan đã có | CreatedBy giữ nguyên, UpdatedAt đổi | E2E | Blocked (harness) |
| TC_M11_006 | Đổi Company→Bank xóa lịch | BF-M11-03 | accountant | Loan Company có lịch → đổi Bank → Lưu | repayments bị xóa, MonthlyDeduction/DeductionStart=null | E2E | Blocked (harness) |
| TC_M11_007 | Tạo thiếu ứng viên | BF-M11-01 | accountant | Không chọn ứng viên → Lưu | Cảnh báo "Vui lòng chọn ứng viên", không lưu | E2E | Pass (code) |
| TC_M11_008 | Tạo lịch thiếu Amount/Term | BF-M11-02 | accountant | Company, tick lịch, bỏ trống Amount | Cảnh báo cần Amount+Term, không sinh lịch | E2E | Pass (code) |
| TC_M11_009 | Autocomplete lọc ứng viên đã có loan | BF-M11-01 | accountant | Mở autocomplete | Chỉ hiện ứng viên chưa có loan | E2E | Pass (code) |

## Functional — Thu nợ & tất toán

| TC | Tên | Flow | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M11_010 | Thu 1 kỳ | BF-M11-04 | accountant | /debt-collection → "Thu kỳ tới" | Kỳ Paid, PaidAmount=Amount, PaidDate=today, audit update | E2E | Blocked (harness) |
| TC_M11_011 | Tự tất toán khi đủ kỳ | BF-M11-04 | accountant | Thu kỳ cuối cùng | Loan.Status=Settled tự động | E2E | Blocked (harness) |
| TC_M11_012 | Bank không hiện ở Thu nợ | BF-M11-04 | accountant | Tạo loan Bank → mở /debt-collection | Loan Bank KHÔNG xuất hiện | E2E | Pass (code) |
| TC_M11_013 | Progress bar đúng % | BF-M11-04 | accountant | Xem DebtCard | pct = paid/total×100 | E2E | Pass (code) |
| TC_M11_014 | Double-click "Đã thu" idempotent | BF-M11-04 | accountant | Bấm "Đã thu" 2 lần | Kỳ vẫn Paid=Amount (không cộng dồn) | E2E | Blocked (harness) |

## Authorization

| TC | Tên | Flow | Role | Steps | Expected | Status |
|---|---|---|---|---|---|---|
| TC_M11_015 | Chưa đăng nhập | — | anon | GET /loans | Redirect login (page `[Authorize(loans:read)]`) | Pass (code) |
| TC_M11_016 | Director không sửa được | BF-M11-03 | director | Mở /loans | Chỉ read; không nút Thêm/Sửa/Xóa (`loans:read` only) | Pass (code) |
| TC_M11_017 | Recruiter tạo/sửa được | BF-M11-01 | recruiter | Thêm loan | Thành công (`loans:create/update`+CanEditLoan) | Pass (code) |
| TC_M11_018 | Recruiter KHÔNG xóa | BF-M11-05 | recruiter | Nút Xóa | Không có nút (`loans:delete` thiếu) | Pass (code) |
| TC_M11_019 | Accountant xóa được | BF-M11-05 | accountant | Xóa loan | Thành công + audit delete | Pass (code) |
| TC_M11_020 | Agent chỉ thấy loan ứng viên mình | BF (scope) | agent | /loans | Chỉ loan của candidate thuộc agent (AgentScope.IsAgentOnly) | Pass (code) |
| TC_M11_021 | Agent read-only (không mutation) | — | agent | Thao tác | Không create/update/delete (chỉ `loans:read`) | Pass (code) |
| TC_M11_022 | Parent/Student self-scope | BF (scope) | parent | /loans | Chỉ loan của con/mình (OwnedCandidateId) | Pass (code) |
| TC_M11_023 | MarkPaid không quyền → chặn | BF-M11-04 | director | (nếu vào được) MarkPaid | `_canUpdate=false` → cảnh báo, không đổi DB | Pass (code) |

## State / Business rule

| TC | Tên | Flow | Steps | Expected | Status |
|---|---|---|---|---|---|
| TC_M11_024 | Gate B20 chặn khi chưa tất toán | BF-M11-06 | Loan Borrowing → advance B20 | Chặn "Khoản vay chưa tất toán" | Pass (code) |
| TC_M11_025 | Gate B20 mở khi Settled | BF-M11-06 | Loan Settled → advance B20 | Confirm → Completed | Pass (code) |
| TC_M11_026 | Gate B20 mở khi không vay | BF-M11-06 | Không có loan → advance B20 | Confirm → Completed | Pass (code) |
| TC_M11_027 | Regen giữ kỳ đã thu | BF-M11-02 | Loan có 3 kỳ (1 đã thu) → regen | Kỳ đã thu giữ; kỳ chưa thu thay mới; remaining=total−paid | Blocked (harness) |
| TC_M11_028 | Set Settled thủ công mở gate B20 | BF-M11-03/06 | Dialog set Status=Settled (chưa thu đủ) | Loan Settled → gate B20 mở | **Obs OBS-M11-05** (req U-M11-3) |

## Boundary / Input

| TC | Tên | Steps | Expected | Status |
|---|---|---|---|---|
| TC_M11_029 | Amount=0 tạo lịch | Company, Amount=0, tick lịch | Cảnh báo, không sinh lịch | Pass (code) |
| TC_M11_030 | Term=0 | Company, Term=0, tick lịch | Cảnh báo, không sinh lịch | Pass (code) |
| TC_M11_031 | Rate=null (không lãi) | Company, Rate trống | interest=0, total=gốc | Pass (code) |
| TC_M11_032 | Tên/ghi chú Unicode/tiếng Việt | Nhập dấu + emoji | Lưu nguyên vẹn (text column) | Blocked (harness) |
| TC_M11_033 | Số tiền rất lớn | Amount=999,999,999,999 | numeric(15,2) chứa được | Blocked (harness) |

## Concurrency / DB

| TC | Tên | Steps | Expected | Status |
|---|---|---|---|---|
| TC_M11_034 | 2 create đồng thời cùng candidate | 2 dialog create song song | **Mong đợi:** 1 loan/candidate. **Thực tế:** có thể 2 (CandidateId non-unique) | **Obs OBS-M11-01** |
| TC_M11_035 | Xóa loan → repayments | Xóa loan Company có lịch | **Mong đợi:** repayments xóa kèm. **Thực tế:** orphan (no FK cascade) | **Obs OBS-M11-06** |
| TC_M11_036 | Thu nợ song song 2 kỳ | 2 accountant MarkPaid khác kỳ | Cả 2 Paid; tất toán khi đủ | Blocked (harness) |

## Enum / Entity contract (Automation khả thi ngay)

| TC | Tên | Assert | Status |
|---|---|---|---|
| TC_M11_037 | LoanStatus.Settled là gate B20 | `LoanStatus.Settled` tồn tại, là mốc tất toán | **Automated (unit)** |
| TC_M11_038 | LoanKind chỉ Bank/Company | `Enum.GetValues<LoanKind>()` = {Bank, Company} | **Automated (unit)** |
| TC_M11_039 | LoanRepaymentStatus có Paid | chứa Pending/Partial/Paid/Overdue | **Automated (unit)** |
| TC_M11_040 | Loan default Kind=Bank, Status=Borrowing | `new Loan()` → Kind=Bank, Status=Borrowing | **Automated (unit)** |
| TC_M11_041 | LoanRepayment default Status=Pending | `new LoanRepayment()` → Status=Pending | **Automated (unit)** |

## Codex regression — BUG_M11_01 / CR-M11-1/2/3

| TC | Rule | Automation | Result |
|---|---|---|---|
| TC_M11_042 | Bank Borrowing/Disbursed không gate B20; Company chưa Settled có gate | Unit `LoanCollectionRules` | Pass |
| TC_M11_043 | Bank không được set Settled | Unit `ValidateStatusChange` | Pass |
| TC_M11_044 | Non-finance không đổi trạng thái liên quan Settled | Unit + source authz review | Pass |
| TC_M11_045 | Company còn outstanding không được Settled | Unit `ValidateStatusChange` | Pass |
| TC_M11_046 | Thu một kỳ chỉ thu phần còn thiếu và chưa settle nếu còn kỳ | Unit `Collect` | Pass |
| TC_M11_047 | Thu hết mọi kỳ thu đúng tổng outstanding rồi auto-settle | Unit `Collect` | Pass |
| TC_M11_048 | Loan không lịch: Thu hết thu đủ Amount rồi auto-settle | Unit `Collect` | Pass |
| TC_M11_049 | Migration receipt có LoanId/LoanRepaymentId + unique kỳ thu | Unit migration operations | Pass |

> Runtime DB/UI cho receipt, role matrix và transaction vẫn chờ Claude chạy trên DB test/harness; Codex không đánh dấu Verified.
