# M14 — Messaging / Chat · Automation Report

## Framework & phạm vi

- **Framework:** xUnit (`tests/Polymind.Tests`). Ref chỉ `Polymind.Domain` + `Polymind.Infrastructure` (không ref `Polymind.Web`).
- **Codex:** tách relationship graph + staff candidate scope sang `Polymind.Domain.Messaging`; `Send`/render vẫn ở Web.
- **File:** `tests/Polymind.Tests/M14_MessagingRulesTests.cs` — 7 test.

## Automated tests

| Automated Test | Test Case | Loại | Kết quả |
|---|---|---|---|
| `New_message_defaults_to_unread` | TC_M14_041 | entity default | Passed |
| `Message_retains_participants_and_body` | TC_M14_042 | entity contract | Passed |
| `Responsible_staff_can_message_candidate_portal_accounts` | TC_M14_043 | relationship | Passed |
| `Unrelated_user_cannot_message_candidate_portal_accounts` | TC_M14_044 | fail-closed | Passed |
| `Portal_account_can_reply_to_all_responsible_users_and_family` | TC_M14_045 | symmetric relationship | Passed |
| `Missing_portal_links_fail_closed` | TC_M14_044 | missing mapping | Passed |
| `Responsible_staff_scope_translates_for_postgresql` | TC_M14_046 | EF translation | Passed |

## Lệnh chạy

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả

- **M14:** 7/7. **Toàn suite Codex handoff:** 106 passed, 0 failed, 0 skipped.

## Blocked / pending harness

| Hạng mục | Lý do | Cần |
|---|---|---|
| MessagingPolicy ma trận role còn lại | pure static ở Web | tách Domain HOẶC bUnit |
| Send re-check authz (self-scoped/policy) | Web + DB | integration + AgentScope |
| IDOR đọc hội thoại | Web + DB | integration |
| Attachment upload/validate/download | MinIO | MinIO harness (M18) |
| Recall ownership | Web + DB | integration |

## Automation backlog

- Có thể tách nốt role-only `MessagingPolicy` sang Domain để unit-test toàn sender×recipient; relationship security đã có regression.
- Harness integration (WebApplicationFactory + Postgres + MinIO) → phủ IDOR đọc, re-check Send, self-scoped allowed set, upload validation, recall ownership.
