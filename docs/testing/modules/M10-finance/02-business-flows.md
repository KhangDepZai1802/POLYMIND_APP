# M10 — Finance · 02 Business Flows

> QA: Claude · 2026-07-10.

## BF-M10-01 — Tạo lịch đóng tiền 4 bước

- **Actor:** accountant/super_admin (`payments:create`).
- **Preconditions:** ứng viên đơn gần nhất `CurrentStep >= Deposit`; JobOrder.CostAmount > 0; chưa có (đủ) lịch.
- **Main flow:** tab Tiến độ → "Tạo lịch 4 bước" → `CreateSchedule`: re-check → `PaymentSchedule.Split(total)` (20/30/30/20, bước cuối bù dư) → tạo Payment(Pending) cho từng stage **còn thiếu** (idempotent) + audit → reload.
- **Alternate/Error:** total ≤ 0 → cảnh báo; đã đủ 4 bước → "đã có đủ lịch".
- **DB:** insert ≤4 Payment. **Notification:** none.

## BF-M10-02 — Đánh dấu đã đóng 1 bước (tuần tự) + hoa hồng

- **Actor:** accountant/super_admin (`CanRecordPayment`: update+approve, không Director).
- **Preconditions:** bước là bước chưa đóng đầu tiên (nút chỉ hiện ở `nextStage`).
- **Main flow:** nút "Đánh dấu đã đóng" → `MarkStagePaid`: re-check → **kiểm tuần tự** (mọi stage < hiện tại phải Paid, else cảnh báo) → Status=Paid + ApprovedBy=actor + PaidDate + audit → SaveChanges → **CommissionEngine.EnsureAsync** → nếu có lát mới SaveChanges + snackbar.
- **Alternate/Error:** thiếu quyền → cảnh báo; đóng vượt thứ tự → chặn "đóng bước trước 1→4".
- **DB:** Payment→Paid + AgentCommission(Pending) side-effect. **Notification:** RB-7 khi hoa hồng Approved (M13).
- **Risk:** race 2 mark đồng thời (thứ tự) low; **liên kết BUG_M09_01** (2 EnsureAsync đồng thời → hoa hồng trùng).

## BF-M10-03 — Duyệt khoản thu (tab Khoản thu) ⚠

- **Actor:** accountant/super_admin (`CanRecordPayment`).
- **Main flow:** tab Khoản thu → "Duyệt" (mọi payment chưa Paid/Refunded, **kể cả stage**) → `ApprovePayment`: re-check → Status=Paid + ApprovedBy + PaidDate + audit.
- **⚠ Gap:** **KHÔNG** ép tuần tự, **KHÔNG** gọi CommissionEngine → nếu là stage payment → bỏ qua thứ tự + **thiếu hoa hồng** (BUG_M10_01). Hoa hồng có thể "đuổi kịp" ở lần MarkStagePaid sau (EnsureAsync quét mọi stage Paid), nhưng nếu bước cuối duyệt đường này và không còn thao tác tab Tiến độ → hoa hồng Departure **không phát sinh**.

## BF-M10-04 — Thêm/sửa khoản thu (dialog) ⚠

- **Actor:** accountant/super_admin.
- **Main flow:** dialog → chọn ứng viên đủ điều kiện + số tiền>0 + trạng thái → Save re-check `payments:create/update` → insert/update + audit.
- **⚠ Gap:** Status select cho set **Paid** trực tiếp → cùng vấn đề BUG_M10_01 (không trigger hoa hồng, không tuần tự).

## BF-M10-05 — Khoản chi

- **Actor:** accountant/super_admin (`expenses:*`).
- **Main flow:** dialog → loại + số tiền>0 + ngày → Save re-check → insert/update + audit.
- **Gap:** không luồng duyệt (`ApprovedBy` không set) — OBS-M10-01.

## BF-M10-06 — Phiếu thu/chi + PDF

- **Actor:** accountant/super_admin (`receipts:create`); director/accountant/super_admin đọc + in.
- **Main flow:** nút "Phiếu thu/chi" → `CreateReceiptFor*`: re-check + AnyAsync idempotent → insert Receipt + link + audit. In PDF: `/receipts/{id}.pdf` gated `receipts:read`.
- **Idempotent:** đã có phiếu → thông báo, không tạo trùng.

## BF-M10-07 — Self-scoped xem tiến độ của mình

- **Actor:** parent/student (`payments:read`, IsSelfScoped).
- **Main flow:** `/finance` → chỉ tab Tiến độ, chỉ ứng viên `OwnedCandidateId`; KPI + tab thu/chi/phiếu ẩn; không nút ghi nhận.
- **Error/IDOR:** không thấy ứng viên khác (lọc cứng OwnedCandidateId 2 nơi: Load + LoadProgress).

### State machine khoản thu

| Current | Action | Allowed | Condition | Next | Trigger hoa hồng? | History |
|---|---|---|---|---|---|---|
| Pending | MarkStagePaid | accountant/SA | bước kế + tuần tự | Paid | **CÓ** (EnsureAsync) | audit approve |
| Pending | ApprovePayment | accountant/SA | — | Paid | **KHÔNG** ⚠ | audit approve |
| Pending | Edit Status=Paid | accountant/SA | — | Paid | **KHÔNG** ⚠ | audit update |
| Paid | (không refund UI) | — | — | — | — | — |

### Checklist nghiệp vụ

| Điểm kiểm | Kết quả |
|---|---|
| Đóng tuần tự 1→4 | Chỉ MarkStagePaid ép; đường phụ hở (BUG_M10_01) |
| Trigger hoa hồng khi Paid | Chỉ MarkStagePaid (BUG_M10_01) |
| Attribution | actor thật |
| RB-2 reset hoàn tiền (U2) | Không refund → xác nhận không hoàn |
| IDOR self-scoped/PDF | Đóng (lọc + gated) |
| Receipt idempotent | AnyAsync |
| Split tổng khớp | bù dư bước cuối |
