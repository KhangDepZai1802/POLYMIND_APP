# M15 — AI Assistant · Verification Report

> Claude độc lập xác minh bản sửa BUG_M15_01 của Codex. Không sửa business logic.
> Nguồn đối chiếu: `06-bug-report.md`, `01-analysis.md`, `02-business-flows.md`, `03-test-cases.md`, diff source + test.

## 0. Bối cảnh handoff (bất thường — ghi rõ)

- **Codex đã sửa BUG_M15_01 lúc ~14:55 (2026-07-11), SAU khi board/checkpoint phiên #6 chốt lúc 14:53**, nhưng **KHÔNG viết `07-fix-report.md`** và **KHÔNG cập nhật `MODULE_QA_BOARD.md`/`SESSION_CHECKPOINT.md`**. `06-bug-report.md` đã được cập nhật trạng thái "Fixed by Codex — Waiting for Claude Verification".
- Do thiếu `07-fix-report.md`, Claude xác minh dựa trên: (a) `06-bug-report.md` (đã ghi hướng sửa đã chốt), (b) requirement **U-M15-1 đã chốt**, (c) diff source thực tế, (d) automated test mới, (e) build/suite.
- **Verification Status = Verifying → Verified (code-level).**

## 1. Bug xác minh

| Bug ID | Severity | Verdict |
|---|---|---|
| BUG_M15_01 | Medium | **Verified Fixed (code-level)** |

## 2. Yêu cầu nghiệp vụ áp dụng (U-M15-1 đã chốt 2026-07-11)

> Đại lý/CTV **ĐƯỢC** dùng Trợ lý AI, nhưng AI **chỉ nạp ứng viên trong phạm vi của họ** (lọc `AgentId`/`CollaboratorId` như các màn khác). Fail-closed khi tài khoản partner chưa gắn đại lý/CTV.

Bản sửa của Codex đi đúng hướng "query-level filtering" (không phải chặn route) — khớp U-M15-1.

## 3. Bằng chứng đã đọc / kiểm

### 3.1 Domain — `src/Polymind.Domain/Ai/AiDataScope.cs` (MỚI)
- `readonly record struct AiDataScope(AiDataScopeKind Kind, Guid? ScopeId)` với 4 kind: `None`, `All`, `Agent`, `Collaborator`.
- `ForAgent(agentId)` / `ForCollaborator(collaboratorId)` / `All` / `None`.
- `ApplyCandidates` / `ApplyLeads` / `ApplyJobOrders(query, assignments)`:
  - `All` → trả nguyên query (staff).
  - `Agent` → `Where(candidate.AgentId == agentId)` / lead `AgentId` / job qua `assignments.Any(link.JobOrderId==job.Id && link.Candidate.AgentId==agentId)`.
  - `Collaborator` → `Where(CollaboratorId == collaboratorId)` tương ứng.
  - **`_ => query.Where(_ => false)`** (kể cả `None`, hoặc `Agent`/`Collaborator` khi `ScopeId` null) → **fail-closed, trả rỗng**. Đây là điểm mấu chốt: partner chưa gắn mapping KHÔNG lộ gì.
- Kiến trúc sạch: tách hẳn ra Domain (record struct thuần) → unit-test được không cần ref Web (đúng "quick win refactor" đề xuất ở checkpoint).

### 3.2 Web — `AiAssistant.razor` (wired-in)
- `OnInitializedAsync` (dòng 227-234): resolve `_dataScope` từ `AgentScope.GetAsync()`:
  - `scope.IsAgentOnly` → `scope.AgentId is Guid ? AiDataScope.ForAgent(agentId) : AiDataScope.None`.
  - `scope.IsCollaboratorOnly` → `scope.CollaboratorId is Guid ? AiDataScope.ForCollaborator(collaboratorId) : AiDataScope.None`.
  - còn lại (staff) → `AiDataScope.All`.
- `BuildDataContextAsync` (dòng 314-384): **cả 3 nguồn** đi qua scope —
  - `candidateQuery = _dataScope.ApplyCandidates(db.Candidates.AsNoTracking())`
  - `leadQuery = _dataScope.ApplyLeads(db.Leads.AsNoTracking())`
  - `jobQuery = _dataScope.ApplyJobOrders(db.JobOrders.AsNoTracking(), db.CandidateJobOrders.AsNoTracking())`
  - **Mọi aggregate/list phái sinh** (`jobInfo`, `cands`, `rows`, `leadCount`, `leadsByStatus`, `jobCount`, danh sách 100 ứng viên) đều build từ các query đã lọc → **không còn đường nào nạp ứng viên ngoài phạm vi vào prompt**.
- Self-scoped (parent/student) giữ nguyên `BuildSelfDataContextAsync` (chỉ đúng `OwnedCandidateId`) — không đổi, vẫn cô lập.

### 3.3 `src/Polymind.Web/Identity/AgentScope.cs` (nguồn scope)
- `IsAgentOnly = role Agent && !hasStaffRole`; `IsCollaboratorOnly = !isAgentOnly && role Collaborator && !hasStaffRole`.
- `AgentId` nạp từ `db.Agents.Where(a => a.UserId == userId)`; `CollaboratorId` từ `db.Collaborators.Where(c => c.UserId == userId)`. Tài khoản partner **chưa gắn** đại lý/CTV → `AgentId`/`CollaboratorId` = null → `AiDataScope.None` (fail-closed).
- Staff (8 role) → không phải partner-only → `All` → giữ full context (không hồi quy staff).

## 4. Kiểm tra chống né test / workaround nguy hiểm (PHẦN G mục 10-11)

- **Không** đổi expected result để pass; **không** xóa/skip/weaken test.
- Test `M15_AiDataScopeTests.cs` (6) assert scoping **thật**:
  - `Agent_scope_only_exposes_its_candidates_leads_and_linked_jobs` — mọi candidate/lead trả về có `AgentId == _agentId`; job = 2 (job của agent + job của CTV thuộc agent).
  - `Collaborator_scope_only_exposes_direct_candidates_leads_and_linked_jobs` — chỉ ứng viên/lead có `CollaboratorId == _collaboratorId`.
  - `Missing_partner_mapping_exposes_no_ai_data` — `None` trả rỗng cả 3 (fail-closed).
  - `Staff_scope_preserves_full_ai_context` — `All` giữ nguyên count.
  - `Partner_scope_queries_translate_for_postgresql` (Theory ×2) — `ToQueryString()` sinh `WHERE`/`EXISTS` → chứng minh lọc chạy **ở SQL** (không phải client-side sau khi đã tải toàn bộ).
- Không hard-code, không bỏ nhánh; logic dùng `AgentId`/`CollaboratorId` thật.

## 5. Build & Test

| Hạng mục | Lệnh | Kết quả |
|---|---|---|
| Unit/regression | `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo` | **Passed 94, Failed 0, Skipped 0** (88 cũ + 6 M15 mới) |
| Web build | `dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo` | **0 Warning, 0 Error** |

## 6. Kết luận từng bug

- **BUG_M15_01 → Verified Fixed (code-level).** Đại lý/CTV chỉ nạp ứng viên/lead/job trong phạm vi mình vào ngữ cảnh AI; partner chưa gắn mapping = rỗng (fail-closed); staff giữ full; parent/student giữ self-scope. Regression 6/6, suite 94/94, Web 0/0. Không né test, không workaround nguy hiểm.

## 7. Residual / chưa đo (ghi rõ — không tuyên bố 100%)

- **R-M15-A (runtime E2E):** Chưa đăng nhập agent/CTV thật gọi Gemini thật để quan sát câu trả lời chỉ trong phạm vi — pending harness UI + Gemini key. Rủi ro thấp vì lọc ở tầng SQL đã chứng minh bằng `ToQueryString`.
- **R-M15-B (OBS-M15-05 `catch{}` nuốt lỗi):** nếu build context lỗi, staff vẫn chat thiếu ngữ cảnh mà không báo; **với partner** nếu `BuildDataContextAsync` ném lỗi thì rơi vào `catch` → `_systemPrompt` giữ `ChatSystemBase` (không kèm data) → **an toàn** (không lộ), chỉ mất ngữ cảnh. Non-blocking.
- **OBS-M15-01/02/03/04 giữ nguyên** (memory eviction, rate-limit Gemini, prompt injection, CandidateAnalysisDialog authz) — non-blocking, ngoài phạm vi bug này.
- **U-M09-2 (ẩn doanh số đối thủ):** bản sửa AI đã theo đúng tinh thần (đại lý chỉ thấy ứng viên mình) — nhất quán, không cần thêm ở M15.

## 8. Cập nhật trạng thái

- `06-bug-report.md`: BUG_M15_01 → **Verified Fixed (code-level)**.
- Board: M15 → `QA=Completed`, `Codex=Fixed`, `Verification=Verified (code-level)`. Gỡ BUG_M15_01 khỏi Codex Queue.
- **Lưu ý cho Codex:** lần sau khi fix, vui lòng tạo `07-fix-report.md` + cập nhật board/checkpoint theo giao thức handoff (lần này thiếu, Claude đã tự đối chiếu diff + requirement để xác minh).
