# M07 — Candidate Workflow · Business Flows

## BF-M07-01 — Chuyển bước bình thường (Advance)
- **Role:** nhóm phụ trách bước hiện tại (hoặc super_admin).
- **Precondition:** CandidateJobOrder Active, CurrentStep < Completed, không ở 7.5.
- **Main:** nút "Chuyển bước" → `AdvanceStep` re-check `CanAdvance` → validate gate theo bước (exam mode / orientation / payment mode / overseas) → append `WorkflowStepRecord(Completed/Skipped)` actor → `CurrentStep = Next()` → `CommissionEngine.EnsureAsync` → audit `advance_step`.
- **Validation:** EntranceExam cần `_examMode`; Orientation cần ≥1 nội dung học hoặc Skip; FullPayment cần `_paymentMode` (+ hồ sơ vay nếu "Cam kết trả nợ"); OverseasSupport cần vay tất toán + confirm.
- **DB:** update CurrentStep, insert record; Status=Completed nếu next=Completed.
- **AuthZ:** `CanAdvance` server-side. **Risk:** stale-state race (R3).

## BF-M07-02 — Rớt B8 (FailEntranceExam) → lùi 7.5
- **Role:** Hồ sơ + Tuyển dụng (CanAdvance EntranceExam).
- **Main:** nút "Đánh rớt" (chỉ ở B8) → cần `_examMode` → confirm → append `WorkflowStepRecord(EntranceExam, Failed)` → `CurrentStep = ReselectJobOrder` → audit `fail_step`.
- **DB:** CurrentStep=7.5, giữ hồ sơ + lịch sử.
- **Risk:** không (chặn quyền + chỉ từ B8).

## BF-M07-03 — Chọn lại đơn hàng (ReassignJobOrder) 7.5 → B8
- **Role:** Tuyển dụng (CanAdvance ReselectJobOrder).
- **Main:** ở 7.5 → chọn đơn hàng khác → `ReassignJobOrder` re-check → **bắt job MỚI ≠ đơn cũ** + **còn hạn ứng tuyển** → append record(ReselectJobOrder, Completed) → `JobOrderId = newJob`, `CurrentStep = EntranceExam` → audit `reselect_job_order`.
- **DB:** đổi JobOrderId, quay lại B8; giữ hồ sơ + lịch sử.
- **Risk:** gắn lại đơn cũ / hết hạn (chặn).

## BF-M07-04 — Nhật ký giai đoạn xứ người (B19 OverseasSupport)
- **Role:** `_canLogOverseas` (update candidate hoặc CanAdvance OverseasSupport).
- **Main:** ở B19 → nhập ghi chú → `AddOverseasLog` → append `WorkflowStepRecord(OverseasSupport, InProgress)` actor. Nhiều record (nhật ký nhiều năm).
- **DB:** insert record InProgress (không đổi CurrentStep).

## BF-M07-05 — Hoàn thành (B20 Completed)
- **Role:** Tuyển dụng + Hồ sơ (CanAdvance OverseasSupport).
- **Main:** ở B19 "Chuyển bước" → **gate `_hasOpenLoan` (vay chưa tất toán → chặn)** → confirm "hết nghĩa vụ" → advance → `CurrentStep=Completed`, `Status=Completed`.
- **Risk:** hoàn thành khi còn nợ (chặn — R5).

## BF-M07-06 — RB-2 đổi đơn hàng reset workflow (super_admin)
- **Role:** super_admin only + ConfirmPassword (xem M05 BF-M05-08).
- **Main:** `ChangeJobOrderAsync` → gắn đơn mới + reset tiến trình 20 bước.
- **U1 — ĐÃ CHỐT (user 2026-07-10):** đổi đơn hàng reset workflow **KHÔNG** hoàn/hủy khoản thu + hoa hồng đã phát sinh → hành vi đúng, không bug. Verify chéo M09/M10.

### State transition matrix (rút gọn)
| Current | Action | Allowed Role | Next | DB | History |
|---|---|---|---|---|---|
| B1-B7 | Advance | nhóm bước | +1 (B7→B8) | CurrentStep | record Completed |
| B8 | Advance (đậu) | Hồ sơ/Tuyển dụng | B9 | CurrentStep | record Completed |
| B8 | Fail | Hồ sơ/Tuyển dụng | 7.5 | CurrentStep | record Failed |
| 7.5 | Reassign (job mới) | Tuyển dụng | B8 | JobOrderId+Step | record Completed |
| B10 | Advance/Skip | Hồ sơ | B11 | CurrentStep | record Completed/Skipped |
| B15 | Advance | Kế toán | B16 | CurrentStep | record Completed |
| B19 | AddLog | Tuyển dụng/Hồ sơ | B19 | — | record InProgress |
| B19 | Advance (nợ tất toán) | Tuyển dụng/Hồ sơ | B20 Completed | Status=Completed | record Completed |
