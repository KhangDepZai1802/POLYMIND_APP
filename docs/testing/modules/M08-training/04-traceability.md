# M08 — Training · 04 Traceability

> QA: Claude · 2026-07-10.

| Business Flow | Page | API | Role | State | Test Case IDs | Automated Test IDs | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M08-01 List đào tạo | `/training` | — (Blazor) | staff/agent/collab | read | TC_M08_001..006, 020 | M08 role-permission theory | **CR-M08-1 Fixed by Codex — chờ Claude** | Runtime UI/claim refresh pending |
| BF-M08-02 Self-scoped | `/training`, `/training/{id}` | — | parent/student | read | TC_M08_002-scope, 023 | — | Code-verified (scope fail-closed) | Runtime IDOR PoC pending harness |
| BF-M08-03 Agent/Collab scope | `/training/{id}` | — | agent/collaborator | read | TC_M08_019, 024 | — | Code-verified | Runtime pending |
| BF-M08-04 Cập nhật tiến trình | TrainingTrackDialog | — | SA/RM/consultant | update | TC_M08_007..012, 021, 025 | (clamp/trim logic mô phỏng manual) | Code-verified (clamp/attribution/audit) | bUnit dialog + DB assert pending |
| BF-M08-05 Thêm phiếu tuần | TrainingEvaluationDialog | — | SA/RM/consultant | create | TC_M08_013..017, 022, 025, 028, 034 | — | Code-verified (attribution/audit/JSON safe) | Upload MinIO + bUnit pending |
| Enum/entity contract | — | — | — | — | TC_M08_029, 030 | **M08_TrainingRulesTests** (4 test) | **Automated Pass** | — |
| Concurrency record | TrainingTrackDialog | — | SA/RM/consultant | update | TC_M08_031, 032 | — | Gap | OBS-M08-01 (no rowversion) |
| Audit | cả 2 dialog | — | SA/RM/consultant | — | TC_M08_033 | — | Code-verified (AddAudit gọi) | DB assert pending |

## Gap Analysis

| Gap | Loại | Xử lý |
|---|---|---|
| Runtime component/e2e (razor logic: scope redirect, nút theo quyền, timeline tuần) | Test infra | Cần bUnit + Playwright harness — blocker chung repo. Ghi Blocked, không tự khẳng định pass runtime. |
| DB side-effect assert (unique index, audit row, clamp persisted) | Test infra | Cần WebApplicationFactory + DB test (Testcontainers hoặc `polymind_test`). |
| Upload minh chứng (MinIO) | Test infra | Cần MinIO test container → M18. |
| Concurrency unique-index race | Application (nhẹ) | OBS-M08-01 — không phát bug chặn; theo dõi cùng OBS-M07-01 ở M17/M20. |
| Logic clamp/scope/week-grouping bị "nhốt" trong razor | Testability | Đề xuất (cho Codex/refactor): tách helper thuần (VD `TrainingProgress`, `WeekGrouping`) sang Domain để unit-test — KHÔNG làm trong phiên QA này (không sửa business logic). |

**Coverage tuyên bố:** automated phủ contract enum/entity (4) + CR-M08-1 role matrix (4 role cases), tổng M08 8/8. Phần logic razor = **Verified ở mức code**, runtime **Blocked pending harness** — KHÔNG tuyên bố 100%.
