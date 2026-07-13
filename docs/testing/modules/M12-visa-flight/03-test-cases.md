# M12 — Visa & Flight / Exit · Test Cases

> Quy ước `TC_M12_<NN>`. Expected bám hành vi ĐÚNG. Runtime DB/UI **Blocked (harness)**. Status: Pass (code) / Blocked (harness) / Obs / **Fail (bug)**.

## Functional — Visa

| TC | Tên | Flow | Role | Steps | Expected | Status |
|---|---|---|---|---|---|---|
| TC_M12_001 | Tạo visa | BF-M12-01 | VisaStaff | /visa → Thêm visa → chọn CJO, VisaType, Status, Lưu | Visa tạo, CandidateId/JobOrderId/Country từ CJO | Blocked (harness) |
| TC_M12_002 | HandledBy = actor tạo | BF-M12-01 | VisaStaff (không phải user seed đầu) | Tạo visa → kiểm `visas.handled_by` | **Mong đợi:** = id VisaStaff. **Thực tế:** = user đầu DB | **Fail — BUG_M12_01** |
| TC_M12_003 | Sửa visa giữ CJO | BF-M12-02 | VisaStaff | Edit → CJO khóa | CJO không đổi được; các field khác lưu | Blocked (harness) |
| TC_M12_004 | RejectionReason chỉ khi Rejected | BF-M12-02 | VisaStaff | Status=Rejected → nhập lý do; đổi sang Approved | RejectionReason=null khi !Rejected (ApplyTo) | Pass (code) |
| TC_M12_005 | Tạo thiếu CJO | BF-M12-01 | VisaStaff | Không chọn CJO → Lưu | Cảnh báo "Vui lòng chọn ứng viên – đơn hàng" | Pass (code) |
| TC_M12_006 | Visa không audit | BF-M12-01/02 | VisaStaff | Tạo/sửa visa → kiểm audit_logs | **Không** có bản ghi audit | **Obs OBS-M12-01** |

## Functional — Flight

| TC | Tên | Flow | Role | Steps | Expected | Status |
|---|---|---|---|---|---|---|
| TC_M12_007 | Tạo vé | BF-M12-03 | VisaStaff | Thêm vé → CJO, Airline, TicketCode, ngày/giờ | Flight tạo | Blocked (harness) |
| TC_M12_008 | AssignedTo = actor tạo | BF-M12-03 | VisaStaff | Tạo vé → kiểm `flights.assigned_to` | **Mong đợi:** = id actor. **Thực tế:** = user đầu DB | **Fail — BUG_M12_02** |
| TC_M12_009 | Sửa vé | BF-M12-04 | VisaStaff | Edit vé | Field lưu, UpdatedAt đổi | Blocked (harness) |
| TC_M12_010 | Không set được xuất cảnh thực tế | BF-M12-04/06 | VisaStaff | Mở FlightDialog | Không có input `ActualDepartureAt` → không xác nhận được | **Obs OBS-M12-03** |
| TC_M12_011 | Flight không audit | BF-M12-03/04 | VisaStaff | Tạo/sửa vé → audit_logs | Không có audit | **Obs OBS-M12-01** |

## Authorization

| TC | Tên | Role | Steps | Expected | Status |
|---|---|---|---|---|---|
| TC_M12_012 | Chưa đăng nhập | anon | GET /visa | Redirect login (`[Authorize(visas:read)]`) | Pass (code) |
| TC_M12_013 | Director read-only | director | /visa | Xem được; không nút Thêm (visas:create thiếu); không edit icon | Pass (code) |
| TC_M12_014 | DocumentStaff xem visa | document | /visa | Đọc visa (visas:read); không tạo/sửa | Pass (code) |
| TC_M12_015 | VisaStaff full | visa | Tạo/sửa visa + flight | Thành công (AllActions) | Pass (code) |
| TC_M12_016 | Recruiter/consultant không truy cập | recruiter | /visa | 403/redirect (không visas:read trong seed) | Pass (code) |
| TC_M12_017 | Agent/parent/student không truy cập | agent | /visa | Không có quyền (không visas:read) | Pass (code) |
| TC_M12_018 | Create re-check server | director | (nếu ép) OpenCreateVisa | `HasPermission("visas:create")` false → cảnh báo | Pass (code) |

## Notification (cross M13)

| TC | Tên | Steps | Expected | Status |
|---|---|---|---|---|
| TC_M12_019 | Visa reminder đúng handler | Visa có InterviewDate gần + HandledBy=VisaStaff X | Reminder gửi X | **Fail via BUG_M12_01** (HandledBy=first-user → gửi sai) |
| TC_M12_020 | Departure reminder tới owners | Flight DepartureDate gần, ActualDepartureAt=null | Reminder tới CandidateOwnersOr(VisaStaff,Director) | Pass (code) |
| TC_M12_021 | Visa Approved/Rejected không nhắc | Visa Approved | Không sinh reminder visa | Pass (code) |

## State / Boundary

| TC | Tên | Steps | Expected | Status |
|---|---|---|---|---|
| TC_M12_022 | VisaStatus nhảy cóc | NotSubmitted → Approved trực tiếp | Cho phép (không state-machine) | **Obs OBS-M12-02** |
| TC_M12_023 | Notes/VisaType Unicode/emoji | Nhập dấu + emoji | Lưu nguyên (text) | Blocked (harness) |
| TC_M12_024 | Ngày phỏng vấn quá khứ/tương lai | Nhập ngày | Lưu; reminder chỉ khi trong [today,horizon] | Pass (code) |
| TC_M12_025 | 2 visa cùng candidate/job | Tạo 2 lần | **Mong đợi:** 1? **Thực tế:** cho tạo trùng (no unique) | **Obs OBS-M12-04** |

## Enum / Entity contract (Automation khả thi ngay)

| TC | Tên | Assert | Status |
|---|---|---|---|
| TC_M12_026 | VisaStatus có đủ 6 trạng thái | NotSubmitted..Rejected | **Automated (unit)** |
| TC_M12_027 | Visa default NotSubmitted | `new Visa().Status == NotSubmitted` | **Automated (unit)** |
| TC_M12_028 | Visa.HandledBy nullable | HandledBy là Guid? | **Automated (unit)** |
| TC_M12_029 | Flight.ActualDepartureAt nullable | ActualDepartureAt là DateTimeOffset? | **Automated (unit)** |
| TC_M12_030 | Flight.AssignedTo nullable | AssignedTo là Guid? | **Automated (unit)** |
