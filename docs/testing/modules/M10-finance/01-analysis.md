# M10 — Finance (Payments & Expenses) · 01 Analysis

> QA: Claude · 2026-07-10 · Không sửa business logic. Cross-check **U2** (RB-2 reset không hoàn tiền) + liên kết **BUG_M09_01** (trigger hoa hồng khi mark payment Paid).

## 1. Module Overview

- **Module ID:** M10
- **Module name:** Finance — Khoản thu (Payment), Khoản chi (Expense), Phiếu thu/chi (Receipt), Lịch đóng tiền 4 bước 20/30/30/20.
- **Business purpose:** Quản lý nghĩa vụ đóng tiền của ứng viên theo **chi phí đơn hàng** chia 4 bước (Đặt cọc 20% → Phí dịch vụ 30% → Phí trước xuất cảnh 30% → Tất toán 20%), **đóng tuần tự 1→4**; ghi nhận khoản chi vận hành; sinh phiếu thu/chi + in PDF. Mark payment Paid **kích hoạt hoa hồng đại lý** (CommissionEngine).
- **Actor / Role:**
  - Ghi nhận/duyệt khoản thu (`payments:update`+`payments:approve`, **KHÔNG phải Director**): accountant, super_admin.
  - Tạo/sửa khoản thu (`payments:create/update`): accountant, super_admin.
  - Khoản chi (`expenses:*`): accountant, super_admin.
  - Phiếu thu/chi (`receipts:*`): accountant (all), director (read), super_admin.
  - Xem (`payments:read`): + director, parent, student (self-scoped tiến độ của mình).
- **Dependencies:** M05 (Candidate), M06 (JobOrder.CostAmount = tổng nghĩa vụ), M02 (RBAC), M09 (CommissionEngine — trigger từ `MarkStagePaid`), M16 (Reports đọc Payment/Expense Paid).
- **Entry point:** `/finance` (4 tab: Tiến độ / Khoản thu / Khoản chi / Phiếu thu-chi), `/receipts/{id}.pdf`.
- **Exit point:** Payment (Pending→Paid), Expense, Receipt + audit; commission side-effect.

## 2. Source Code Map

| # | File | Loại | Method | Mục đích |
|---|---|---|---|---|
| 1 | `Web/Components/Pages/Finance/Finance.razor` | Page `/finance` | `Load`, `LoadProgress`, `CreateSchedule`, `MarkStagePaid`, `ApprovePayment`, `CreateReceiptForPayment/Expense`, `CanRecordPayment`, `HasPermission` | Trang tài chính 4 tab; tiến độ đóng tiền + tạo lịch + đánh dấu đóng + duyệt thu + phiếu. |
| 2 | `Web/Components/Pages/Finance/PaymentDialog.razor` | Dialog | `Save`, `ApplyTo` | Thêm/sửa khoản thu; re-check `payments:create/update`; **Status select cho phép set Paid trực tiếp** (xem BUG_M10_01). |
| 3 | `Web/Components/Pages/Finance/ExpenseDialog.razor` | Dialog | `Save` | Thêm/sửa khoản chi; re-check `expenses:create/update`. Không có luồng duyệt. |
| 4 | `Web/Display/PaymentSchedule.cs` | Logic (Web) | `Split`, `AmountFor`, `Percent`, `Stages` | Chia tổng chi phí 20/30/30/20; **bước cuối nhận phần dư** để tổng khớp tuyệt đối. |
| 5 | `Web/Display/FinanceEligibility.cs` | Logic (Web) | `CandidateIdsAsync`, `CandidateJobOrderIdsAsync` | Ứng viên đủ điều kiện tài chính = đơn gần nhất `CurrentStep >= Deposit`. |
| 6 | `Web/Commissions/CommissionEngine.cs` | Logic | `EnsureAsync` | Sinh hoa hồng — gọi từ `MarkStagePaid:691` (M09). |
| 7 | `Web/Reporting/CsvExportEndpoints.cs:34-46` | Endpoint | `/receipts/{id}.pdf` | In phiếu PDF, gated `receipts:read`. |
| 8 | `Domain/Entities/Payment.cs`, `Expense.cs`, `Receipt.cs` | Entity | — | Payment(Stage?, Status, ApprovedBy...); Expense(ApprovedBy — chưa dùng); Receipt(PaymentId?/ExpenseId?). |
| 9 | `Infrastructure/.../ApplicationDbContext.cs:92-121` | DbConfig | — | Payment/Expense/Receipt Code **unique**; precision Amount. |
| 10 | `Domain/Enums/Enums.cs` | Enum | PaymentStage(1..4), PaymentStatus, PaymentType, ExpenseCategory, ReceiptType | Từ vựng. |

## 3. UI Inventory

- **Tab Tiến độ đóng tiền:** card mỗi ứng viên (đủ điều kiện) — progress bar %, 4 stage chip (Đã đóng / Đánh dấu đã đóng[nút, chỉ bước kế] / Chưa tới lượt[khóa] / Chưa tạo), nút "Tạo lịch 4 bước" (nếu chưa có), cảnh báo đơn chưa nhập chi phí. Search. Self-scoped: chỉ ứng viên của mình, không nút (thiếu quyền).
- **Tab Khoản thu:** DataGrid (Mã/Ứng viên/Loại/Số tiền/Trạng thái/Hạn) + nút **Duyệt** (khi chưa Paid), **Phiếu thu** (khi Paid, idempotent), **Sửa**. Nút "Thêm khoản thu" (`payments:create`).
- **Tab Khoản chi:** DataGrid (Mã/Loại/Mô tả/Số tiền/Ngày) + **Phiếu chi** + **Sửa**. Nút "Thêm khoản chi".
- **Tab Phiếu thu/chi:** (chỉ `receipts:read`) DataGrid + nút **In PDF** (`/receipts/{id}.pdf`).
- **KPI (ẩn với self-scoped):** Tổng đã thu / Còn phải thu / Tổng đã chi.
- **Dialogs:** PaymentDialog (ứng viên*, loại, số tiền*, trạng thái, phương thức, hạn/ngày thu, ghi chú); ExpenseDialog (loại, số tiền*, ngày chi*, mô tả).

## 4. API Inventory

| Thao tác | Gate UI | Re-check server | DB side effect | Notification |
|---|---|---|---|---|
| Tạo lịch 4 bước | nút (`payments:create`) | `CreateSchedule` re-check `payments:create` | insert 4 Payment(Pending) idempotent (bỏ bước đã có) + audit | — |
| Đánh dấu đã đóng (stage) | nút bước kế (`CanRecordPayment`) | `MarkStagePaid` re-check + **ép tuần tự 1→4** | Payment→Paid + audit + **CommissionEngine.EnsureAsync** | RB-7 hoa hồng (M13) |
| Duyệt khoản thu | nút (`CanRecordPayment`) | `ApprovePayment` re-check | Payment→Paid + audit — **KHÔNG trigger commission, KHÔNG ép tuần tự** (BUG_M10_01) | — |
| Tạo/sửa khoản thu | nút/dialog | `PaymentDialog.Save` re-check `payments:create/update` | insert/update Payment + audit; **Status có thể set Paid trực tiếp** (BUG_M10_01) | — |
| Khoản chi | nút/dialog | `ExpenseDialog.Save` re-check `expenses:*` | insert/update Expense + audit | — |
| Phiếu thu/chi | nút | `CreateReceiptFor*` re-check `receipts:create` + AnyAsync idempotent | insert Receipt + link + audit | — |
| In PDF phiếu | nút | endpoint `.RequireAuthorization("receipts:read")` | — (read) | — |

## 5. Database Impact

- **payments:** Code **unique**; Stage? (null = thu lẻ), Status, ApprovedBy, PaidDate, ReceiptId?. Không FK cứng candidate/job (Guid thô).
- **expenses:** Code **unique**; ApprovedBy (**không bao giờ set** — OBS-M10-01); ReceiptId?.
- **receipts:** Code **unique**; PaymentId?/ExpenseId? (nguồn); CandidateId?/AgentId? (đối tượng).
- **Audit:** create/update payment+expense, approve payment, create receipt. **Không audit** khi ApprovePayment? — có (`approve`). 
- **Concurrency:** không rowversion; Code unique là chốt trùng (random suffix → hiếm khi va → DbUpdateException chưa bắt, OBS-M10-02).

## 6. Roles & Permissions

| Action | Role | Nguồn |
|---|---|---|
| payments read | super_admin, director, accountant, parent, student (self) | DbSeeder |
| payments create/update/approve | super_admin, accountant (AllActions) | DbSeeder:85 |
| ghi nhận Paid (`CanRecordPayment`) | super_admin, accountant (**Director bị loại tường minh**) | Finance.razor:744-753 |
| expenses all | super_admin, accountant | DbSeeder:86 |
| receipts all | super_admin, accountant; director read | DbSeeder:87, Director Read |

> **Director:** xem tài chính + duyệt hoa hồng (M09) nhưng **KHÔNG ghi nhận khoản thu** (`CanRecordPayment` loại Director dù có quyền) → tách bạch phê duyệt tiền. Khớp quyết định user 2026-07-10 (accountant chi/thu).

## 7. Risk Analysis

| Rủi ro | Đánh giá | Kết luận |
|---|---|---|
| **Nhiều đường set Paid, chỉ 1 trigger hoa hồng** | `MarkStagePaid` (progress) trigger EnsureAsync + ép tuần tự; `ApprovePayment` (Khoản thu tab) + `PaymentDialog` set Paid **không** trigger, **không** ép tuần tự. Stage payment có thể Paid sai đường → **thiếu hoa hồng** (đặc biệt bước cuối) + phá thứ tự. | **BUG_M10_01 (Medium)**. |
| Đóng vượt thứ tự | `MarkStagePaid` ép 1→4; ApprovePayment/edit không. | Hở qua đường phụ → gộp BUG_M10_01. |
| Attribution sai (first-user) | actorId khắp nơi (`GetRequiredUserIdAsync`). | **Đúng** — không anti-pattern. |
| Broken authz / IDOR | Page `[Authorize("payments:read")]`; dialog + action re-check; self-scoped lọc `OwnedCandidateId`; PDF gated `receipts:read` (chỉ staff). | **Đóng** ở code. |
| IDOR PDF phiếu | endpoint gated `receipts:read` — chỉ accountant/director/super_admin (không partner/self-scoped). Không candidate-scope nhưng các role này thấy toàn bộ tài chính. | **An toàn hiện tại** (OBS-M10-03 latent). |
| RB-2 reset hoàn tiền (U2) | **Không có logic refund Payment** (Refunded status không được set từ UI). Đổi đơn không hoàn khoản thu. | **U2 xác nhận: không hoàn tiền.** |
| Khoản chi duyệt (RB-7) | Expense.ApprovedBy + notification ExpenseApproval tồn tại nhưng **không có UI duyệt chi**. | **OBS-M10-01** (req — RB-7 một phần). |
| Receipt trùng | `AnyAsync(PaymentId/ExpenseId)` idempotent trước tạo. | **Đóng** (race hiếm). |
| Code trùng (random suffix) | Code unique index → va → DbUpdateException chưa bắt. | **OBS-M10-02** (Low). |
| Split 20/30/30/20 lệch tổng | bước cuối = total − running → tổng khớp tuyệt đối. | **Đúng**. |

## 8. Unknowns / Needs Requirement Clarification

- **U-M10-1 (OBS-M10-01):** Khoản chi có cần luồng **duyệt** (set `ApprovedBy`) không? Hiện chỉ tạo/sửa; RB-7 có notification "khoản chi chờ duyệt" nhưng thiếu UI duyệt. Non-blocking.
- **U-M10-2 (BUG_M10_01):** Có chủ đích cho phép set stage payment Paid qua tab "Khoản thu"/dialog (bỏ qua tuần tự + hoa hồng) không, hay chỉ được qua tab Tiến độ? Ảnh hưởng cách fix.
