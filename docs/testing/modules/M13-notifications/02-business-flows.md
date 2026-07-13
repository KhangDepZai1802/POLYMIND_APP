# M13 — Notifications · Business Flows

> Nguồn: `NotificationService.BuildReminderEventsAsync` / `PersistEventsAsync` / `SendPendingAsync`, `NotificationJob`, `Notifications.razor`, RB-6/RB-7 (WORKLOG).

---

## BF-M13-01 — Job nền sinh nhắc việc định kỳ (Hangfire)

- **Actor:** hệ thống (RecurringJob `polymind-notification-reminders`, cron `*/5 * * * *`).
- **Preconditions:** Hangfire server chạy; DB sẵn sàng.
- **Main flow:** `NotificationJob.RunAsync` → `GenerateRemindersForAllUsersAsync` (build events từ 9 nguồn, persist theo recipient + kênh) → `SendPendingAsync` (dispatch `SentAt==null`).
- **Validation/dedup:** unique `(UserId,Type,ReferenceId,Channel)` + `seen` set; catch `DbUpdateException` → nuốt trùng.
- **DB changes:** thêm Notification (mỗi recipient×kênh); set `SentAt`.
- **Final state:** notification tồn tại, InApp sẵn hiển thị; email/sms/zalo đã "gửi/queue".
- **Risk:** recipient thiếu Agent/CTV (BUG_M13_01); timezone biên ngày (OBS-M13-04).

## BF-M13-02 — User mở trang `/notifications`

- **Actor:** user có `notifications:read`.
- **Main flow:** `OnInitializedAsync` → `GenerateRemindersAsync(userId)` (scoped: chỉ event có `Recipients.Contains(userId)`; super_admin/director sinh cho mọi recipient) → `LoadAsync` (`GetForUserAsync` InApp của mình, `GetPreferencesAsync`).
- **UI:** danh sách unread-first; badge unread; empty state.
- **Authorization:** policy `notifications:read`; **chỉ thấy notification của mình** (lọc `UserId==userId`).
- **Risk:** IDOR đọc — đóng (lọc theo userId). canSeeAll misnomer (OBS-M13-02).

## BF-M13-03 — Bấm 1 thông báo → điều hướng trang nguồn (RB-6)

- **Actor:** user.
- **Main flow (`MarkAsync`):** nếu chưa đọc → `MarkReadAsync(n.Id)` set `IsRead`; `ResolveTargetUrlAsync(refType, refId)` → điều hướng nếu có URL, ngược lại `LoadAsync`.
- **Bảng RB-6 (referenceType → URL):**

| ReferenceType | URL | Fallback |
|---|---|---|
| lead | `/leads/{id}` | — |
| candidate | `/candidates/{id}` | — |
| payment | `/candidates/{candidateId}` | `/finance` |
| visa | `/candidates/{candidateId}` | `/visa` |
| flight | `/candidates/{candidateId}` | `null` (không điều hướng) |
| commission | `/agents/{agentId}` | `/agents` |
| loan | `/candidates/{candidateId}` | `/loans` |
| loan_repayment | `/candidates/{candidateId}` (join Loan) | `/debt-collection` |
| expense | `/finance` | — |
| (khác/null) | `null` | — |

- **Validation:** `referenceId is not Guid` → null (không điều hướng). Mark read chạy trước điều hướng.
- **Risk:** trang đích tự enforce authz; self-scoped (parent/student) tới `/candidates/{id}` sẽ bị target page chặn nếu ngoài phạm vi (không phải lỗi M13).

## BF-M13-04 — Đánh dấu đã đọc / đã đọc tất cả

- **Main flow:** `MarkReadAsync(id)` (idempotent: return nếu đã đọc); `MarkAllReadAsync(userId)` cập nhật mọi InApp chưa đọc của user.
- **DB:** set `IsRead/ReadAt/UpdatedAt`.
- **Risk:** `MarkReadAsync` không kiểm ownership (OBS-M13-03) — không exploit qua UI (không REST, list server-side).

## BF-M13-05 — Tùy chọn kênh nhận

- **Main flow:** `GetPreferencesAsync` (default InApp bật cho type thiếu) → user tick → `SavePreferencesAsync` upsert theo `(userId,type)`.
- **Hệ quả:** `ChannelsFor` quyết định tập kênh khi sinh reminder: pref null → chỉ InApp; else theo cờ.
- **DB:** upsert NotificationPreference.
- **Risk:** tắt InApp cho 1 type → không tạo InApp cho type đó (đúng ý). Notification cũ đã tạo giữ nguyên.

## BF-M13-06 — Dispatcher gửi kênh chờ (SendPendingAsync)

- **Main flow:** lấy tối đa `take` notification `SentAt==null` theo `CreatedAt`; map kênh→sender; gửi; nếu success set `SentAt`, else log warning (thử lại sau).
- **Sender:** InApp (no-op success); Email (SMTP khi `Email.Enabled` + Host; else log-as-queued); SMS/Zalo (log/queue).
- **Risk:** sender exception/ fail → không set SentAt → retry lần sau. An toàn.

---

## Reminder Event Matrix (nguồn → type → recipient → referenceType)

| Nguồn | NotificationType | Recipient (code) | RB-7 yêu cầu | Khớp? |
|---|---|---|---|---|
| Payment due/overdue (≠Paid/Refunded, DueDate≤horizon) | ReminderPayment | owner ứng viên **else** Accountant/Director | Kế toán + Director/super_admin | ⚠️ owner-first, thiếu super_admin (OBS-M13-01) |
| Visa (Interview/Result trong 7 ngày, ≠Approved/Rejected) | ReminderVisa | `HandledBy` (M12=actor) else VisaStaff/Director | Visa staff + RM | ✅ (M12 Verified) |
| Flight (DepartureDate 7 ngày, chưa ActualDeparture) | ReminderDeparture | owner else VisaStaff/Director | Visa staff + RM | ✅ |
| Lead appointment (7 ngày) | ReminderInterview | `AssignedTo` else Recruiter/RM | — | ✅ |
| Lead care overdue (LeadCareRules) | ReminderLeadCare | assigned/recruiter/consultant + RM/super_admin | góp ý Vietgroup | ✅ (có revive) |
| CJO Active ≥ Document, chưa có doc | ReminderDocument | CJO `AssignedTo` else DocumentStaff/RM | — | ✅ |
| Commission Approved (chờ chi) | CommissionPayment | Agent owner + Accountant/Director | **CTV/Đại lý liên quan + Kế toán** | ◐ Agent done; CTV chờ U-M13-2 |
| Commission Pending (chờ duyệt) | CommissionPending | Agent owner + Accountant/Director | **CTV/Đại lý liên quan + Kế toán** | ◐ Agent done; CTV chờ U-M13-2 |
| Commission Paid (đã chi) | CommissionPaid | Agent owner + Accountant/Director | RB-7 flow "…→ đã chi" | ◐ Event/Agent done; CTV chờ U-M13-2 |
| LoanRepayment due/overdue (≠Paid, DueDate≤horizon) | ReminderLoanRepayment | Accountant/Director | Kế toán + Director/super_admin | ⚠️ thiếu super_admin (OBS-M13-01) |
| Expense chờ duyệt (ApprovedBy null, 60 ngày) | ExpenseApproval | Accountant/Director | Kế toán + Director/super_admin | ⚠️ thiếu super_admin (OBS-M13-01) |

**Kiểm state/duplicate:**
- Không có state-machine cho notification (chỉ IsRead/SentAt); không rowversion (không cần — không mutation nghiệp vụ đồng thời).
- Double click "Quét nhắc việc"/job trùng → dedup an toàn.
- Refresh trang → `GenerateRemindersAsync` lại nhưng dedup → không tạo trùng.
