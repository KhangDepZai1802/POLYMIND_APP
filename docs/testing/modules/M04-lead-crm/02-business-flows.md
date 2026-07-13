# M04 — Lead CRM · Business Flows

## BF-M04-01 — Tạo Lead
- **Actor/Role:** super_admin/RM/recruiter/consultant (`leads:create`). **Main flow:** `/leads` → Thêm Lead → LeadDialog nhập (FullName bắt buộc) → lưu → Code auto `LD-YYYYMMDD-XXXX`, Status=New → reload.
- **API tương đương:** `POST /api/leads` (FullName bắt buộc → 400 nếu thiếu) → 201 + audit create.
- **DB:** `leads`. **Audit:** create. **Risk:** —.

## BF-M04-02 — Đổi trạng thái Lead (10 trạng thái)
- **Main flow:** LeadDetail → chọn trạng thái mới (KHÔNG cho chọn "Đã chuyển"/Converted thủ công) + ghi chú → `UpdateStatus` → `leads:update`+CanEditLead → lưu Status + thêm LeadActivity(StatusChange, Old/New) → reload.
- **Alternate:** nếu lead đã convert (`IsConverted`) → nút đổi thành "chuyển ngược về Lead" → `RevertToLead`.
- **State machine:** New→NotContacted→Contacted→Interested→Appointment→Consulting→Registered→Converted; nhánh Unsuitable/Cancelled bất kỳ lúc nào. UI cho chọn tự do mọi trạng thái (trừ Converted) → không ép thứ tự (linh hoạt CRM).
- **DB:** `leads.status`, `lead_activities`. **Audit:** qua LeadActivity (không AddAudit riêng cho status).

## BF-M04-03 — Phân công tư vấn viên
- **Main flow:** chọn TVV (role consultant, active) → `AssignLead` → chặn nếu IsConverted → lưu `AssignedTo` + LeadActivity(Note) + audit assign.
- **Kiểm:** lead đã convert → khóa (opacity + snackbar). **Source:** LeadDetail:378-412.

## BF-M04-04 — Lịch hẹn tư vấn
- **Main flow:** chọn ngày/giờ (MinDate=Today) → `SaveAppointment` → chặn nếu IsConverted → validate không quá khứ → lưu `AppointmentAt` (UTC) + LeadActivity(Appointment).
- **Kiểm:** quá khứ → chặn; lead-care của trạng thái Appointment tính từ giờ hẹn.

## BF-M04-05 — Chuyển Lead → Ứng viên (Convert)
- **Preconditions:** `leads:update` + `candidates:create`, lead chưa có Candidate.
- **Main flow:** `Convert` → xác nhận → kiểm `existingId` (chống trùng) → tạo `Candidate` copy field từ Lead (AgentId, CollaboratorId, ConsultantId=AssignedTo) → Lead.Status=Converted + LeadActivity → điều hướng `/candidates/{id}`.
- **LỖI:** `CreatedBy = db.Users.First()` (user đầu tiên, KHÔNG phải actor) → **BUG_M04_01**.
- **Alternate:** lead đã có ứng viên → snackbar + điều hướng tới ứng viên đó (không tạo trùng).
- **Risk:** race 2 request đồng thời (không unique constraint LeadId).

## BF-M04-06 — Chuyển ngược Ứng viên → Lead (Revert)
- **Preconditions:** lead đã convert (có candidate).
- **Main flow:** `RevertToLead` → xác nhận → kiểm ứng viên CHƯA phát sinh dữ liệu (documents/payments/CJO/visa/flight/commission) → nếu có → chặn ("xử lý ở trang Ứng viên trước") → nếu không → xóa Candidate + Lead.Status=mới + LeadActivity + audit revert_to_lead.
- **An toàn:** không mất dữ liệu ngoài ý (guard đầy đủ). **Source:** LeadDetail:503-565.

## BF-M04-07 — Xóa Lead
- **Preconditions:** `leads:delete` + CanDeleteLead (super_admin/RM/doc_staff).
- **Main flow:** `DeleteLead` → xác nhận → set Candidate.LeadId=null (giữ hồ sơ ứng viên) + xóa LeadActivities + xóa Notifications(ref=lead) + audit delete + xóa Lead.
- **An toàn:** dọn liên kết đầy đủ (khác BUG_M03_01 — ở đây dọn đúng).

## BF-M04-08 — Nhắc chăm sóc Lead quá hạn (LeadCareRules)
- **Rule:** ThresholdHours theo trạng thái (New/NotContacted 24h; Contacted/Interested 48h; Appointment 24h TỪ GIỜ HẸN; Consulting/Registered 72h; kết thúc = không nhắc).
- **Job:** `NotificationService` (mỗi 5' qua Hangfire) → với lead chưa convert quá hạn → nhắc TVV phụ trách + RM + super_admin; UPDATE hàng notification cũ (tránh vỡ khóa unique 23505), bỏ qua nếu chưa đọc hoặc mới nhắc <24h.
- **UI:** chuông đỏ ở list + banner ở detail. **Kiểm sâu:** M13.

## Bảng trạng thái Lead

| Current | Action | Allowed Role | Condition | Next | DB | LeadActivity | Notification |
|---|---|---|---|---|---|---|---|
| New…Registered | Đổi trạng thái | leads:update + CanEditLead | không phải Converted | trạng thái chọn | leads.status | StatusChange | — |
| bất kỳ (chưa convert) | Convert | leads:update + candidates:create | chưa có candidate | Converted | tạo candidate | StatusChange | — |
| Converted (có candidate) | Revert | leads:update | candidate chưa có dữ liệu | trạng thái chọn | xóa candidate | StatusChange | — |
| bất kỳ | Xóa | leads:delete + CanDeleteLead | — | (deleted) | leads xóa, candidate.lead_id=null | (xóa) | xóa ref=lead |
| overdue | Job nhắc | hệ thống | quá ThresholdHours | (giữ) | notifications UPDATE | — | ReminderLeadCare |

**Kiểm tra điển hình:** đổi trạng thái tự do (không ép thứ tự — linh hoạt, không bug); convert chống trùng (còn race nhỏ); revert guard dữ liệu; xóa dọn liên kết đủ; lead-care tránh trùng khóa.
