# M12 — Visa & Flight / Exit · Business Flows

> Dựng từ source thật (`Visas.razor`, `VisaDialog.razor`, `FlightDialog.razor`, `NotificationService.cs`, `CsvExportEndpoints.cs`).

## BF-M12-01 — Tạo hồ sơ visa

- **Actor/Role:** super_admin, VisaStaff (`visas:create`).
- **Preconditions:** tồn tại CandidateJobOrder (ứng viên đã gắn đơn hàng).
- **Initial state:** không có Visa cho cặp candidate/job.
- **Input:** chọn CandidateJobOrder (→ CandidateId/JobOrderId/Country auto), VisaType, Status, SubmittedDate/InterviewDate/ResultDate, (Rejected) RejectionReason, Notes.
- **Main flow:** `/visa` tab Visa → Thêm → chọn CJO → nhập → Lưu → re-check `visas:create` → `new Visa { HandledBy = <first user> }` (⚠ nên là actor) → ApplyTo → insert → SaveChanges.
- **Error flow:** chưa chọn CJO → cảnh báo; thiếu quyền → cảnh báo.
- **Validation:** CandidateId/JobOrderId != empty; RejectionReason chỉ giữ khi Status=Rejected.
- **AuthZ:** `visas:create`.
- **DB changes:** insert `visas`.
- **Notification:** phụ thuộc `HandledBy` (xem BF-M12-05).
- **Audit:** **KHÔNG** (OBS-M12-01).
- **Final state:** Visa tồn tại, HandledBy = first-user (BUG_M12_01).
- **Risk:** BUG_M12_01 (HandledBy sai → notification sai người); OBS-M12-01 (no audit).

## BF-M12-02 — Sửa hồ sơ visa

- **Actor/Role:** super_admin, VisaStaff (`visas:update`).
- **Main flow:** tab Visa → edit → CJO khóa → sửa Status/ngày/notes → Lưu → re-check `visas:update` → ApplyTo → UpdatedAt → SaveChanges.
- **State:** Status set tự do (không state-machine — OBS-M12-02). RejectionReason=null nếu không Rejected.
- **HandledBy:** KHÔNG đổi khi edit (giữ giá trị create).
- **Audit:** KHÔNG.

## BF-M12-03 — Tạo vé máy bay

- **Actor/Role:** super_admin, VisaStaff (`flights:create`).
- **Input:** CandidateJobOrder, Airline, TicketCode, DepartureDate/Time, sân bay đi/đến, Notes.
- **Main flow:** tab Flight → Thêm → chọn CJO → nhập → Lưu → re-check `flights:create` → `new Flight { AssignedTo = <first user> }` (⚠ nên actor) → ApplyTo → insert.
- **AuthZ:** `flights:create`.
- **DB changes:** insert `flights` (ActualDepartureAt = null).
- **Audit:** KHÔNG.
- **Risk:** BUG_M12_02 (AssignedTo sai — cosmetic, không dùng cho notification).

## BF-M12-04 — Sửa vé máy bay

- **Actor/Role:** super_admin, VisaStaff (`flights:update`).
- **Main flow:** tab Flight → edit → CJO khóa → sửa → Lưu → re-check → ApplyTo → UpdatedAt → SaveChanges.
- **Lưu ý:** FlightDialog **không** cho set `ActualDepartureAt` → không xác nhận được xuất cảnh thực tế (OBS-M12-03).

## BF-M12-05 — Nhắc việc visa/xuất cảnh (cross M13)

- **Trigger:** NotificationJob quét (NotificationService).
- **Visa reminder (281-299):** visa chưa Approved/Rejected có InterviewDate/ResultDate trong [today, horizon] → sự kiện "Phỏng vấn visa"/"Có kết quả visa" → recipient = **`HandledBy`** nếu có, ngược lại `CandidateOwnersOr(VisaStaff, Director)`.
  - **BUG_M12_01 tác động:** HandledBy = first-user (super admin seed) → reminder gửi **sai người** thay vì VisaStaff thật.
- **Departure reminder (301-313):** flight `ActualDepartureAt == null` và DepartureDate trong [today, horizon] → "Sắp xuất cảnh" → recipient = `CandidateOwnersOr(VisaStaff, Director)` (**không** dùng AssignedTo).
  - Vì ActualDepartureAt không set runtime (OBS-M12-03), điều kiện chỉ tự tắt khi DepartureDate rời khỏi window.

## BF-M12-06 — Báo cáo xuất cảnh thực tế (cross M16)

- **CsvExportEndpoints 216/245:** thống kê flights có `ActualDepartureAt != null`.
- **OBS-M12-03 tác động:** không đường runtime set `ActualDepartureAt` → báo cáo actual-departure **luôn rỗng** với dữ liệu thật (chỉ demo seed có).

## State/transition tổng hợp

| Entity | Current | Action | Allowed Role | Next | DB | Notification | Audit |
|---|---|---|---|---|---|---|---|
| Visa | (none) | Create | VisaStaff/super | NotSubmitted..Rejected (tự do) | insert visas | phụ thuộc HandledBy | KHÔNG |
| Visa | any | Update status | VisaStaff/super | bất kỳ VisaStatus | update visas | — | KHÔNG |
| Visa | !Approved/!Rejected | reminder | (job) | — | — | HandledBy hoặc owners | — |
| Flight | (none) | Create | VisaStaff/super | — | insert flights | — | KHÔNG |
| Flight | ActualDepartureAt=null | reminder | (job) | — | — | owners(VisaStaff,Director) | — |
| Flight | — | Confirm exit | **(không có UI)** | ActualDepartureAt set | — | — | — |

Kiểm tra trạng thái không thể đi tới / bỏ qua: VisaStatus cho nhảy cóc (không state-machine) — OBS-M12-02. Không có xóa. Không có xác nhận xuất cảnh runtime — OBS-M12-03.
