# M13 — Notifications · Analysis

> QA phiên #5 (Claude). Đọc source thật: `NotificationService`, `NotificationJob` (Hangfire), `NotificationSender`, `Notifications.razor`, `NotificationBell.razor`, entity + enum + DbContext config + Program.cs.

## 1. Module Overview

- **Module ID:** M13
- **Module name:** Notifications (Nhắc việc tự động + In-app + tùy chọn kênh)
- **Business purpose:** Sinh nhắc việc tự động theo người phụ trách/role (khoản thu, visa, xuất cảnh, hồ sơ, hoa hồng, nợ vay, khoản chi, chăm sóc lead), lưu theo kênh nhận, hiển thị chuông + trang `/notifications`, điều hướng tới trang nguồn khi bấm (RB-6), tùy chọn kênh (In-app/Email/SMS/Zalo) theo loại.
- **Actor:** mọi user đăng nhập có `notifications:read` (tất cả role nghiệp vụ + parent/student). Job nền chạy dưới quyền hệ thống.
- **Role liên quan:** Accountant/Director (tài chính, hoa hồng, khoản chi, nợ vay), VisaStaff (visa/xuất cảnh), Recruiter/Consultant/RM (lead, hồ sơ), DocumentStaff (hồ sơ), Agent/Collaborator (hoa hồng — **xem BUG_M13_01**).
- **Dependencies:** M07 Workflow (bước Document, CJO owner), M10 Finance (payment/expense/receipt), M11 Loans (repayment), M09 Commissions (AgentCommission), M12 Visa/Flight (**đã Verified**: `HandledBy`/`AssignedTo` = actor → nguồn recipient reminder visa đúng).
- **Entry point:** `NotificationBell` (chuông trên layout) → `/notifications`; job nền Hangfire `polymind-notification-reminders` (cron `*/5 * * * *`).
- **Exit point:** Notification lưu DB (theo kênh) → InApp hiển thị / Email-SMS-Zalo qua sender; bấm → mark read + điều hướng.

## 2. Source Code Map

| File | Symbol | Mục đích | Dependency |
|---|---|---|---|
| `src/Polymind.Web/Notifications/NotificationService.cs` | `GenerateRemindersAsync(userId)` | Sinh reminder scoped theo user; super_admin/director `canSeeAll` → sinh cho mọi recipient | DbContext, UserManager, RoleNames |
| " | `GenerateRemindersForAllUsersAsync()` | Sinh cho tất cả (job nền gọi) | " |
| " | `BuildReminderEventsAsync(db)` | Dựng danh sách `ReminderEvent` từ payments/visas/flights/leads/docs/commissions/repayments/expenses | 9 loại nguồn |
| " | `PersistEventsAsync(db, events, currentUserOnly)` | Chống trùng theo `(UserId,Type,ReferenceId,Channel)` + LeadCare revive; ghi Notification theo kênh (pref) | NotificationPreference, unique index |
| " | `SendPendingAsync(take)` | Dispatcher: lấy `SentAt==null`, gửi theo sender kênh, set `SentAt` | INotificationSender |
| " | `GetForUserAsync` / `GetUnreadCountAsync` | Đọc InApp của **chính** user (lọc `UserId==userId`) | — |
| " | `ResolveTargetUrlAsync(refType, refId)` | **RB-6**: map referenceType→URL (lead/candidate/payment/visa/flight/commission/loan/loan_repayment/expense) | DbContext |
| " | `MarkReadAsync` / `MarkAllReadAsync` | Đánh dấu đã đọc (theo id / theo user) | — |
| " | `GetPreferencesAsync` / `SavePreferencesAsync` | Tùy chọn kênh theo user+type (default InApp bật) | NotificationPreference |
| " | `ChannelsFor(pref)` | Ánh xạ pref → tập kênh (null → chỉ InApp) | — |
| `src/Polymind.Web/Notifications/NotificationJob.cs` | `RunAsync()` | Job Hangfire: generate-all + send-pending | NotificationService |
| `src/Polymind.Web/Notifications/NotificationSender.cs` | `InApp/SmtpEmail/LoggingSms/LoggingZalo` sender | Gửi theo kênh; Email dùng SMTP khi Enabled; SMS/Zalo log/queue | NotificationOptions |
| `src/Polymind.Web/Components/Pages/Notifications/Notifications.razor` | trang `/notifications` | Tab Thông báo + tab Kênh nhận; RB-6 click điều hướng | NotificationService |
| `src/Polymind.Web/Components/Layout/NotificationBell.razor` | chuông + badge unread | Điều hướng `/notifications` | NotificationService |
| `src/Polymind.Domain/Entities/Notification.cs` | entity | UserId/Type/Channel/IsRead/Reference*/SentAt/ReadAt | BaseEntity |
| `src/Polymind.Domain/Entities/NotificationPreference.cs` | entity | UserId/Type + 4 cờ kênh (InApp default true) | BaseEntity |
| `src/Polymind.Domain/Enums/Enums.cs:124-139` | `NotificationType` (10), `NotificationChannel` (4) | Enum lưu dạng string | — |
| `src/Polymind.Web/Display/Labels.cs:431-473` | `Vi/IconOf/ColorOf(NotificationType)` | Nhãn/icon/màu; **có `_ =>` default** → không crash khi enum lạ | — |
| `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs:188-197` | index | Notification unique `(UserId,Type,ReferenceId,Channel)` + index `(SentAt,Channel)`; Preference unique `(UserId,Type)` | — |
| `src/Polymind.Web/Program.cs:63-78,269-272` | DI + Hangfire | Đăng ký service/sender/job; RecurringJob cron `*/5` | Hangfire PostgreSQL |

## 3. UI Inventory

- **Trang `/notifications`** (`[Authorize(Policy="notifications:read")]`): PageHeader + 2 nút (Quét nhắc việc / Gửi kênh chờ) + nút Đánh dấu đã đọc (disable khi `_unread==0`); mobile gom vào menu ⋮.
- **Tab "Thông báo":** MudList; item có icon/màu theo `Labels`, chip "Mới" khi chưa đọc, nền nhấn khi chưa đọc, thời điểm `CreatedAt.LocalDateTime`. Empty state khi rỗng.
- **Tab "Kênh nhận":** bảng NotificationPreference (Loại × 4 checkbox In-app/Email/SMS/Zalo) + nút Lưu tùy chọn. Ghi chú Email/SMS/Zalo adapter.
- **NotificationBell:** MudBadge unread + IconButton → `/notifications`. Load unread ở `OnInitializedAsync`.
- **Loading state:** `_loading` → MudProgressLinear.

## 4. API Inventory

- **Không có REST endpoint** cho notifications (Program.cs chỉ map auth/leads/candidates/joborders). Mọi thao tác qua Blazor Server circuit gọi `NotificationService` server-side.
- **Job nền (Hangfire):** `NotificationJob.RunAsync` — không phải HTTP; chạy theo cron; không nhận input người dùng.

## 5. Database Impact

| Entity | Table | Constraint | Ghi chú |
|---|---|---|---|
| Notification | notifications | **unique `(UserId,Type,ReferenceId,Channel)`** | chống trùng nhắc việc; index `(SentAt,Channel)` cho dispatcher |
| NotificationPreference | notification_preferences | unique `(UserId,Type)` | 1 pref/user/type |
| (đọc) Payment/Visa/Flight/Lead/CandidateDocument/AgentCommission/LoanRepayment/Loan/Expense/CandidateJobOrder/Candidate/Agent | — | chỉ READ để dựng reminder | không mutation nguồn |

- `CreatedAt/UpdatedAt/SentAt/ReadAt` là `DateTimeOffset` UTC (quy ước app).
- Enum lưu string (`EnumToStringConverter`) → thêm giá trị mới không cần migration.

## 6. Roles & Permissions

| Action | Role | UI | Business condition | Source |
|---|---|---|---|---|
| Xem trang `/notifications` | tất cả role có `notifications:read` (mọi role nghiệp vụ + parent/student) | policy | chỉ thấy notification **của chính mình** (`GetForUserAsync` lọc `UserId==userId`) | Notifications.razor:2, DbSeeder |
| Nhận reminder tài chính (thu/chi/nợ vay) | Accountant, Director (+ owner ứng viên cho payment) | — | **RB-7 nêu "Kế toán + Director/super_admin"** — super_admin không nằm trong recipient (OBS-M13-01) | NotificationService:265-278,422-464 |
| Nhận reminder hoa hồng | Agent owner, Accountant, Director | — | Codex đã thêm Agent + Pending/Approved/Paid; CTV còn chờ U-M13-2 | NotificationService commission block |
| Nhận reminder visa/phỏng vấn | `Visa.HandledBy` (đã Verified M12 = actor) else VisaStaff/Director | — | route đúng nhờ M12 fix | NotificationService:281-299 |
| Nhận reminder xuất cảnh | candidate owners else VisaStaff/Director | — | không dùng `Flight.AssignedTo` | NotificationService:301-313 |
| Sửa tùy chọn kênh | chính user | pref table | upsert theo user | SavePreferencesAsync |

## 7. Risk Analysis

- **Recipient còn thiếu quyết định (RB-7):** Agent owner đã nhận đủ lifecycle; CTV direct/all-tree và mức tiền được xem còn U-M13-2. Finance thiếu super_admin + payment owner-first vẫn là OBS-M13-01.
- **IDOR đọc thông báo:** `GetForUserAsync`/`GetUnreadCountAsync` lọc `UserId==userId` → **không leak** chéo user. `MarkReadAsync(id)` không kiểm ownership (OBS-M13-03) nhưng **không có REST endpoint** + Blazor Server giữ list server-side → không exploit qua UI.
- **Duplicate/double-submit:** unique index + `seen` set + catch `DbUpdateException` → job chạy trùng/nhiều phiên không vỡ trang, không tạo dupe.
- **Stale/dedup vĩnh viễn:** non-LeadCare types dedup theo reference key kể cả đã đọc → không tái nhắc dù sự kiện còn hiệu lực (OBS-M13-05, intentional; chỉ LeadCare revive).
- **Timezone:** biên ngày dùng `DateTime.UtcNow.Date` (VN UTC+7) → lệch ±1 ngày quanh nửa đêm (OBS-M13-04, nhất quán quy ước UTC).
- **canSeeAll misnomer:** super_admin/director sinh cho mọi recipient nhưng `GetForUserAsync` vẫn lọc theo mình → không thực sự "thấy tất cả" (OBS-M13-02).
- **Notification content:** Title/Body render qua Blazor (auto-encode) → không XSS. Không nhận input tự do người dùng ở reminder.
- **Sender failure:** SendPendingAsync try/catch mỗi notification, log warning, không set SentAt khi fail → sẽ thử lại lần sau. An toàn.

## 8. Unknowns / Needs Requirement Clarification

- **U-M13-1 (OBS-M13-01):** RB-7 "Tài chính → Kế toán + Director/super_admin" — có cần thêm **super_admin** vào recipient tài chính? Payment reminder hiện gửi **owner ứng viên trước** (recruiter/consultant), chỉ fallback accountant/director khi không có owner — đúng ý đồ hay cần đổi sang accountant/director trực tiếp?
- **BUG_M13_01:** Agent owner + event Paid đã được Codex xử lý. Còn cần user chốt CTV nào nhận (direct/all-tree) và nội dung tiền (tổng/share/không nêu) trước khi hoàn tất.
