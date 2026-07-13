# M13 — Notifications · Automation Report

## Framework & phạm vi

- **Framework:** xUnit (`tests/Polymind.Tests`). Test project chỉ ref `Polymind.Domain` + `Polymind.Infrastructure` (KHÔNG ref `Polymind.Web` — tránh khóa DLL khi dev host `:5177` chạy). → chỉ automate được **Domain contract**; logic điều phối M13 nằm trong `Polymind.Web`.
- **File:** `tests/Polymind.Tests/M13_NotificationRulesTests.cs` — contract + lifecycle/recipient regression.

## Automated tests

| Automated Test | Test Case | Loại | Kết quả |
|---|---|---|---|
| `NotificationType_contains_all_expected_values` | TC_M13_037 | contract enum (11 giá trị, gồm CommissionPaid) | Passed |
| `NotificationChannel_contains_four_channels` | TC_M13_038 | contract enum (4 kênh) | Passed |
| `New_notification_defaults_to_unread_unsent` | TC_M13_039 | entity default | Passed |
| `New_preference_defaults_to_inapp_only` | TC_M13_040/022 | entity default (InApp bật) | Passed |
| `New_notification_has_no_reference_by_default` | TC_M13_036 | entity nullable (RB-6) | Passed |
| `Commission_lifecycle_maps_to_notification_type` | TC_M13_041 | Pending/Approved/Paid mapping | Passed (3 rows) |
| `Commission_recipients_include_agent_account_without_duplicates` | TC_M13_041 | Agent recipient + null-safe | Passed |
| `Financial_recipient_roles_are_accountant_and_super_admin_only` | TC_M13_042 / CR-M13-1 | exact role list; Director excluded | Passed |
| `Financial_recipients_union_finance_roles_and_candidate_owners` | TC_M13_002/042 | finance + owner union | Passed |
| `Financial_recipients_keep_finance_roles_when_candidate_has_no_owner` | TC_M13_042 | source không candidate | Passed |
| `Collaborator_notification_contains_share_but_not_agent_total` (3 rows) | TC_M13_041 / BUG_M13_01 | Pending/Approved/Paid chỉ chứa CTV share | Passed 3/3 |

## Lệnh chạy

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả

- **M13:** 15/15. **Toàn suite Codex handoff:** 98 passed, 0 failed, 0 skipped.
- **Web build:** 0 warning / 0 error bằng output riêng.

## Blocked / pending harness

| Hạng mục | Lý do | Cần |
|---|---|---|
| RB-6 URL điều hướng (9 nhánh) | `ResolveTargetUrlAsync` ở Web + cần DB | bUnit/WebApplicationFactory + Postgres |
| Recipient routing (payment/visa/commission/…) | `BuildReminderEventsAsync` ở Web + DB | integration + seed dữ liệu |
| Dedup unique + LeadCare revive | `PersistEventsAsync` ở Web + Postgres unique | integration Postgres |
| Job nền Hangfire | RecurringJob | Hangfire server + Postgres storage |
| Preferences upsert + ChannelsFor | Web + DB | bUnit + DB |
| Sender (SMTP/SMS/Zalo) | provider thật | môi trường tích hợp |

## Automation backlog

- Dựng harness integration (WebApplicationFactory + `polymind_test` DB + Hangfire in-memory) → phủ runtime recipient routing RB-7 (đặc biệt CTV trực tiếp + finance không Director), RB-6 URL, dedup/revive, mark-read IDOR (OBS-M13-03), timezone biên (OBS-M13-04).
- Tách `ResolveTargetUrlAsync` map + `ChannelsFor` sang Domain/Application để unit-test không cần Web (như đã làm với M06/M09/M10/M12 factory).
