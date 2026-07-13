# M04 — Lead CRM · Traceability

| Business Flow | Page/API | Điểm phân quyền | State/Action | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|
| BF-M04-01 Tạo Lead | LeadDialog / POST /api/leads | `leads:create` | create | TC_M04_001,002,015 | — | Manual + API blocked | harness API |
| BF-M04-02 Đổi trạng thái | LeadDetail | `leads:update`+CanEditLead | StatusChange | TC_M04_003,004 | — | Manual | — |
| BF-M04-03 Phân công | LeadDetail | `leads:update` | assign | TC_M04_005 | — | Manual | — |
| BF-M04-04 Lịch hẹn | LeadDetail | `leads:update` | update | TC_M04_006 | — | Manual | — |
| BF-M04-05 Convert | LeadDetail | `leads:update`+`candidates:create` | create candidate | TC_M04_007,008,009,010 | — | Manual + Integration blocked | BUG_M04_01, race |
| BF-M04-06 Revert | LeadDetail | `leads:update` | delete candidate | TC_M04_011,012 | — | Manual | — |
| BF-M04-07 Xóa Lead | LeadDetail / DELETE /api/leads | `leads:delete`+CanDeleteLead | delete | TC_M04_013,014 | — | Manual | — |
| BF-M04-08 Lead-care | NotificationService / Leads / LeadDetail | hệ thống | ReminderLeadCare | TC_M04_017,018,019,020,021 | — | Unit **blocked (Web ref)** | tách LeadCareRules để unit test |
| (list/search) | Leads | `leads:read` | read | TC_M04_016,022,023 | — | Manual + API blocked | — |

## Tổng hợp coverage
- **Business flows:** 8/8 có test case.
- **Test cases:** 23; **Automated (chạy được session này):** 0; **Unit-blocked (Web ref lock):** 4 (TC_017–020); **Integration-blocked:** 5; **Manual:** phần còn lại.
- **Bug:** BUG_M04_01 (Low). Rủi ro theo dõi: perf client-side filter, convert race.
- **Gap chính:** `LeadCareRules`/`BusinessRoleAccess` là logic thuần đáng unit test nhưng ở `Polymind.Web` → không automate được đến khi tách khỏi Web hoặc dừng dev server để ref Web.
