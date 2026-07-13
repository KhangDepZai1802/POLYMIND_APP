# M15 — AI Assistant · Analysis

## 1. Module Overview

- **Module ID:** M15
- **Module name:** AI Assistant (Trợ lý AI Gemini)
- **Business purpose:** Trợ lý hỏi–đáp nghiệp vụ XKLĐ trên dữ liệu thật của hệ thống (xếp hạng ứng viên tiềm năng theo tiến độ 20 bước, kế hoạch, tư vấn thị trường) + trích xuất thông tin từ CV/ảnh. Với Phụ huynh/Học viên: trợ lý giới hạn chỉ hồ sơ của chính ứng viên.
- **Actor / Role:** Mọi user đã đăng nhập (`[Authorize]`, không policy). Hai chế độ: **staff** (đầy đủ) và **self-scoped** (parent/student — giới hạn hồ sơ mình).
- **Dependencies:** M02 (AgentScope data-scope), M05 (Candidate), M07 (WorkflowSteps.Progress), M10 (Payments), M11 (Loans), M08 (Training). Ngoài: Google Gemini API (key free, model `gemini-2.5-flash`).
- **Entry point:** `/ai` (AiAssistant.razor); top-bar icon (MainLayout, ẩn khi `_isAgentOnly`); sidebar + bottom-nav cho self-scoped; CandidateAnalysisDialog (phân tích 1 ứng viên).
- **Exit point:** trả lời hiển thị trong khung chat / panel trích xuất; lưu vào `AiSessionStore` (RB-5).

## 2. Source Code Map

| File | Loại | Vai trò |
|---|---|---|
| `Components/Pages/Ai/AiAssistant.razor` | page `/ai` | Chat + trích xuất CV; dựng system prompt (staff = **toàn bộ** dữ liệu; self-scoped = chỉ hồ sơ mình) |
| `Components/Pages/Ai/CandidateAnalysisDialog.razor` | dialog | Phân tích chuyên sâu 1 ứng viên (mở từ CandidateDetail) |
| `Ai/GeminiClient.cs` | service (HttpClient) | Gọi Gemini: GenerateText / Chat / ChatWithFile / ExtractFromFile; `IsConfigured`; fail-soft khi thiếu key |
| `Ai/AiSessionStore.cs` | singleton | RB-5: lưu `History` + `CvResult` theo `userId`; `Clear(userId)` khi logout |
| `Ai/GeminiOptions.cs` | options | `ApiKey`, `Model=gemini-2.5-flash` |
| `Ai/AiModels.cs` | DTO | `AiResult(Ok,Text,Error)`, `AiChatMessage` |
| `Program.cs:57-60` | DI | Configure options + HttpClient(60s) + `AddSingleton<AiSessionStore>` |
| `Program.cs:244-252` | endpoint | `/Account/Logout` gọi `aiSessions.Clear(userId)` (RB-5) |
| `Identity/AgentScope.cs` | service | Nguồn `IsSelfScoped`/`IsAgentOnly`/`OwnedCandidateId` mà AiAssistant dựa vào để chọn chế độ |
| `Components/Layout/{MainLayout,NavMenu,PortalBottomNav}.razor` | nav | Điểm vào `/ai` |

## 3. UI Inventory

- **Trang `/ai`:** header; cảnh báo "chưa cấu hình key"; tab **Hỏi đáp** (khung chat, ô nhập, nút gửi, đính kèm file, nút xóa hội thoại); tab **Trích xuất CV/ảnh** (upload zone, nút trích xuất, panel kết quả) — **ẩn khi self-scoped** (`@if (!_selfScoped)`).
- **States:** empty (chưa có hội thoại), loading ("Đang trả lời..."), busy (trích xuất), error (AiResult.Fail → alert/bubble ⚠️), file-chip.

## 4. API / External Inventory

| Gọi ra | Đích | Auth | Dữ liệu gửi |
|---|---|---|---|
| `POST generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key=…` | Google Gemini | API key (query string) | systemInstruction (**có thể chứa dữ liệu ứng viên thật**), lịch sử chat, file base64 |

- Không có REST endpoint nội bộ cho M15 (trừ logout hook RB-5). Toàn bộ chạy qua Blazor Server circuit.

## 5. Database Impact

- **Chỉ đọc** (không ghi DB): `Candidates`, `JobOrders`, `CandidateJobOrders`, `Leads`, `Payments`, `Loans`, `TrainingRecords`.
- `AiSessionStore` là **in-memory singleton** (không persist DB). Không migration cho M15.

## 6. Roles & Permissions

| Action | Điều kiện hiện tại | Nguồn |
|---|---|---|
| Vào `/ai` | Bất kỳ user đăng nhập (`[Authorize]`, **không** policy) | AiAssistant.razor:2 |
| Nhánh dữ liệu **staff** (toàn bộ ứng viên) | `!_selfScoped` = **mọi role trừ parent/student** → gồm **agent/collaborator** | AiAssistant.razor:227,237-239 + AgentScope:61 |
| Nhánh **self-scoped** (chỉ hồ sơ mình) | parent/student | AgentScope:61-62 |
| Icon AI top-bar | `!_isAgentOnly` → **CTV vẫn thấy** | MainLayout.razor:23-30 |

## 7. Risk Analysis

- **[XÁC NHẬN BUG] Data-scope bypass cho agent/collaborator:** `BuildDataContextAsync` nạp `db.Candidates`/`db.Leads`/`db.JobOrders` **không lọc `AgentId`**, trong khi mọi màn khác lọc `scope.IsAgentOnly`. Đại lý/CTV `!_selfScoped` → prompt AI chứa **toàn bộ** ứng viên (tên/giới tính/tỉnh/quốc gia/bước/tiến độ, tối đa 100) + thống kê lead/job → hỏi AI là lộ. → **BUG_M15_01**.
- **Self-scoped isolation (đúng):** self-scoped chỉ nạp `_ownedCandidateId`; tab trích xuất ẩn; prompt cấm lộ người khác. Cô lập ở **tầng dựng ngữ cảnh** (dữ liệu người khác không có trong context) chứ không chỉ ở chỉ dẫn prompt → đúng thiết kế.
- **Prompt injection:** file/nội dung người dùng có thể cố ghi đè system instruction. Với self-scoped, context chỉ có dữ liệu của mình nên không lộ người khác. Với staff (đã được phép xem) tác động thấp. Với agent/CTV, kết hợp bug trên là đường lộ dữ liệu.
- **RB-5 (đúng):** `AiSessionStore` singleton theo userId, sống qua chuyển trang/F5, `Clear` ở logout. Rủi ro phụ: chỉ xóa khi **POST /Account/Logout**; nếu cookie hết hạn không logout chủ động → state lưu mãi (memory) → OBS.
- **Cost/rate:** không giới hạn số lần gọi Gemini/user → lạm dụng chi phí. OBS.
- **Key trong query string:** `?key=…` — Gemini yêu cầu vậy; log HttpClient có thể lộ key. Không ghi vào audit/log app (chỉ log message lỗi). OBS thấp.

## 8. Unknowns / Needs Requirement Clarification

- **U-M15-1 (gắn BUG_M15_01):** Đại lý/CTV có được dùng Trợ lý AI không? Nếu **có** thì AI PHẢI lọc dữ liệu theo đúng phạm vi đại lý (như các màn khác); nếu **không** thì chặn `/ai` cho partner role. Hiện tại: CTV thấy icon + full data; agent vào được bằng URL + full data. (Liên quan U-M09-2 "ẩn dữ liệu với đối thủ đại lý".)
