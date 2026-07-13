# M07 — Candidate Workflow · Automation Report

## Framework & môi trường
- `tests/Polymind.Tests` (xUnit, net10.0). Ref Domain + Infrastructure, không ref Web.
- **Lệnh:** `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo` → `Passed 29, Failed 0, Skipped 0`.
- **Web compile:** `dotnet build src/Polymind.Web/Polymind.Web.csproj` → 0/0.

## Automated tests M07
- **Không có test mới.** `WorkflowStepAccess` (CanAdvance) và `WorkflowSteps` (Next/Progress) nằm ở `Polymind.Web/Display` → không ref được từ test project. Mutation logic ở `CandidateDetail.razor`. Không có class Domain thuần để unit-test.
- State-machine + phân quyền verify qua **source review** (xem bảng dưới).

## Verified bằng source review (dòng code)
| Điểm | Bằng chứng |
|---|---|
| Phân quyền chuyển bước | `WorkflowStepAccess.CanAdvance` (map role/bước); re-check `AdvanceStep:1723`, `FailEntranceExam:1857`, `ReassignJobOrder:1911` |
| State tuần tự (không nhảy bước) | `WorkflowSteps.Next` (7→9 skip 7.5, cap Completed) |
| Fail B8 → 7.5 | `FailEntranceExam:1888` `CurrentStep=ReselectJobOrder` |
| Reassign new≠old + deadline | `ReassignJobOrder:1924,1931` |
| B20 gate nợ vay | `AdvanceStep:1777` `_hasOpenLoan` |
| Actor attribution đúng | `GetRequiredUserIdAsync` ở 1797/1875/1937/1979 |
| Duplicate-submit guard | `_busy` ở 1792 |

## Pass / Fail / Blocked
- **Pass (code review):** TC_002–012, 014, 017, 019 (+ regression 29/29 không ảnh hưởng).
- **Fail:** 0 (không có confirmed bug).
- **Blocked (no harness):** TC_001, 013, 015, 016, 018, 020, 021 (runtime advance/commission/concurrency/RB-2 reset).

## Automation backlog
1. Tách `WorkflowStepAccess` + `WorkflowSteps` sang `Polymind.Domain` → unit-test ma trận `CanAdvance(role, step)` (bao 20+ trường hợp) và `Next()` cho toàn enum.
2. Integration harness → advance end-to-end, commission idempotency (M09), concurrency (TC_020, OBS-M07-01).
3. Thêm rowversion `candidate_job_orders` (nếu user chốt) → test lost-update.
