# M11 — Loans & Debt Collection · Analysis

> QA phiên #4 (Claude 2026-07-11). Đọc source thật; không sửa business logic. Dep: **M05 Candidate (Verified)**, **M10 Finance (Verified)** — non-blocking.

## 1. Module Overview

- **Module ID:** M11
- **Module name:** Hỗ trợ vay & Thu nợ (Loans & Debt Collection)
- **Business purpose:** Quản lý hồ sơ hỗ trợ vay vốn của ứng viên XKLĐ. Hai loại: **Vay ngân hàng** (chỉ lưu thông tin, người vay tự trả ngân hàng) và **Nợ công ty** (VG cho đi dù không vay được → trừ dần vào lương theo lịch trả góp gốc+lãi). Kế toán "Thu nợ" từng kỳ; khi thu đủ → tự **tất toán**. Tất toán là điều kiện cổng (**gate B20**) để hoàn thành quy trình 20 bước.
- **Actor:** super_admin, accountant, recruitment_manager, recruiter, consultant (edit); accountant/super_admin (delete); director/document/visa/agent/parent/student (read); agent (read self-scope).
- **Dependencies:** M02 Authorization (permission `loans:*` + `AgentScope`), M05 Candidate (Loan→Candidate), M06/M07 (chi phí đơn hàng gợi ý số nợ; B20 workflow gate), M10 Finance (song song — vay ngân hàng đóng tiền bên Finance).
- **Entry point:** `/loans` (Hồ sơ vay), `/debt-collection` (Thu nợ), thẻ "Hỗ trợ vay vốn" trong `/candidates/{id}`.
- **Exit point:** Loan.Status = Settled (tất toán) → mở gate B20; hoặc xóa hồ sơ vay.

## 2. Source Code Map

| File | Loại | Symbol | Mục đích | Dependency |
|---|---|---|---|---|
| `src/Polymind.Web/Components/Pages/Loans/Loans.razor` | Page `/loans` | `OnInitializedAsync`, `Load`, `OpenCreate/OpenEdit`, `DeleteLoan`, `Filtered` | Danh sách + tạo/sửa/xóa hồ sơ vay; thẻ tổng quan; filter status/kind/search | DbFactory, AgentScope, AuthZ, BusinessRoleAccess |
| `src/Polymind.Web/Components/Pages/Loans/LoanDialog.razor` | Dialog | `SaveAsync`, `RegenerateScheduleAsync`, `SearchCandidates`, `GetOrderCostAsync`, `MaybePrefillCompanyAmount` | Tạo/sửa 1 hồ sơ vay; sinh lịch trả góp (nợ công ty) | DbFactory, AgentScope, AuthZ, BusinessRoleAccess |
| `src/Polymind.Web/Components/Pages/Loans/DebtCollection.razor` | Page `/debt-collection` | `OnInitializedAsync`, `Load`, `DebtCard`, `MarkPaid`, `ToggleExpand` | Thu nợ công ty theo kỳ; tiến độ; tự tất toán khi thu đủ | DbFactory, AgentScope, AuthZ, BusinessRoleAccess |
| `src/Polymind.Domain/Entities/Loan.cs` | Entity | — | Hồ sơ vay: Kind/Status/Amount/TermMonths/BankName/InterestRate/DisbursedDate/MonthlyDeductionAmount/DeductionStartDate/Note/CreatedBy | BaseEntity, enums |
| `src/Polymind.Domain/Entities/LoanRepayment.cs` | Entity | — | Kỳ trả góp: LoanId/InstallmentNo/DueDate/Amount/PaidAmount/PaidDate/Status/Note | BaseEntity, enum |
| `src/Polymind.Domain/Enums/Enums.cs` | Enum | `LoanKind`, `LoanStatus`, `LoanRepaymentStatus` | Bank/Company; NotBorrowed/Borrowing/Disbursed/Settled; Pending/Partial/Paid/Overdue | — |
| `src/Polymind.Web/Display/BusinessRoleAccess.cs` | Guard | `CanEditLoan`, `CanDeleteLoan` | Siết vai trò cho sửa/xóa (kèm permission) | RoleNames |
| `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor` | Page (cross-module) | `Load` (loan block dòng 1131-1143), `AdvanceStep` (B20 gate 1775-1789), `DeleteLoansAsync` (~1509) | Hiển thị loan; **gate B20 `_hasOpenLoan`**; xóa loan | DbFactory, AuthZ |
| `src/Polymind.Infrastructure/Persistence/DbSeeder.cs` | Seed | `RolePermissionMap` | Map role → `loans:*` | — |
| `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs` | DbContext | `Loan` config (dòng 100-104) | Index Code(unique), CandidateId(**non-unique**), Status | — |
| `src/Polymind.Infrastructure/Persistence/DemoDataSeeder.cs` | Seed demo | dòng 541-615 | Seed vài hồ sơ vay + nợ công ty có lịch | — |

**Không có REST endpoint cho loans** — toàn bộ qua Blazor Server component (giảm attack surface IDOR/API trực tiếp).

## 3. UI Inventory

- **`/loans`:** thẻ tổng quan (Đang vay/Nợ công ty/Đã tất toán/Tổng giải ngân) · search (tên/mã/ngân hàng) · filter LoanStatus · filter LoanKind · bảng desktop + card mobile · nút Thêm/Sửa/Xóa/Thu nợ · empty state · loading.
- **LoanDialog:** autocomplete ứng viên (create; chỉ ứng viên chưa có loan) hoặc tên khóa (edit) · select Kind/Status · Amount/Term/Bank/InterestRate/DisbursedDate · (Company) MonthlyDeduction/DeductionStart + checkbox "Tạo lịch trả góp" · Note · alert giải thích Bank không có lịch.
- **`/debt-collection`:** thẻ tổng quan (còn nợ/tổng phải thu/đã thu/đã tất toán) · nhóm "Đang còn nợ" + "Đã tất toán" · DebtCard: progress bar %, kỳ tới, nút "Thu kỳ tới", bảng kỳ chi tiết (collapse) với nút "Đã thu" từng kỳ · empty state.

## 4. API Inventory

Không có REST API. Mọi thao tác là Blazor Server event handler (server-bound):

| Thao tác | Handler | AuthZ | DB side effect | Audit |
|---|---|---|---|---|
| Tạo/sửa hồ sơ vay | `LoanDialog.SaveAsync` | `loans:create`/`loans:update` + `CanEditLoan` | insert/update `loans`; (Company+schedule) regen `loan_repayments`; (Bank) xóa repayments | `create`/`update` loans |
| Xóa hồ sơ vay | `Loans.DeleteLoan` / `CandidateDetail.DeleteLoansAsync` | `loans:delete` + `CanDeleteLoan` (+ AgentScope) | `RemoveRange(loans)` theo candidate | `delete` loans |
| Thu 1 kỳ | `DebtCollection.MarkPaid` | `_canUpdate` (`loans:update`+`CanEditLoan`) | update `loan_repayments` (Paid, PaidAmount, PaidDate); nếu đủ kỳ → `loans.Status = Settled` | `update` loans |
| Advance B20 | `CandidateDetail.AdvanceStep` (Completed) | workflow authz + `_hasOpenLoan == false` | workflow record + cjo Completed | `advance_step` |

## 5. Database Impact

- **`loans`:** PK Id; `Code` unique; `CandidateId` **non-unique** index (⚠ không ràng buộc 1-loan/candidate ở DB); `Status` index. FK CandidateId → candidates (logic, không cascade khai báo rõ). Audit field: CreatedBy, CreatedAt/UpdatedAt (BaseEntity). Không có rowversion/concurrency token.
- **`loan_repayments`:** PK Id; FK LoanId → loans (logic). InstallmentNo, DueDate, Amount, PaidAmount, PaidDate, Status. Không unique (LoanId, InstallmentNo). Không rowversion.
- **State field:** `Loan.Status` (LoanStatus), `LoanRepayment.Status` (LoanRepaymentStatus) — set tự do qua UI, không có state-machine cứng ở server (khác M09 commission).

## 6. Roles & Permissions

| Action | Role (permission) | UI Permission | API Permission | Business Condition | Source |
|---|---|---|---|---|---|
| Xem `/loans`, `/debt-collection` | director, RM, recruiter, consultant, document, visa, accountant, agent(self), parent(self), student(self), super_admin | `loans:read` | — | agent/parent/student self-scoped qua AgentScope | DbSeeder 40-111 |
| Tạo/sửa loan | super_admin, accountant, RM, recruiter, consultant | `loans:create`/`loans:update` **AND** `CanEditLoan` | — | mỗi candidate 1 loan (app-level) | DbSeeder + BusinessRoleAccess:30 |
| Xóa loan | super_admin, accountant | `loans:delete` **AND** `CanDeleteLoan` | — | AgentScope re-check ở Loans.DeleteLoan | BusinessRoleAccess:33 |
| Thu nợ (MarkPaid) | super_admin, accountant, RM, recruiter, consultant | `loans:update` + `CanEditLoan` (cached) | — | chỉ nợ công ty có lịch | DebtCollection:103,246 |

**Ghi chú quyền:** `AllActions("loans")` cho Accountant = create/read/update/delete. Director chỉ `loans:read`. Agent chỉ `loans:read` (self-scope) → **không** create/update/delete → mọi mutation loan miễn nhiễm IDOR qua vai trò agent-scoped (vai trò duy nhất bị scope lại là read-only).

## 7. Risk Analysis

| Rủi ro | Đánh giá ở source | Kết luận |
|---|---|---|
| **Broken authorization** | Page `[Authorize(loans:read)]`; mutation re-check permission+role; delete re-check | ✅ Đúng — mutation đều gate |
| **IDOR (candidate/loan khác scope)** | Chỉ agent bị scope, mà agent read-only. RM/accountant thấy toàn bộ (không scope). MarkPaid/Save không re-check scope nhưng caller vai trò không-scope | ✅ Không IDOR thực (vai trò scoped = read-only) |
| **API trực tiếp bỏ qua UI** | Không có REST endpoint loans | ✅ Không áp dụng |
| **Duplicate submit / 2 loan/candidate** | `CandidateId` index **non-unique**; dedup chỉ ở app (SearchCandidates lọc + latest-wins). Race 2 create đồng thời → 2 loan | ⚠ **OBS-M11-01** (Low, concurrency, no unique/rowversion) |
| **Invalid state transition** | Loan.Status set tự do (dropdown); không state-machine. Nhưng tất toán thật do MarkPaid tự set khi đủ kỳ | ⚠ **OBS-M11-05** (set Status=Settled thủ công qua dialog dù chưa thu đủ — mở gate B20 sớm) |
| **B20 gate bypass** | Gate trong Blazor AdvanceStep, không API; dùng latest loan | ✅ Đúng — trừ edge duplicate loan (OBS-M11-01) |
| **Lost update / concurrency MarkPaid** | Không guard status; nhưng op idempotent (set Paid = Paid); double-click vô hại | ✅ Chấp nhận (idempotent) |
| **Thu nợ over-permission** | `loans:update` cấp cho RM/recruiter/consultant → họ cũng thu được nợ (hành vi tài chính) | ⚠ **OBS-M11-02** (req: thu nợ có nên accountant-only?) |
| **Thu nợ không sinh Receipt/income** | MarkPaid chỉ update repayment + audit; không tạo Receipt như Finance | ⚠ **OBS-M11-03** (req: cần ghi nhận thu tiền mặt?) |
| **Partial payment** | Enum có `Partial` nhưng UI luôn set full `PaidAmount = Amount` | ⚠ **OBS-M11-04** (Low — không hỗ trợ trả một phần) |
| **Attribution actor** | Save/Delete/MarkPaid đều `GetRequiredUserIdAsync` (không first-user) | ✅ Đúng |
| **Interest/schedule sai số** | interest = gốc×%/100×tháng/12 (lãi đơn); chia đều + bù dư kỳ cuối; giữ kỳ đã thu | ✅ Hợp lý (xem BF-M11-02) |
| **Xóa loan khi đang thu** | Delete RemoveRange loans (không xóa repayments tường minh — FK cascade?) | ⚠ **OBS-M11-06** (repayment orphan nếu không cascade) |
| **Timezone** | DateOnly cho ngày; `DateTime.UtcNow` cho PaidDate | ✅ Nhất quán UTC |

## 8. Unknowns (Needs Requirement Clarification)

- **U-M11-1 (OBS-M11-02):** Chức năng "Thu nợ" (ghi nhận thu tiền trả góp) có nên **chỉ kế toán/super_admin**, hay đúng khi RM/recruiter/consultant (có `loans:update`) cũng thu được? Hiện tại: ai có `loans:update`+`CanEditLoan` đều thu được.
- **U-M11-2 (OBS-M11-03):** Thu nợ trả góp có cần sinh **Receipt/bút toán thu** (như khoản thu Finance) để đối soát dòng tiền không? Hiện tại: không sinh receipt, chỉ update kỳ + audit.
- **U-M11-3 (OBS-M11-05):** Cho phép set `Loan.Status = Settled` thủ công qua LoanDialog (dù chưa thu đủ kỳ) — có chủ đích (VD miễn nợ) hay nên chỉ Settled tự động khi thu đủ? Ảnh hưởng gate B20.
- **U-M11-4 (OBS-M11-06):** Xóa hồ sơ vay có nên xóa kèm `loan_repayments` (cascade) không? Cần xác nhận FK cascade để tránh orphan.
