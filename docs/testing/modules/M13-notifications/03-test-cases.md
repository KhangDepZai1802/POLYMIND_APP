# M13 — Notifications · Test Cases

> Quy ước `TC_M13_<NNN>`. Automation Layer: Unit (Domain contract, chạy được), Manual/Integration (Web+DB/Hangfire — pending harness).
> Nguồn: BF-M13-01..06 + RB-6/RB-7. Status: Pass (source-verified) / Blocked (pending runtime harness) / Fail (bug).

## Functional — sinh & hiển thị

| ID | Name | Flow | Role | Steps | Expected | Layer | Status |
|---|---|---|---|---|---|---|---|
| TC_M13_001 | Job nền sinh reminder | BF-M13-01 | system | Chạy `NotificationJob.RunAsync` | Tạo Notification cho recipient đúng loại; set SentAt sau dispatch | Manual/Integration | Blocked |
| TC_M13_002 | Payment due → reminder | BF-M13-01 | Accountant | Payment ≠Paid DueDate≤7d | ReminderPayment tới owner + Accountant/SuperAdmin, không Director, ref=payment | Manual | Blocked (source + role-rule ✅) |
| TC_M13_003 | Visa interview → reminder tới HandledBy | BF-M13-01 | VisaStaff | Visa InterviewDate≤7d, HandledBy=actor | ReminderVisa tới `HandledBy` (actor thật — M12 Verified) | Manual | Blocked (source ✅) |
| TC_M13_004 | Departure → reminder | BF-M13-01 | VisaStaff | Flight DepartureDate≤7d, ActualDeparture null | ReminderDeparture tới owners/VisaStaff/Director | Manual | Blocked |
| TC_M13_005 | Thiếu hồ sơ → reminder | BF-M13-01 | DocumentStaff | CJO Active ≥Document, 0 doc | ReminderDocument tới AssignedTo/DocumentStaff/RM | Manual | Blocked |
| TC_M13_006 | Lead care overdue → reminder + revive | BF-M13-01 | Recruiter | Lead đứng yên quá ngưỡng; đọc rồi vẫn quá hạn >24h | Nhắc lại (revive) chứ không chèn trùng (vỡ unique) | Manual | Blocked (logic ✅) |
| TC_M13_007 | Danh sách unread-first | BF-M13-02 | any | Mở `/notifications` | Unread trên đầu, rồi theo CreatedAt desc | Manual | Blocked (query ✅) |
| TC_M13_008 | Empty state | BF-M13-02 | parent | User không recipient reminder | Hiện "Không có thông báo nào." | Manual | Blocked |
| TC_M13_009 | Badge unread ở chuông | BF-M13-02 | any | Có N unread | MudBadge = N; ẩn khi 0 | Manual | Blocked |

## RB-6 — điều hướng trang nguồn

| ID | Name | referenceType | Expected URL | Layer | Status |
|---|---|---|---|---|---|
| TC_M13_010 | Click lead | lead | `/leads/{id}` | Unit(logic)/Manual | Blocked (source ✅) |
| TC_M13_011 | Click candidate | candidate | `/candidates/{id}` | Manual | Blocked |
| TC_M13_012 | Click payment | payment | `/candidates/{candidateId}` (fallback `/finance`) | Manual | Blocked |
| TC_M13_013 | Click visa | visa | `/candidates/{candidateId}` (fallback `/visa`) | Manual | Blocked |
| TC_M13_014 | Click flight | flight | `/candidates/{candidateId}` (fallback null → không điều hướng) | Manual | Blocked |
| TC_M13_015 | Click commission | commission | `/agents/{agentId}` (fallback `/agents`) | Manual | Blocked |
| TC_M13_016 | Click loan_repayment | loan_repayment | `/candidates/{candidateId}` (join Loan; fallback `/debt-collection`) | Manual | Blocked |
| TC_M13_017 | Click expense | expense | `/finance` | Manual | Blocked |
| TC_M13_018 | referenceId null | any | Không điều hướng (`ResolveTargetUrlAsync`→null) | Manual | Blocked (source ✅) |
| TC_M13_019 | Mark read khi click | BF-M13-03 | any | Click → IsRead=true trước điều hướng | Manual | Blocked (source ✅) |

## Read/Unread & preferences

| ID | Name | Flow | Expected | Layer | Status |
|---|---|---|---|---|---|
| TC_M13_020 | Đánh dấu đã đọc tất cả | BF-M13-04 | Mọi InApp chưa đọc của user → read; disable nút khi unread=0 | Manual | Blocked |
| TC_M13_021 | MarkRead idempotent | BF-M13-04 | Gọi 2 lần → không lỗi, không đổi ReadAt lần 2 | Manual | Blocked (source ✅) |
| TC_M13_022 | Preference default InApp | BF-M13-05 | Type chưa có pref → InApp=true, các kênh khác false | Unit(entity)/Manual | Pass (entity ✅) |
| TC_M13_023 | Lưu preference upsert | BF-M13-05 | Tick Email + Lưu → upsert `(userId,type)`; reload giữ giá trị | Manual | Blocked |
| TC_M13_024 | Tắt InApp cho type | BF-M13-05 | ChannelsFor không yield InApp → không tạo InApp cho type đó | Manual | Blocked (source ✅) |
| TC_M13_025 | ChannelsFor null→InApp | BF-M13-05/06 | pref null → chỉ InApp | Unit(logic)/Manual | Blocked |

## Authorization / IDOR / security

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M13_026 | `/notifications` yêu cầu `notifications:read` | Chưa quyền → chặn policy | Manual | Blocked (attr ✅) |
| TC_M13_027 | Chỉ thấy notification của mình | `GetForUserAsync` lọc `UserId==userId` → không leak chéo user | Manual | Blocked (source ✅) |
| TC_M13_028 | Unread count theo user | `GetUnreadCountAsync` lọc `UserId==userId` | Manual | Blocked (source ✅) |
| TC_M13_029 | MarkRead không kiểm ownership | OBS-M13-03: id lạ → mark read được nhưng **không** có REST + Blazor list server-side → không exploit qua UI | Manual | Blocked (obs) |

## Concurrency / duplicate / DB

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M13_030 | Job trùng/nhiều phiên | unique + seen + catch DbUpdateException → không dupe, không sập | Manual | Blocked (source ✅) |
| TC_M13_031 | Refresh trang | GenerateReminders lại → dedup, không tạo trùng | Manual | Blocked (source ✅) |
| TC_M13_032 | Dedup vĩnh viễn non-LeadCare | OBS-M13-05: đã đọc → không tái nhắc dù event còn hiệu lực (chỉ LeadCare revive) | Manual | Blocked (obs) |
| TC_M13_033 | Unique preference (userId,type) | 2 pref cùng type/user → vỡ unique | Manual | Blocked (index ✅) |

## Boundary / input

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M13_034 | Timezone biên ngày | OBS-M13-04: `DateTime.UtcNow.Date` (VN UTC+7) → lệch ±1 ngày quanh nửa đêm | Manual | Blocked (obs) |
| TC_M13_035 | Không recipient (không accountant/super_admin) | Reminder không có recipient → không crash (inner loop rỗng) | Manual | Blocked (source ✅) |
| TC_M13_036 | Label enum lạ | `Vi/IconOf/ColorOf` có `_ =>` default → không crash | Unit(contract)/Manual | Pass (source ✅) |

## Contract (Unit — chạy được từ test project)

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M13_037 | NotificationType đủ 11 giá trị | gồm RB-7 mới: ReminderLoanRepayment, ExpenseApproval, CommissionPending, CommissionPaid | Unit | Pass |
| TC_M13_038 | NotificationChannel đủ 4 | Email/Sms/Zalo/InApp | Unit | Pass |
| TC_M13_039 | Notification default | IsRead=false, SentAt/ReadAt null | Unit | Pass |
| TC_M13_040 | NotificationPreference default | InApp=true, Email/Sms/Zalo=false | Unit | Pass |

## RB-7 recipient — bug/clarification

| ID | Name | Expected | Actual | Status |
|---|---|---|---|---|
| TC_M13_041 | Hoa hồng tới CTV/Đại lý | Agent + CTV trực tiếp nhận đủ 3 mốc; CTV chỉ thấy share của mình | Agent nhận tổng; CTV event riêng theo `Candidate.CollaboratorId`, route `/my-commissions`, không chứa tổng | **Pass (15/15 M13) — runtime chờ Claude** |
| TC_M13_042 | Tài chính đúng role | Chỉ Kế toán + super_admin; payment/repayment thêm owner ứng viên; không Director | `RecipientRoleNames = [accountant, super_admin]`; source dùng danh sách này | **Pass (unit/source) — runtime chờ Claude** |
