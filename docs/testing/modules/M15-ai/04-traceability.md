# M15 — AI Assistant · Traceability

| Business Flow | Page/Service | Role | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M15-01 Chat staff | AiAssistant / GeminiClient.ChatAsync | staff | TC_M15_001/002/006/040/041 | — | Source-verified | Runtime cần key + bUnit |
| BF-M15-02 Chat self-scoped | AiAssistant.BuildSelfDataContext | parent/student | TC_M15_020/021/030 | — | Source-verified (isolation) | Runtime bUnit |
| BF-M15-03 Trích xuất CV | GeminiClient.ExtractFromFile | staff | TC_M15_003/004/005 | — | Source-verified | Runtime |
| BF-M15-04 Agent/CTV data-scope | AiAssistant.BuildDataContext + AiDataScope | agent/CTV | TC_M15_022/023 | `M15_AiDataScopeTests` (6 cases gồm SQL translation) | **Fixed by Codex — chờ Claude** | Runtime UI/Gemini E2E |
| BF-M15-05 RB-5 | AiSessionStore / Logout hook | any | TC_M15_010..013 | — | Source-verified | Runtime multi-session |
| BF-M15-06 Analyze 1 ứng viên | CandidateAnalysisDialog | staff | (qua M05 scope) | — | Source-verified | Dialog no independent authz (OBS-M15-04) |
| Contract DTO | AiModels | — | TC_M15_042/043 | **Blocked** (DTO ở Web) | Không automate được | Ref Web hoặc tách DTO |

## Gap analysis
- **Automation hiện có:** `AiDataScope` đã tách sang Domain và có 6 regression case cho staff/Agent/CTV/missing mapping + EF PostgreSQL translation. Phần render Blazor, DTO và gọi Gemini vẫn ở Web nên cần bUnit/integration.
- **Runtime gap:** cần Gemini key + circuit Blazor để E2E chat/extract; multi-session RB-5; xác nhận leak agent/CTV trên UI thật.
- **Automated hiện có cho M15:** 6 test pass; toàn suite 94/94 tại Codex handoff.
