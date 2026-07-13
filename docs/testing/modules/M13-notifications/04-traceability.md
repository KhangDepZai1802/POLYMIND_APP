# M13 — Notifications · Traceability Matrix

| Business Flow | Page/Entry | API/Service | Role | State/Ref | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M13-01 sinh reminder (job) | Hangfire `polymind-notification-reminders` | `GenerateRemindersForAllUsersAsync`/`BuildReminderEventsAsync`/`SendPendingAsync` | system | 9 nguồn → Notification | TC_M13_001..006, 030, 035 | contract (037-040) | Source ✅ / runtime Blocked | Hangfire + DB E2E pending harness |
| BF-M13-02 xem trang | `/notifications` | `GenerateRemindersAsync`/`GetForUserAsync`/`GetUnreadCountAsync` | notifications:read | InApp của mình | TC_M13_007..009, 026..028 | — | Source ✅ / runtime Blocked | bUnit/UI pending |
| BF-M13-03 RB-6 điều hướng | `Notifications.razor MarkAsync` | `ResolveTargetUrlAsync`/`MarkReadAsync` | any | referenceType→URL | TC_M13_010..019 | — | Source ✅ / runtime Blocked | 9 nhánh URL cần bấm thật |
| BF-M13-04 read/read-all | `/notifications` | `MarkReadAsync`/`MarkAllReadAsync` | any | IsRead/ReadAt | TC_M13_020, 021, 029 | — | Source ✅ / runtime Blocked | ownership check obs |
| BF-M13-05 preferences | tab Kênh nhận | `GetPreferencesAsync`/`SavePreferencesAsync`/`ChannelsFor` | any | pref (userId,type) | TC_M13_022..025, 033 | contract (040) | Source ✅ / runtime Blocked | upsert reload UI pending |
| BF-M13-06 dispatch | nút "Gửi kênh chờ"/job | `SendPendingAsync` + senders | any/system | SentAt | TC_M13_001, 024 | — | Source ✅ / runtime Blocked | SMTP/SMS/Zalo provider thật |
| RB-7 hoa hồng recipient | job | `BuildReminderEventsAsync` (commission) | Agent/CTV/Accountant/SuperAdmin | Commission Pending/Approved/Paid | TC_M13_041 | 3 lifecycle + 3 CTV-content + recipient tests | **Fixed by Codex — chờ Claude** | Runtime DB/Hangfire |
| RB-7 tài chính recipient | job | `BuildReminderEventsAsync` (payment/expense/repayment) | Accountant/SuperAdmin + candidate owner | due/overdue/pending | TC_M13_042 | exact role-list + union tests | **Fixed by Codex — chờ Claude** | Runtime DB/Hangfire |

## Gap analysis

- **Automated (chạy được):** chỉ contract Domain (enum/entity default) — TC_M13_037..040, một phần 022/036. Toàn bộ logic điều phối (`ResolveTargetUrlAsync`, `ChannelsFor`, `PersistEventsAsync` dedup/revive, recipient routing) nằm trong `Polymind.Web` → **không unit-test được** từ test project (không ref Web, tránh khóa DLL — blocker chung). Cần bUnit/WebApplicationFactory + Postgres + Hangfire harness.
- **Manual/source-verified (✅ ở source, runtime Blocked):** RB-6 URL map, IDOR đọc (lọc UserId), dedup unique+catch, mark-read idempotent, visa reminder tới HandledBy (M12 Verified).
- **Codex regression:** 15/15 M13 pass; toàn suite 98/98. BUG_M13_01 + CR-M13-1 đã hết clarification và đang chờ Claude xác minh runtime/source độc lập.
- **Rủi ro còn lại chưa đo:** timezone biên ngày (OBS-M13-04), dedup vĩnh viễn non-LeadCare (OBS-M13-05), MarkRead ownership (OBS-M13-03 — không exploit qua UI), canSeeAll misnomer (OBS-M13-02).
