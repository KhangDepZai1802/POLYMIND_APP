# M10 — Finance · 06 Bug Report

> QA: Claude · 2026-07-10. Chỉ ghi bug có bằng chứng (code-level). Không sửa business logic.

## BUG_M10_01 — Đánh dấu khoản thu "Đã đóng" qua đường phụ bỏ qua ép tuần tự + KHÔNG phát sinh hoa hồng

- **Module:** M10 · **Severity:** Medium · **Priority:** Medium · **Confidence:** Medium (code-level; runtime cần integration DB)
- **Business Flow:** BF-M10-03 / BF-M10-04 · **Test Case:** TC_M10_009, TC_M10_010, TC_M10_011
- **Environment:** code · **Role:** accountant / super_admin
- **Preconditions:** ứng viên có lịch 4 bước; đại lý có cấu hình hoa hồng.
- **Steps to Reproduce:**
  1. Ở tab **Khoản thu**, bấm **Duyệt** trên khoản thu là **bước đóng tiền** (VD Settlement) — hoặc mở **PaymentDialog** đặt Status=Paid cho bước đó.
  2. Khoản thu → `Paid` nhưng:
     - `ApprovePayment` (`Finance.razor:499-525`) và `PaymentDialog.Save` **KHÔNG** kiểm tuần tự (khác `MarkStagePaid:667-679`);
     - **KHÔNG** gọi `CommissionEngine.EnsureAsync` (chỉ `MarkStagePaid:691` gọi).
  3. Nếu đây là bước cuối/sau cùng và không còn thao tác ở tab **Tiến độ** → **hoa hồng mốc tương ứng (VD Departure) không bao giờ phát sinh** → đại lý bị thiếu hoa hồng.
- **Expected:** mọi đường đưa **khoản thu theo bước** về `Paid` phải: (a) ép tuần tự 1→4; (b) kích hoạt CommissionEngine như `MarkStagePaid`.
- **Actual (code-level):** chỉ tab Tiến độ (`MarkStagePaid`) làm đúng; tab Khoản thu (`ApprovePayment`) và dialog edit thì không → **thiếu hoa hồng + phá thứ tự đóng**.
- **Evidence:**
  - `Finance.razor:660-697` `MarkStagePaid` — có siblings check tuần tự + `CommissionEngine.EnsureAsync`.
  - `Finance.razor:499-525` `ApprovePayment` — set Paid, **không** tuần tự, **không** EnsureAsync.
  - `PaymentDialog.razor:29-34` Status select có `Paid`; `:210-220` `ApplyTo` set Status trực tiếp.
  - Nút "Duyệt" hiện cho **mọi** payment chưa Paid ở tab Khoản thu (`Finance.razor:169-172`), gồm cả stage payment.
- **Suspected Source Area:** ba đường set `Payment.Status=Paid` phân tán, side-effect không đồng nhất.
- **Required Files for Codex:** `src/Polymind.Web/Components/Pages/Finance/Finance.razor`, `src/Polymind.Web/Components/Pages/Finance/PaymentDialog.razor`, `src/Polymind.Web/Commissions/CommissionEngine.cs`.
- **Đề xuất fix (Codex quyết định):** gom việc chuyển `Payment→Paid` vào **một** hàm dùng chung (kiểm tuần tự cho stage payment + gọi `CommissionEngine.EnsureAsync`); hoặc ở tab Khoản thu/dialog **chặn** đổi trực tiếp stage payment sang Paid (yêu cầu thao tác qua tab Tiến độ). Với thu **lẻ** (Stage=null) giữ đường duyệt hiện tại.
- **Dependencies / Regression Risk:** liên quan M09 (hoa hồng) + M16 (report doanh thu Paid). Regression: MarkStagePaid happy path, tạo lịch, tuần tự, receipt.
- **Status:** Verified Fixed (code-level) — Claude 2026-07-11 (`08-verification-report.md`); runtime 3-entry-point posting pending harness

## Observations (không phải bug chặn)

| ID | Severity | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|
| OBS-M10-01 | Info/Req | Khoản chi **không có luồng duyệt**: `Expense.ApprovedBy` không bao giờ set; RB-7 có notification `ExpenseApproval` nhưng thiếu UI duyệt chi. | Xác nhận có cần duyệt chi (set ApprovedBy) không → nếu có, thêm action + gate `expenses:approve`. | **Req U-M10-1** |
| OBS-M10-02 | Low | Code `PT-/EX-/RC-{yyyyMMdd}-{Random(1000,9999)}` + unique index → va cùng ngày (~birthday) → `DbUpdateException` chưa bắt → lỗi thô. | Dùng sequence/counter hoặc bắt lỗi + retry. | Theo dõi |
| OBS-M10-03 | Info | Endpoint `/receipts/{id}.pdf` gated `receipts:read` (chỉ accountant/director/super_admin) nhưng **không candidate-scope**. An toàn hiện tại (partner/self-scoped không có quyền); latent IDOR nếu sau này cấp `receipts:read` cho role scoped. | Nếu mở receipts:read cho scoped role → thêm kiểm scope theo CandidateId. | Theo dõi |
| OBS-M10-04 | Info | `_expenses`/`_receipts` nạp không phân trang tận gốc (client pager) — chấp nhận quy mô hiện tại. | Server-side paging khi dữ liệu lớn. | Theo dõi (perf → M21) |

## Cross-check U2 (RB-2 reset không hoàn tiền)

- **Xác nhận:** module Finance **không có** logic hoàn khoản thu (`PaymentStatus.Refunded` không được set từ UI nào). Đổi đơn hàng (RB-2 reset) → khoản thu đã `Paid` **giữ nguyên**, không hoàn. Khớp U2 (user chốt 2026-07-10). Không phát sinh bug.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M10_01 | Medium | TC_M10_009/010/011 | BF-M10-03/04 | 3 đường set Payment→Paid side-effect không đồng nhất (tuần tự + hoa hồng) | `Finance.razor`, `PaymentDialog.razor`, `CommissionEngine.cs` | MarkStagePaid happy path, tạo lịch, tuần tự, receipt idempotent | Verified Fixed (code) — Claude 2026-07-11 |

## CR-M10-3 — Tách "ứng viên đã nộp" khỏi "kế toán đã duyệt" + Kho lưu trữ (user 2026-07-13)

**Yêu cầu user:**
1. Tick ở tab *Tiến độ đóng tiền* **không được tự động duyệt** khoản thu bên tab *Khoản thu*.
2. Duyệt xong ở *Khoản thu* thì bên *Phiếu thu/chi* **phải có phiếu để in ngay**.
3. Ứng viên thu đủ 100% → có nút **Đưa vào kho lưu trữ**, biến mất khỏi *Tiến độ* + *Khoản thu* (2 tab này đồng bộ với nhau). *Phiếu thu/chi* có kho lưu trữ **riêng, không đồng bộ**.
4. Nút *Thêm khoản thu* chuyển xuống mục **Khoản thu lẻ** (khoản tạo tay luôn không có Stage → chắc chắn là thu lẻ).
5. Xóa khoản đặt cọc 20tr seed rời — trùng với bước 1 của lịch 4 bước.

**Đã làm:**
- `PaymentStatus.Submitted = 5` (nối cuối enum, không xê dịch giá trị int cũ). Tiến độ → `MarkSubmittedAsync` (Submitted); Khoản thu → `MarkPaidAsync` (Paid). Hoa hồng + phiếu thu **chỉ** phát sinh khi kế toán duyệt.
- Hai cổng thứ tự tách riêng: `UnpaidEarlierStages` (duyệt — đòi bước trước **Paid**) vs `UnsubmittedEarlierStages` (nộp — chỉ đòi bước trước **đã nộp**).
- `MarkPaidAsync` tự lập phiếu thu (idempotent) → duyệt xong có phiếu in ngay.
- `Payment.ArchivedAt/ArchivedBy` + `Receipt.ArchivedAt/ArchivedBy` (migration `AddFinanceArchive`). `FinanceArchiveService` + tab **Kho lưu trữ** (khôi phục được).
- **Chốt chặn chống miễn nợ:** `PaymentPostingRules.CanArchiveSchedule` — chỉ lưu trữ khi **4/4 bước Paid**; Submitted/Refunded không tính. Re-check ở server, không chỉ ẩn nút.
- Lưu trữ **KHÔNG** trừ khỏi `Tổng đã thu` (KPI tính trên toàn bộ payment kể cả đã lưu trữ) — lưu trữ là ẩn, không phải xóa.
- `DemoDataSeeder`: bỏ khoản 20tr rời + `RemoveDuplicateSeedDepositsAsync` dọn dữ liệu cũ (đã gỡ 11 bản ghi trên DB dev).

**Test:** `M10_FinanceRulesTests` +15 case (submit-gate vs approve-gate, thứ tự enum, 5 case kho lưu trữ). Suite 228/228 xanh.

**Status:** Fixed (code-level) — **chờ user xác minh trên UI**.
