# M11 — Loans & Debt Collection · Bug Report

> **Cập nhật Claude 2026-07-11 (phiên #6):** BUG_M11_01 và CR-M11-1/2/3 → **VERIFIED FIXED** (code + runtime migration `polymind_m11_verify` + DB unique-index PoC). Chi tiết ở `08-verification-report.md`.
> **Codex 2026-07-11:** BUG_M11_01 và CR-M11-1/2/3 đã Fixed — chờ Claude xác minh độc lập.
> Nhờ user giải thích luồng nợ công ty, phát hiện **BUG_M11_01**: cổng B20 chặn CẢ vay ngân hàng.
> Đồng thời 3 observation được user chốt thành **change request** cho Codex (thu nợ finance-only, sinh phiếu thu, phân quyền/miễn nợ tất toán).

---

## BUG_M11_01 — Cổng B20 "Hoàn thành quy trình" chặn cả VAY NGÂN HÀNG (không chỉ nợ công ty)

- **Bug ID:** BUG_M11_01
- **Module ID:** M11 (gate ở M07 workflow, dữ liệu M11)
- **Title:** `CandidateDetail` tính `_hasOpenLoan` từ khoản vay **mới nhất bất kể `Kind`** → ứng viên **vay ngân hàng** chưa "Đã tất toán" bị **chặn hoàn thành quy trình 20 bước** (B20). Theo nghiệp vụ user chốt: vay ngân hàng là việc ứng viên ↔ ngân hàng, **không** phải nghĩa vụ với công ty → **không được** gate B20.
- **Severity:** **Medium** (chặn sai completion; buộc nhân viên đánh dấu sai bank loan = Settled để lách → sai dữ liệu công nợ).
- **Priority:** P2
- **Business Flow ID:** BF-M11-06
- **Test Case ID:** TC_M11_024/025/026 (bổ sung case Bank loan)
- **Environment:** mọi môi trường
- **Preconditions:** ứng viên có khoản **vay ngân hàng** (Kind=Bank) status Borrowing/Disbursed; workflow ở bước OverseasSupport (B19→B20).
- **Steps to Reproduce:**
  1. Tạo ứng viên có Bank loan Borrowing.
  2. Đưa workflow tới bước OverseasSupport → bấm hoàn thành (B20).
- **Expected Result (user-confirmed 2026-07-11):** Bank loan **không bao giờ** chặn B20; chỉ **nợ công ty (Kind=Company)** chưa tất toán mới chặn. Ngoài ra **ẩn trạng thái "Đã tất toán" khỏi dropdown khi Kind=Bank** (công ty không theo dõi việc ứng viên tất toán với ngân hàng).
- **Actual Result:** mọi loan (kể cả Bank) chưa Settled đều chặn → "Khoản vay chưa tất toán — chưa thể hoàn thành quy trình". LoanDialog hiện cho cả Bank chọn "Đã tất toán".
- **Suspected Source Area:**
  - Gate: `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor:1133-1143` — query loan **không lọc `Kind`**; `_hasOpenLoan = loan is not null && loan.Status != LoanStatus.Settled` (dòng 1143). Gate tại `AdvanceStep` (OverseasSupport, dòng 1777).
  - UI status: `src/Polymind.Web/Components/Pages/Loans/LoanDialog.razor:116` — `LoanStatusOptions = { Borrowing, Disbursed, Settled }` dùng chung cho cả Bank/Company.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor` (loan block + B20 gate)
  - `src/Polymind.Web/Components/Pages/Loans/LoanDialog.razor` (status options theo Kind)
- **Dependencies:** M07 workflow B20; M11 loan.
- **Regression Risk:** Thấp — không đổi schema. Fix 2 phần độc lập.
- **Confidence Level:** Cao (source rõ + user xác nhận rule).
- **Status:** **Verified Fixed** (Claude phiên #6 — code + runtime migration + DB PoC)
- **Gợi ý hướng sửa (2 phần):**
  1. **Gate:** `_hasOpenLoan` chỉ xét **latest company loan** (`l.Kind == LoanKind.Company`) chưa Settled. Bank loan **không** ảnh hưởng B20.
  2. **UI:** khi `Kind == Bank`, dropdown trạng thái chỉ hiện **Đang vay / Đã giải ngân** (ẩn "Đã tất toán"); Company giữ đủ 3 trạng thái. (Có thể lọc `LoanStatusOptions` theo `_kind`.)

---

## User-confirmed change requests (2026-07-11 — chuyển observation → yêu cầu sửa cho Codex)

| ID | Quyết định user | Hiện trạng (defect/gap) | Hướng cho Codex |
|---|---|---|---|
| **CR-M11-1** (U-M11-1) | Thu nợ **chỉ kế toán/super_admin** | `DebtCollection.MarkPaid` cho mọi role có `loans:update` (gồm RM/recruiter/consultant) | **Fixed — chờ Claude:** `CanCollectDebt` + re-check `loans:update`/`receipts:create` ở mọi mutation |
| **CR-M11-2** (U-M11-2) | Thu nợ **sinh phiếu thu** để đối soát sổ | `MarkPaid` chỉ update kỳ + audit; không tạo `Receipt` → không lên báo cáo doanh thu | **Fixed — chờ Claude:** Income Receipt trong cùng transaction, gắn Candidate/Loan/(Repayment nếu thu kỳ) |
| **CR-M11-3** (U-M11-3) | **Tất toán CHỈ khi thu đủ 100% tiền thật — KHÔNG BAO GIỜ miễn nợ**; chỉ finance | Set Settled tự do, ai có `loans:update` cũng được → có thể "tất toán" khi còn nợ (= miễn nợ ngầm, CẤM) | **Fixed — chờ Claude:** chặn Settled khi còn dư; thêm Thu hết; auto-settle sau thu đủ; không có write-off |

> **🚫 QUY TẮC CỨNG (user nhấn mạnh 2026-07-11):** **KHÔNG BAO GIỜ miễn nợ** — "đây là kinh doanh, không phải làm từ thiện". Nợ công ty chỉ tất toán khi **thu đủ 100%**. Không có nút/luồng write-off; không cho đánh dấu Settled khi còn dư nợ.
> **Lưu ý:** CR-M11-1/2/3 là **thay đổi nghiệp vụ**, Codex triển khai theo hướng trên. BUG_M11_01 là defect logic rõ ràng, ưu tiên sửa.

---

## Observations (theo dõi — không handoff Codex trừ khi user chốt là bug)

### OBS-M11-01 — `loans.candidate_id` non-unique → race có thể tạo 2 hồ sơ vay/ứng viên (Low, concurrency)

- **Bằng chứng:** `ApplicationDbContext` `Loan` config chỉ `HasIndex(CandidateId)` (không `IsUnique`). Dedup "1 loan/candidate" chỉ ở app: `SearchCandidates` lọc ứng viên đã có loan + dialog load latest. Hai create đồng thời cùng candidate → 2 loan.
- **Tác động:** Loans list dung nạp (GroupBy→latest-wins); gate B20 dùng latest loan → nếu latest=Settled còn older chưa Settled thì gate mở dù còn nợ. Cần race để xảy ra.
- **Severity:** Low. Cùng lớp OBS-M07-01/OBS-M08-01 (no rowversion/unique). **Không** gây double-pay tiền như BUG_M09_01.
- **Đề xuất (nếu user coi là bug):** unique index `loans(candidate_id)` (cần migration + đối soát dữ liệu trùng như M09).

### OBS-M11-02 — "Thu nợ" cho phép RM/recruiter/consultant, không chỉ kế toán (req)

- **Bằng chứng:** `DebtCollection.MarkPaid` gate `_canUpdate = loans:update + CanEditLoan`. Seed cấp `loans:update` cho RM/recruiter/consultant → họ ghi nhận thu tiền trả góp (hành vi tài chính).
- **Req U-M11-1:** thu nợ có nên **accountant/super_admin-only**? Nếu có → tách permission (vd `loans:collect`) hoặc siết `CanCollectDebt`.
- **Severity:** Low (segregation-of-duties). Chờ user chốt.

### OBS-M11-03 — Thu nợ không sinh Receipt/bút toán thu (req)

- **Bằng chứng:** `MarkPaid` chỉ update `loan_repayments` + audit; không tạo `Receipt` (khác khoản thu Finance có phiếu thu).
- **Req U-M11-2:** dòng tiền thu nợ trả góp có cần ghi nhận vào sổ thu (Receipt/Reports doanh thu) để đối soát? Hiện tại không phản ánh ở báo cáo Finance.
- **Severity:** Low/req.

### OBS-M11-04 — Không hỗ trợ trả một phần dù enum có `Partial` (Low)

- **Bằng chứng:** `MarkPaid` luôn `PaidAmount = inst.Amount` (full), Status=Paid; `LoanRepaymentStatus.Partial` không bao giờ được set từ UI.
- **Severity:** Low (thiếu tính năng, không sai dữ liệu).

### OBS-M11-05 — Set `Loan.Status = Settled` thủ công qua dialog mở gate B20 dù chưa thu đủ (req)

- **Bằng chứng:** `LoanDialog` cho chọn Status tự do (dropdown, không state-machine). Đặt Settled → `_hasOpenLoan=false` → gate B20 mở dù `loan_repayments` chưa Paid hết.
- **Req U-M11-3:** cho phép tất toán thủ công (miễn nợ) có chủ đích? Nếu không → chỉ cho Settled tự động khi thu đủ kỳ. Kết hợp OBS-M11-02: non-finance role có thể "miễn nợ" để qua gate.
- **Severity:** Low/req (business policy + segregation-of-duties).

### OBS-M11-06 — Xóa hồ sơ vay để lại `loan_repayments` orphan (Low, data hygiene)

- **Bằng chứng:** `loan_repayments` **không có FK** tới `loans` (migration `20260706081025` chỉ PK; DbContext `LoanRepayment` không `HasOne`). `Loans.DeleteLoan`/`CandidateDetail.DeleteLoansAsync` `RemoveRange(loans)` không xóa repayments → orphan rows.
- **Tác động:** DebtCollection.Load lọc theo loanIds hiện có → orphan không hiển thị; không sai kết quả nhìn thấy. Chỉ là rác dữ liệu + mất referential integrity.
- **Severity:** Low. **Đề xuất:** thêm FK cascade hoặc xóa repayments tường minh khi xóa loan.

---

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M11_01 | Medium | TC_M11_024..026 | BF-M11-06 | B20 gate `_hasOpenLoan` không lọc `Kind` → chặn cả Bank | CandidateDetail.razor (1133-1143, 1777) | gate Bank không chặn / Company chặn | **Verified Fixed** |
| 2 | CR-M11-1 | Change | — | BF-M11-04 | Thu nợ + đổi Settled = chỉ finance | DebtCollection.razor, LoanDialog.razor | quyền recruiter/consultant bị chặn thu nợ | **Verified Fixed** |
| 3 | CR-M11-2 | Change | — | BF-M11-04 | Thu nợ sinh Receipt income | DebtCollection.razor | thu 1 kỳ → 1 receipt | **Verified Fixed** |
| 4 | CR-M11-3 | Change | — | BF-M11-03/04 | Tất toán chỉ khi thu đủ 100% (thu-hết + phiếu thu); CHẶN Settled thủ công khi còn nợ; **KHÔNG miễn nợ** | LoanDialog.razor, DebtCollection.razor | chặn Settled khi outstanding>0; thu-hết sinh receipt | **Verified Fixed** |

> **Claude verify 2026-07-11 (phiên #6):** 4/4 **Verified Fixed** — suite 88/88, Web 0/0, migration `20260711123000` áp sạch trên DB test, unique index `ix_receipts_loan_repayment_id` chặn thu trùng (DB PoC). Chi tiết `08-verification-report.md`. OBS-M11-01/04/06 + R-M11-A/B/C vẫn là backlog ngoài phạm vi.
