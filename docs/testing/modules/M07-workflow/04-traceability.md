# M07 — Candidate Workflow · Traceability

| Business Flow | Code | Role | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M07-01 Advance | `AdvanceStep` + `WorkflowSteps.Next` + `CanAdvance` | nhóm bước | TC_001–008, 017–019 | code review | AuthZ + state + validation ✔ (code) | Runtime advance + commission |
| BF-M07-02 Fail B8 | `FailEntranceExam` | Hồ sơ/Tuyển dụng | TC_009–010 | code review | AuthZ + branch ✔ (code) | Runtime |
| BF-M07-03 Reassign | `ReassignJobOrder` | Tuyển dụng | TC_011–013 | code review | new≠old + deadline ✔ (code) | Runtime |
| BF-M07-04 Overseas log | `AddOverseasLog` | Tuyển dụng/Hồ sơ | TC_016 | — | code | Runtime |
| BF-M07-05 Completed B20 | `AdvanceStep` OverseasSupport gate | Tuyển dụng/Hồ sơ | TC_014–015 | code review | loan-settled gate ✔ (code) | Runtime |
| BF-M07-06 RB-2 reset | `ChangeJobOrderAsync` | super_admin | TC_021 | code review (@M05) | AuthZ ✔; refund? U1 | Spec + runtime |

## Coverage summary
- **Verified (code review):** phân quyền chuyển bước theo nhóm (CanAdvance re-check ở cả 3 mutation); state-machine tuần tự (`Next()` 7→9, 7.5 chỉ qua fail); validation gate từng bước; reselect new≠old+deadline; B20 loan-settled gate; **actor attribution đúng** (GetRequiredUserIdAsync — không dính anti-pattern first-user).
- **Confirmed bugs:** **0**.
- **Observations:** OBS-M07-01 (concurrency/stale-state — no rowversion).
- **Blocked (no harness):** advance/fail/reassign/complete runtime, commission idempotency (M09), concurrency race.
- **Requirement Clarification ĐÃ CHỐT (user 2026-07-10):** U1 (RB-2 reset đơn hàng) **KHÔNG** hoàn tiền/hoa hồng → hành vi đúng, không bug.

## Rủi ro còn lại
- Không rowversion trên `candidate_job_orders` → 2 advance đồng thời có thể double/skip (OBS-M07-01, cần integration test).
- Không unit test trực tiếp cho `WorkflowStepAccess`/`WorkflowSteps` (ở Web) — backlog tách Domain.
