# M15 — AI Assistant · Automation Report

## Framework & phạm vi
- **Framework:** xUnit (`tests/Polymind.Tests`), ref **chỉ** `Polymind.Domain` + `Polymind.Infrastructure`.
- **Thay đổi Codex:** tách quy tắc data-scope AI thuần sang `Polymind.Domain.Ai.AiDataScope`; test project vẫn không tham chiếu Web.
- **Kết quả:** **M15 có 6 automated regression test** cho BUG_M15_01. Không thêm test giả/hard-code.

## Automated tests
| Automated Test | Test Case | Kết quả |
|---|---|---|
| `Agent_scope_only_exposes_its_candidates_leads_and_linked_jobs` | TC_M15_022 | Pass |
| `Collaborator_scope_only_exposes_direct_candidates_leads_and_linked_jobs` | TC_M15_023 | Pass |
| `Missing_partner_mapping_exposes_no_ai_data` | TC_M15_022/023 fail-closed | Pass |
| `Staff_scope_preserves_full_ai_context` | TC_M15_001 regression | Pass |
| `Partner_scope_queries_translate_for_postgresql` (Agent + CTV) | TC_M15_022/023 | Pass 2/2 |

## Lệnh chạy (suite chung — không đổi vì M15 không thêm test)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Codex handoff: Passed 94, Failed 0, Skipped 0.
```

## Phân loại các phát hiện
- **Application Defect:** BUG_M15_01 (data-scope bypass agent/CTV) — **Fixed by Codex, chờ Claude xác minh**.
- **Requirement:** U-M15-1 đã chốt: partner được dùng AI, dữ liệu phải scope theo Agent/CTV.

## Blocked / pending harness
| Hạng mục | Lý do | Cần |
|---|---|---|
| DTO contract (AiResult/AiChatMessage) | ở `Polymind.Web.Ai` | ref Web HOẶC tách DTO sang Domain |
| Dựng ngữ cảnh staff vs self-scoped | Blazor component + AgentScope (Web) + DB | bUnit + Postgres |
| RB-5 persistence/clear multi-session | Web singleton + circuit + logout | integration |
| Chat/Extract E2E | cần Gemini key thật + circuit | E2E (không dùng production key trong test) |
| Leak agent/CTV runtime | Web + DB + role | bUnit/integration với seed agent/CTV; quy tắc query + SQL translation đã automate |

## Automation backlog
- Tách `AiSessionStore` + DTO + một `AiContextBuilder` (nhận scope + DbContext) sang `Polymind.Application`/`Domain` → unit-test: (a) staff nạp toàn bộ, (b) agent/CTV **phải** lọc AgentId, (c) self-scoped chỉ OwnedCandidateId. Đây cũng là đường regression cho BUG_M15_01.
- Harness bUnit cho AiAssistant (render theo role → khẳng định tab trích xuất ẩn/hiện + nội dung context).
