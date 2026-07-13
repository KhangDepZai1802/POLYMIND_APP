# M14 — Messaging / Chat · Traceability Matrix

| Business Flow | Page/Entry | Service/Logic | Role | State/Ref | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M14-01 danh bạ | `/messages` | `LoadContacts`/`MessagingPolicy`/`BuildRelationshipRecipientsAsync` | messages:read | contacts | TC_M14_001..016, 043..046 | 7 M14 tests | **CR-M14-1 Fixed by Codex — chờ Claude** | bUnit/DB runtime pending |
| BF-M14-02 hội thoại + mark read | `/messages` | `LoadThread` | participant | Message.IsRead | TC_M14_017..019 | — | Source ✅ / runtime Blocked | DB mark-read pending |
| BF-M14-03 gửi tin | `/messages` | `Send` (re-check role + relationship từ DB) | sender | Message insert | TC_M14_020..025 | relationship unit | **Fixed by Codex — chờ Claude** | runtime DB pending |
| BF-M14-04 thu hồi | `/messages` | `RecallMessage` | tác giả | Message delete | TC_M14_026..028 | — | Source ✅ / runtime Blocked | audit/attachment cleanup obs |
| BF-M14-05 self-scoped | `/messages` | `BuildRelationshipRecipientsAsync` + AgentScope | parent/student | allowed set đối xứng | TC_M14_029..032, 043..045 | relationship unit | **Fixed by Codex — chờ Claude** | AgentScope resolve DB pending |
| BF-M14-06 attachment | `/messages` | `MinioDocumentStorage` | sender | MinIO object | TC_M14_033..037 | — | Source ✅ / runtime Blocked | MinIO harness pending (M18) |

## Gap analysis

- **Automated:** 7 M14 tests: 2 entity contract + 4 candidate-relationship/fail-closed + 1 PostgreSQL translation. Role-only `MessagingPolicy` vẫn ở Web và kiểm source/manual.
- **Manual/source-verified (✅):** IDOR đọc (scoped me↔other), re-check authz Send 2 nhánh, recall ownership, upload validation, XSS encode, self-scoped allowed set.
- **Không có confirmed bug ban đầu; CR-M14-1 đã Fixed by Codex — chờ Claude.** Partner→staff role policy giữ nguyên; portal relationship nay đối xứng và server re-check.
- **Rủi ro còn lại chưa đo:** IsActive re-check Send (OBS-M14-03), recall no-audit + attachment orphan (OBS-M14-02), no-paging (OBS-M14-04), no-FK orphan (OBS-M14-05).
