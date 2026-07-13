# M14 — Messaging / Chat · Analysis

> QA phiên #5 (Claude). Đọc source thật: `Messages.razor`, `MessagingPolicy.cs`, `Message` entity, `MinioDocumentStorage` (attachment), DbContext config, DbSeeder (permission `Messaging()`).

## 1. Module Overview

- **Module ID:** M14
- **Module name:** Messaging / Chat nội bộ (1-1)
- **Business purpose:** Nhắn tin 1-1 giữa các tài khoản (nhân sự nội bộ + đại lý/CTV + phụ huynh/học viên), có đính kèm file/ảnh/ghi âm, đánh dấu đã đọc, thu hồi tin của mình. Phân quyền "ai nhắn được cho ai" theo role (`MessagingPolicy`) + giới hạn quan hệ cho self-scoped (phụ huynh/học viên).
- **Actor:** mọi user có `messages:read`/`messages:create` (tất cả role đều có `Messaging()`).
- **Dependencies:** M02 Authorization (**Verified**) — policy `messages:read`; AgentScope (self-scoped resolve); M18 Documents (MinIO attachment). Không phụ thuộc M13.
- **Entry point:** `/messages`.
- **Exit point:** Message lưu DB; attachment lưu MinIO; đọc/thu hồi cập nhật DB.

## 2. Source Code Map

| File | Symbol | Mục đích | Dependency |
|---|---|---|---|
| `src/Polymind.Web/Components/Pages/Messages/Messages.razor` | trang `/messages` `[Authorize(Policy="messages:read")]` | Danh bạ + hội thoại + soạn/gửi/thu hồi/đính kèm | DbFactory, AuthStateProvider, AgentScope, DocumentStorage |
| " | `LoadContacts()` | Dựng danh bạ: self-scoped→`_allowedRecipientIds`; nội bộ→`MessagingPolicy.CanMessage`; tính last/unread | MessagingPolicy, AgentScope |
| " | `BuildAllowedRecipientsAsync(db)` | Self-scoped: tập user được nhắn (CTV+TVV+con/phụ huynh) từ Candidate FK | AgentScope, Candidate/Collaborator |
| " | `LoadThread()` | Nạp hội thoại me↔other + mark-read tin gửi cho me + resolve attachment URL | DocumentStorage |
| " | `Send()` | **Server re-check quyền** (self-scoped allowed set / MessagingPolicy) → upload attachment → thêm Message | MessagingPolicy, DocumentStorage |
| " | `RecallMessage(id)` | Xóa cứng tin **của chính mình** (`SenderId==me`) | — |
| " | `ParseMessageBody`/`SerializeMessageBody` | Body = JSON `polymind-message-v1` {Text, Attachment}; tin cũ hiển thị nguyên văn | System.Text.Json |
| `src/Polymind.Web/Identity/MessagingPolicy.cs` | `CanMessage(senderRoles, recipientRoles)` | Ma trận role: super=ai cũng nhắn; director chỉ super; agent/CTV chỉ nhận từ recruitment roles; còn lại nội bộ=cho phép | RoleNames |
| " | `PrimaryRoleLabel(roles)` | Nhãn vai trò chính hiển thị | — |
| `src/Polymind.Domain/Entities/Message.cs` | entity | SenderId/RecipientId/Body/IsRead/ReadAt | BaseEntity |
| `src/Polymind.Web/Storage/MinioDocumentStorage.cs` | `UploadMessageAttachmentAsync`/`UploadMessageAudioAsync`/`GetDownloadUrlAsync` | Upload + presigned URL; **validate size + extension whitelist server-side** | Minio, MinioStorageOptions |
| `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs:136-140` | index | `(RecipientId,IsRead)` + `(SenderId,RecipientId,CreatedAt)`; **không FK tới Users** | — |
| `src/Polymind.Infrastructure/Persistence/DbSeeder.cs:115` | `Messaging()` | `["messages:read","messages:create"]` — mọi role đều có | — |

## 3. UI Inventory

- **Danh bạ (md=4):** search theo tên (client, `FilteredContacts`), MudList; avatar + badge unread + role label + preview tin cuối. Empty state "Không có ai bạn được phép nhắn tin."
- **Khung hội thoại (md=8):** header người nhận; chat thread (bubble mine/theirs), attachment (ảnh inline/audio player/file + tải), nút thu hồi (chỉ tin mình); ô soạn + đính kèm (InputFile accept ảnh/pdf/office) + gửi (Enter gửi, Shift+Enter xuống dòng); chip file đã chọn.
- **Loading/empty:** "Chọn một người…"; "Chưa có tin nhắn…".

## 4. API Inventory

- **Không có REST endpoint** cho messages. Mọi thao tác qua Blazor Server circuit → server-side (`DbFactory`).
- Attachment tải qua **presigned URL** MinIO (`GetDownloadUrlAsync`, hết hạn theo `PresignedUrlExpirySeconds`).

## 5. Database Impact

| Entity | Table | Constraint | Ghi chú |
|---|---|---|---|
| Message | messages | index `(RecipientId,IsRead)`, `(SenderId,RecipientId,CreatedAt)`; **KHÔNG FK** tới AspNetUsers | SenderId/RecipientId là Guid trần → xóa user để lại orphan message (OBS-M14-05) |
| (attachment) | MinIO `messages/{sender}/{recipient}/{file}` | — | không phải DB |

- `CreatedAt`/`ReadAt` = DateTimeOffset UTC.
- Body lưu JSON (versioned `polymind-message-v1`); parse an toàn (try/catch → hiển thị nguyên văn tin cũ).

## 6. Roles & Permissions

| Action | Ai | Điều kiện | Source |
|---|---|---|---|
| Mở `/messages` | mọi role (có `messages:read`) | policy | Messages.razor:2, DbSeeder:115 |
| Thấy 1 người trong danh bạ | tùy | self-scoped→quan hệ ứng viên; nội bộ→`MessagingPolicy.CanMessage(my,their)` | LoadContacts:226-230 |
| Gửi tin | sender | **server re-check**: self-scoped allowed set / MessagingPolicy | Send:363-383 |
| Đọc hội thoại | người tham gia | `LoadThread` chỉ nạp tin me↔other (me là sender/recipient) → **không leak chéo** | LoadThread:300-304 |
| Thu hồi | chỉ tác giả | `SenderId==me` (xóa cứng) | RecallMessage:432-435 |
| Đính kèm | sender | size ≤ MaxUploadBytes + extension whitelist (server) | UploadObjectAsync:96-105 |

### Ma trận MessagingPolicy (recipient-first)

| Recipient role | Ai được nhắn tới |
|---|---|
| SuperAdmin | tất cả |
| Director | chỉ SuperAdmin |
| Agent / Collaborator | SuperAdmin, Director, RecruitmentManager, Recruiter |
| Khác (nội bộ: RM/Recruiter/Consultant/Document/Visa/Accountant, **parent/student**) | tất cả (branch "nội bộ" trả true) |

## 7. Risk Analysis

- **IDOR đọc hội thoại:** đóng — `LoadThread` scoped me↔other; `SelectContact` chỉ nhận id từ danh bạ đã render (Blazor Server giữ list server-side). Không thấy tin của người khác.
- **Broken authz gửi:** đóng — Send **re-check server** cả 2 nhánh (self-scoped/MessagingPolicy); không tin UI.
- **Recall người khác:** đóng — chỉ xóa `SenderId==me`.
- **File upload:** đóng — validate size + extension server-side (whitelist pdf/ảnh/office/audio); tên file `SanitizeFileName`.
- **XSS:** đóng — Text/FileName render qua Blazor auto-encode; ảnh/audio src từ presigned URL.
- **⚠️ Scoping bất đối xứng (OBS-M14-01, req U-M14-1):** self-scoping chỉ áp phía **phụ huynh/học viên**; phía nhân sự (kể cả **CTV/Đại lý**) dùng MessagingPolicy → (a) CTV/Đại lý/nhân sự nội bộ có thể **nhắn tới BẤT KỲ phụ huynh/học viên** toàn hệ thống (không giới hạn ứng viên phụ trách); (b) Đại lý/CTV có thể **khởi tạo** tin tới nhân sự nội bộ (kế toán/visa/hồ sơ) — những người này lại KHÔNG được nhắn ngược lại → thread một chiều. Cần user chốt phạm vi.
- **Concurrency/duplicate:** double-send chặn bởi `_sending`; Enter gửi 1 lần. Không unique message → gửi trùng nội dung được (đúng nghiệp vụ chat).
- **Recipient IsActive:** danh bạ lọc `IsActive` nhưng Send không re-check → circuit cũ có thể gửi cho user vừa bị khóa (OBS-M14-03).
- **Perf:** `LoadThread`/`LoadContacts` nạp **toàn bộ** message (không phân trang) — OBS-M14-04.
- **Orphan/audit:** Message không FK Users (orphan khi xóa user); Recall xóa cứng không audit (OBS-M14-02/05).

## 8. Unknowns / Needs Requirement Clarification

- **U-M14-1 (OBS-M14-01):** (a) CTV/Đại lý/nhân sự có nên bị **giới hạn** chỉ nhắn phụ huynh/học viên **của ứng viên mình phụ trách** (đối xứng với self-scoped), hay được nhắn mọi phụ huynh/học viên? (b) Đại lý/CTV có được **khởi tạo** tin tới kế toán/visa/hồ sơ không (hiện được, nhưng chiều ngược bị chặn → thread một chiều)?
