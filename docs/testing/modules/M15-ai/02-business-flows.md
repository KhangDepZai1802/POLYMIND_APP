# M15 — AI Assistant · Business Flows

## BF-M15-01 — Chat nghiệp vụ (staff)
- **Actor/Role:** staff (super_admin, RM, recruiter, consultant, document, visa, accountant, director).
- **Precondition:** đăng nhập; Gemini key cấu hình.
- **Main:** mở `/ai` → `OnInitialized` nạp `AiSessionStore` (RB-5) + dựng `_systemPrompt = ChatSystemBase + toàn bộ dữ liệu ứng viên/lead/job` → user nhập câu hỏi → `ChatAsync(history, systemPrompt)` → hiển thị + lưu History.
- **Auth:** `[Authorize]` (không policy). **Không lọc theo role trong dựng dữ liệu.**
- **DB:** chỉ đọc. **Notification/History:** không.
- **Risk:** nhánh dữ liệu áp cho **mọi** `!_selfScoped` (gồm agent/CTV) → BF-M15-04 (bug).

## BF-M15-02 — Chat giới hạn (self-scoped: parent/student)
- **Actor:** parent/student (self-scoped).
- **Main:** mở `/ai` → `_selfScoped=true` → `BuildSelfDataContextAsync` nạp **chỉ** `OwnedCandidateId` → `_systemPrompt = RestrictedSystemBase + HỒ SƠ CỦA BẠN` → chat.
- **Isolation:** tab "Trích xuất CV" ẩn; context chỉ chứa dữ liệu của mình → không thể lộ người khác kể cả prompt-injection.
- **Verdict:** đúng thiết kế (cô lập ở tầng dữ liệu).

## BF-M15-03 — Trích xuất CV/ảnh (staff)
- **Main:** upload PDF/ảnh ≤8MB → `ExtractFromFileAsync(bytes, mime, CvPrompt)` → hiển thị trường trích xuất; lưu `CvResult` vào session (RB-5).
- **Validate:** size ≤ 8MB; mime whitelist (image/*, pdf, txt, csv cho chat); Word/Excel → yêu cầu xuất PDF.
- **Verdict:** đúng; tab ẩn với self-scoped.

## BF-M15-04 — [DEFECT] Agent/CTV nhận toàn bộ dữ liệu ứng viên
- **Actor:** agent-only / collaborator-only.
- **Trigger:** CTV thấy icon AI (MainLayout `!_isAgentOnly`) → bấm; hoặc agent gõ URL `/ai`.
- **Hiện tại:** `_selfScoped=false` (agent/CTV không phải parent/student) → `BuildDataContextAsync` nạp **toàn bộ** `Candidates`/`Leads`/`JobOrders` **không lọc AgentId** → prompt AI liệt kê tên/tỉnh/tiến độ mọi ứng viên (kể cả của đại lý khác).
- **Kỳ vọng:** hoặc AI lọc đúng phạm vi đại lý (như DebtCollection/LoanDialog), hoặc chặn `/ai` cho partner.
- **→ BUG_M15_01** (cần U-M15-1).

## BF-M15-05 — RB-5 vòng đời phiên AI
| State | Action | Kết quả |
|---|---|---|
| Có hội thoại | Chuyển trang / F5 | `AiSessionStore.Get(userId)` giữ nguyên History + CvResult |
| Có hội thoại | Xóa hội thoại (nút) | `_history.Clear()` (cùng list trong store) |
| Bất kỳ | POST `/Account/Logout` | `aiSessions.Clear(userId)` → mất sạch |
| Cookie hết hạn (không logout chủ động) | — | State còn trong memory (OBS-M15-01) |

## BF-M15-06 — Phân tích 1 ứng viên (CandidateAnalysisDialog)
- Mở từ CandidateDetail (đã scope theo M05) với `CandidateId` → dựng hồ sơ 1 người → `GenerateTextAsync`.
- Dialog **không** authz độc lập (tin caller) — giống các dialog khác; CandidateDetail đã chặn IDOR (M05 Verified). OBS-M15-04.
