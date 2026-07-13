# M15 — AI Assistant · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M15_<NN>`.

## BUG_M15_01 — Trợ lý AI lộ TOÀN BỘ danh sách ứng viên cho Đại lý/CTV (vượt phạm vi data-scope)

- **Bug ID:** BUG_M15_01
- **Module ID:** M15 (data-scope; liên quan M02 AgentScope)
- **Title:** `AiAssistant.BuildDataContextAsync` nạp **toàn bộ** `Candidates`/`Leads`/`JobOrders` **không lọc theo `AgentId`** vào system prompt. Với **agent-only** và **collaborator-only** (`_selfScoped=false` vì họ không phải parent/student), trợ lý AI liệt kê được tên/giới tính/tỉnh/quốc gia/bước/tiến độ của **mọi ứng viên trong công ty** — kể cả ứng viên của đại lý khác (đối thủ). Mọi màn khác (DebtCollection, LoanDialog, danh sách ứng viên…) đều lọc `scope.IsAgentOnly` theo `AgentId`; riêng AI thì không.
- **Severity:** **Medium** (information disclosure vượt ranh giới đại lý: lộ PII ứng viên + tình báo cạnh tranh "ai gần xuất cảnh"; actor là partner role hợp lệ, không leo thang admin).
- **Priority:** P2
- **Business Flow ID:** BF-M15-04
- **Test Case ID:** TC_M15_022 (agent, URL), TC_M15_023 (CTV, UI)
- **Environment:** mọi môi trường có Gemini key.
- **Role:** agent (đại lý), collaborator (CTV).
- **Preconditions:** đăng nhập bằng tài khoản đại lý hoặc CTV; Gemini key cấu hình.
- **Steps to Reproduce:**
  1. **CTV:** đăng nhập CTV → icon "Trợ lý AI" ở thanh trên **hiện** (chỉ ẩn với `_isAgentOnly`) → bấm → hỏi "liệt kê tất cả ứng viên đang có". **Đại lý:** icon ẩn nhưng gõ thẳng URL `/ai` (chỉ `[Authorize]`, không policy) → hỏi tương tự.
  2. Quan sát AI trả lời dựa trên context chứa toàn bộ roster.
- **Expected Result:** Với đại lý/CTV, AI **chỉ** được dùng dữ liệu ứng viên thuộc phạm vi đại lý mình (giống các màn khác) — HOẶC chặn `/ai` cho partner role. (Cần U-M15-1 chốt hướng.)
- **Actual Result:** context = toàn bộ ứng viên/lead/job không lọc → AI liệt kê được ứng viên của đại lý khác.
- **UI Evidence:** MainLayout icon AI hiện với CTV (`!_isAgentOnly`), AiAssistant chat tab dùng `_systemPrompt` = full context.
- **API/Source Evidence:**
  - `src/Polymind.Web/Components/Pages/Ai/AiAssistant.razor:308-372` — `BuildDataContextAsync`: `db.Candidates`, `db.Leads`, `db.JobOrders` **không** `Where(AgentId==scope.AgentId)`.
  - `AiAssistant.razor:226-240` — chọn nhánh: `_selfScoped = scope.IsSelfScoped`; `!_selfScoped` → full context. Không xét `IsAgentOnly`/`IsCollaboratorOnly`.
  - `src/Polymind.Web/Identity/AgentScope.cs:61-62` — `IsSelfScoped` chỉ true cho parent/student → agent/CTV = false.
  - `src/Polymind.Web/Components/Layout/MainLayout.razor:23-30` — icon AI ẩn khi `_isAgentOnly`, **không** ẩn với CTV.
  - `AiAssistant.razor:2` — `[Authorize]` không policy → agent vào bằng URL được.
- **Database Evidence:** chỉ đọc, không ghi; leak nằm ở nội dung prompt gửi Gemini.
- **Suspected Source Area:** thiếu nhánh lọc data-scope cho partner trong `BuildDataContextAsync` + thiếu chặn route cho partner.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Ai/AiAssistant.razor` (BuildDataContextAsync + chọn chế độ)
  - `src/Polymind.Web/Identity/AgentScope.cs` (đã có `IsAgentOnly`/`AgentId` để lọc)
  - `src/Polymind.Web/Components/Layout/MainLayout.razor` (icon), `NavMenu.razor`
- **Dependencies:** M02 AgentScope (đã Verified) cung cấp `AgentId` để lọc.
- **Regression Risk:** Thấp — thêm nhánh lọc/route guard, không đổi schema.
- **Confidence Level:** Cao (source rõ + AgentScope semantics rõ + đối chiếu các màn khác đều lọc).
- **Status:** **Verified Fixed (code-level) — Claude 2026-07-11 (phiên #7).** `AiDataScope` lọc candidate/lead/job theo Agent/CTV và fail-closed khi thiếu mapping; cả 3 nguồn + aggregate đều đi qua scope; 6/6 regression + toàn suite 94/94; Web build 0/0. Xem `08-verification-report.md`. Residual R-M15-A (runtime E2E Gemini) pending harness. **Handoff bất thường:** Codex sửa nhưng thiếu `07-fix-report.md` + không cập nhật board (Claude đối chiếu diff + U-M15-1 để xác minh).
- **Hướng sửa (đã chốt):** trong `BuildDataContextAsync`, khi `scope.IsAgentOnly` → lọc `Candidates.Where(c => c.AgentId == scope.AgentId)`; khi `scope.IsCollaboratorOnly` → lọc theo ứng viên CTV giới thiệu (như các màn CTV, qua `CollaboratorId`); áp tương tự cho Leads/JobOrders (hoặc bỏ phần lead/job nếu ngoài phạm vi đại lý). Cân nhắc U-M09-2 (ẩn doanh số đối thủ). **Không** để prompt chứa ứng viên ngoài phạm vi. Giữ self-scoped (parent/student) như cũ.

---

## Observations (theo dõi — không handoff trừ khi user chốt)

- **OBS-M15-01 — AiSessionStore không evict trừ logout chủ động (Low, memory):** singleton in-memory, chỉ `Clear` ở `POST /Account/Logout`. Cookie hết hạn/không logout → `History` (kèm CvResult) sống mãi trong RAM → rò rỉ bộ nhớ theo thời gian. Đề xuất: TTL/eviction theo idle, hoặc dọn khi security-stamp thay đổi.
- **OBS-M15-02 — Không rate-limit/giới hạn chi phí Gemini theo user (Low):** spam gọi API không bị chặn → rủi ro chi phí/lạm dụng. (Đề xuất khi bật key production.)
- **OBS-M15-03 — Prompt injection từ file/nội dung người dùng (Low cho staff/self-scoped):** với self-scoped an toàn (context chỉ có dữ liệu của mình); với staff tác động thấp (đã được phép xem). Ghi nhận để không mở rộng context ngoài phạm vi.
- **OBS-M15-04 — CandidateAnalysisDialog không authz độc lập (Low):** nhận `CandidateId` từ caller, không re-check; dựa CandidateDetail đã chặn IDOR (M05 Verified). Giống các dialog khác.
- **OBS-M15-05 — Nhánh `catch` khi nạp context nuốt lỗi (Low):** `OnInitializedAsync` `catch {}` → nếu lỗi DB, staff vẫn chat nhưng thiếu ngữ cảnh mà không báo; self-scoped fallback prompt rỗng. Non-blocking.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M15_01 | Medium | TC_M15_022/023 | BF-M15-04 | `BuildDataContextAsync` không lọc AgentId cho partner | AiAssistant.razor, AgentScope.cs | agent/CTV chỉ thấy ứng viên của mình (KHÔNG lộ ứng viên đại lý khác) | **✅ Verified Fixed (code-level) — Claude phiên #7** |

> **Ghi chú:** **U-M15-1 ĐÃ CHỐT (2026-07-11):** đại lý/CTV ĐƯỢC dùng AI, AI chỉ nạp dữ liệu trong phạm vi của họ. Codex đã fix theo hướng query-level filtering (`AiDataScope`). **Claude phiên #7 đã xác minh độc lập → Verified Fixed (code-level).** M15 → `QA=Completed`, `Codex=Fixed`, `Verification=Verified`. Xem `08-verification-report.md`.
