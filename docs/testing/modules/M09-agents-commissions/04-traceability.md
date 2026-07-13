# M09 — Agents & Commissions · 04 Traceability

> QA: Claude · 2026-07-10.

| Business Flow | Page/Logic | Role | State | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|
| BF-M09-01 Sinh hoa hồng | CommissionEngine.EnsureAsync | system(actor) | →Pending | TC_M09_001..010 | M09_CommissionRatesTests (rate) | Engine verified(code); rate Pass | **BUG_M09_01** concurrency; integration pending |
| BF-M09-02 Duyệt | AgentDetail.ApproveCommission | SA/director/accountant | Pending→Approved | TC_M09_011,013,014,015 | — | Verified(code) | **BUG_M09_02**; bUnit pending |
| BF-M09-03 Chi | AgentDetail.MarkCommissionPaid | SA/accountant | Approved→Paid | TC_M09_012,016 | — | Verified(code) | **BUG_M09_02**; bUnit pending |
| BF-M09-04 Config | CommissionConfigDialog | SA | — | TC_M09_017,018,019 | (ApplyTo logic manual) | Verified(code) | bUnit pending |
| BF-M09-05 CTV | CollaboratorDialog | SA/RM/recruiter/agent | — | TC_M09_020..023 | — | Verified(code) clamp | bUnit pending |
| BF-M09-06 Portal | MyCommissions | agent/collaborator | read | TC_M09_024..029,034 | snapshot rule/model tests | CR-M09-1 implemented; final regression blocked environment | migration/runtime pending |
| BF-M09-07 Leaderboard | Agents.Load | staff/partner | read | TC_M09_027,035 | visibility rule matrix | CR-M09-2 implemented; final regression blocked environment | e2e pending |
| Contract hằng số | AgentCommissionRates | — | — | TC_M09_030..033 | **M09_CommissionRatesTests (4)** | **Pass** | — |

## Gap Analysis

| Gap | Loại | Xử lý |
|---|---|---|
| Concurrency idempotency hoa hồng | **Application defect** | **BUG_M09_01 (Medium)** → Codex: unique index (AgentId,CandidateId,Milestone) + bắt DbUpdateException. |
| State guard approve/pay | **Application defect** | **BUG_M09_02 (Low)** → Codex: guard `Status==Pending`/`==Approved` server-side. |
| Runtime engine (amount, config match, stage gating) | Test infra | Integration DB test (WebApplicationFactory) — blocker chung. |
| bUnit dialog (clamp, re-check, snackbar) | Test infra | bUnit host pending. |
| e2e portal (scope, mask, leaderboard, redirect) | Test infra | Playwright pending. |
| CTV share snapshot / leaderboard privacy | Change requests | U-M09-1/2 đã chốt; Codex implemented; restore/test rerun đang blocked. |
| `CommissionEngine.Map` + clamp nằm ở Web | Testability | Đề xuất tách sang Domain để unit-test (cho Codex; KHÔNG làm ở QA). |

**Coverage tuyên bố:** phân tích + test case đủ 7 flow + engine + authz + concurrency. Automated **chỉ** phủ contract hằng số hoa hồng (4 Pass). 2 defect phát hiện ở mức code (BUG_M09_01/02) — runtime-repro cần integration parallel (Blocked). KHÔNG tuyên bố 100%.
