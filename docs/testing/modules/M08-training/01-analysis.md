# M08 — Training · 01 Analysis

> QA: Claude · Ngày: 2026-07-10 · Không sửa business logic (chỉ đọc source + viết tài liệu QA).

## 1. Module Overview

- **Module ID:** M08
- **Module name:** Training (Đào tạo — 2 mảng: Học tiếng / Chuyên môn) + Phiếu đánh giá tuần
- **Business purpose:** Theo dõi tiến trình đào tạo của ứng viên đã đặt cọc theo **2 mảng tách biệt** (Language / Vocational — góp ý Vietgroup) và ghi **phiếu đánh giá định kỳ theo tuần** trên 4 tiêu chí (Chuyên cần / Chuyên môn / Kỷ luật / Tài chính) kèm minh chứng (ảnh/PDF) để Phụ huynh, Đại lý và đối tác Nhật theo dõi.
- **Actor / Role:**
  - Quản lý đào tạo (ghi tiến trình + phiếu): `super_admin`, `recruitment_manager`, `consultant` (đủ `training:create/update`).
  - Chỉ xem: `director`, `agent`, `collaborator`, `parent`, `student` (`training:read`).
  - Không có quyền training: `recruiter`, `document_staff`, `visa_staff`, `accountant` (→ không thấy menu/thẻ Đào tạo). Xem OBS-M08-02.
- **Dependencies:** M02 (RBAC — policy `training:*`), M05 (Candidate — `TrainingRecord.CandidateId`/`TrainingEvaluation.CandidateId` trỏ Candidate; scope qua `AgentScope`/`CandidateAccessScope` pattern), M18 (Documents/MinIO — lưu minh chứng phiếu đánh giá). Gián tiếp M07 (bước B10 Orientation trong workflow chỉ ghi *notes*, KHÔNG tạo TrainingRecord — record/phiếu tạo độc lập ở module này).
- **Entry point:** `/training` (danh sách) · `/training/{id}` (chi tiết ứng viên) · thẻ "Đào tạo" trên `/candidates/{id}` (chỉ hiện khi `training:read` + ứng viên đã đặt cọc `PersonTitle.IsTitled`).
- **Exit point:** Lưu TrainingRecord (mỗi mảng) / TrainingEvaluation (mỗi phiếu tuần) + audit; điều hướng về `/candidates/{id}`.

## 2. Source Code Map

| # | File | Loại | Method/Thành phần | Mục đích | Dependency |
|---|---|---|---|---|---|
| 1 | `src/Polymind.Web/Components/Pages/Training/Training.razor` | Page `/training` | `OnInitializedAsync`, `Load`, `Filtered`, `TitleChip`, `TrackCell` | Danh sách học viên đang đào tạo + KPI (đang đào tạo / tiến độ TB / hoàn tất / có phiếu). Self-scoped redirect thẳng chi tiết. | `IDbContextFactory`, `AgentScope`, `PersonTitle`, `Labels` |
| 2 | `src/Polymind.Web/Components/Pages/Training/TrainingDetail.razor` | Page `/training/{Id}` | `OnInitializedAsync`, `Load`, `WeekStart`, `OpenTrackEdit`, `OpenAddEvaluation` | Chi tiết: 2 mảng tiến trình + timeline phiếu đánh giá gộp theo tuần; nút cập nhật/ thêm phiếu theo quyền. | `IAuthorizationService`, `AuthenticationStateProvider`, `AgentScope`, `IDocumentStorage`, `IDialogService` |
| 3 | `src/Polymind.Web/Components/Pages/Training/TrainingTrackDialog.razor` | Dialog | `OnInitializedAsync`, `SaveAsync` | Cập nhật 1 mảng đào tạo (enrolled/level/progress/note). Re-check `training:update` server-side + clamp 0..100 + audit. | `IDbContextFactory`, `AuthenticationStateProvider`, `IAuthorizationService` |
| 4 | `src/Polymind.Web/Components/Pages/Training/TrainingEvaluationDialog.razor` | Dialog | `OnFilesSelected`, `RatingSelect`, `SaveAsync`, `record AttachmentInfo` | Tạo phiếu đánh giá tuần (track?/ngày/4 rating/note/đính kèm). Re-check `training:create` server-side + upload MinIO + audit. | `IDocumentStorage`, `AuthenticationStateProvider`, `IAuthorizationService` |
| 5 | `src/Polymind.Domain/Entities/TrainingRecord.cs` | Entity | — | `CandidateId, Track, IsEnrolled(=true), LevelLabel?, ProgressPercent(0..100), Note?, CreatedBy` | `BaseEntity`, `TrainingTrack` |
| 6 | `src/Polymind.Domain/Entities/TrainingEvaluation.cs` | Entity | — | `CandidateId, Track?, EvaluationDate, Attendance/Professional/Discipline/Financial(EvaluationRating), Note?, AttachmentsJson?, CreatedBy` | `BaseEntity`, `TrainingTrack`, `EvaluationRating` |
| 7 | `src/Polymind.Domain/Enums/Enums.cs` | Enum | `TrainingTrack {Language, Vocational}`, `EvaluationRating {Weak, Average, Good, Excellent}` | Từ vựng mảng + thang đánh giá | — |
| 8 | `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs:168-177` | DbContext | `TrainingRecords`, `TrainingEvaluations` DbSet + config | Unique index `(CandidateId, Track)`; index `CandidateId`, `EvaluationDate` | EF Core |
| 9 | `src/Polymind.Infrastructure/Persistence/DbSeeder.cs:37-112` | RBAC seed | `RolePermissionMap` | Map role→training perms (RM/Consultant = Crud; Director/Agent/Collab/Parent/Student = read; SuperAdmin = all) | `PermissionRegistry` |
| 10 | `src/Polymind.Infrastructure/Persistence/Migrations/20260706081025_AddTrainingAndLoanRepayment.cs` | Migration | — | Tạo bảng `training_records`, `training_evaluations` + index | EF Core |
| 11 | `src/Polymind.Web/Display/PersonTitle.cs` | Helper | `IsTitled/IsStudent/Of` | Danh xưng + gate "đang đào tạo" (đã đặt cọc B5+) | `WorkflowStep`, `JobCategory` |
| 12 | `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor:614-660,1300-1339` | Consumer | `LoadTrainingAsync` | Thẻ tóm tắt đào tạo trên hồ sơ ứng viên (read-only, link sang `/training/{id}`) | `training:read`, `PersonTitle` |

**Không có:** REST endpoint riêng cho training (chỉ Blazor Server components); background job; notification handler (RB-7 không thêm nhóm training).

## 3. UI Inventory

- **`/training` (list):** search (tên/mã, Immediate), 4 KPI cards, bảng desktop (Mã/Họ tên/Danh xưng/Học tiếng/Chuyên môn/Tiến độ chung/Phiếu gần nhất) + card mobile, empty state (`MudAlert`), loading (`MudProgressLinear`), progress bars mỗi mảng + chung, chip danh xưng. Row click → `/training/{id}`.
- **`/training/{id}` (detail):** header (tên/mã/chip danh xưng, nút "Hồ sơ ứng viên"); cột trái 2 mảng tiến trình (progress + level + nút Edit nếu `_canManage`); nút "Thêm báo cáo tuần" nếu `_canCreate`; cột phải timeline phiếu theo tuần (chip 4 tiêu chí màu theo rating, note, link đính kèm), empty state; not-found state (`MudAlert.Warning` + nút về list).
- **TrainingTrackDialog:** switch "Có học mảng này", TextField cấp độ, NumericField tiến độ (Min0 Max100, disabled khi !enrolled), TextField ghi chú, nút Hủy/Lưu (Lưu disabled khi `_saving`).
- **TrainingEvaluationDialog:** Select mảng (Chung/Language/Vocational, Clearable), DatePicker ngày, 4 Select rating, TextField nhận xét, InputFile (image/pdf, multiple ≤10), nút Hủy/Lưu phiếu.

## 4. API Inventory

Module KHÔNG có REST endpoint. Mọi thao tác qua Blazor Server render + `IDbContextFactory` trực tiếp. Gate:

| Thao tác | Điểm kiểm quyền UI | Kiểm quyền server (defense-in-depth) | Side effect DB | Notification |
|---|---|---|---|---|
| Xem list/detail | `[Authorize(Policy="training:read")]` trên page | Query lọc `AgentScope` (self/agent/collaborator/staff) | — | — |
| Cập nhật tiến trình mảng | Nút Edit chỉ hiện khi `training:update` | `TrainingTrackDialog.SaveAsync` re-check `training:update`, nếu fail → snackbar + return | Insert/Update `training_records` (unique candidate+track) + audit `create/update` | — |
| Thêm phiếu tuần | Nút chỉ hiện khi `training:create` | `TrainingEvaluationDialog.SaveAsync` re-check `training:create` | Upload MinIO (0..10 tệp) + Insert `training_evaluations` + audit `create` | — |

## 5. Database Impact

- **`training_records`**: PK `id`; **unique `(candidate_id, track)`** → mỗi ứng viên tối đa 1 record/mảng; cột `is_enrolled`, `level_label?`, `progress_percent`, `note?`, `created_by`, audit fields (`created_at/updated_at` từ `BaseEntity`). KHÔNG có FK ràng buộc cứng tới `candidates` (candidate_id là Guid thô — theo pattern chung của repo). Không cascade.
- **`training_evaluations`**: PK `id`; index `candidate_id`, `evaluation_date`; cột `track?`, 4 rating, `note?`, `attachments_json?` (JSON list object key MinIO), `created_by`, audit fields. Không unique → cho phép nhiều phiếu/ngày/ứng viên (đúng nghiệp vụ "báo cáo tuần").
- **State field:** không có state-machine; `progress_percent` 0..100 (clamp ở save). `is_enrolled=false` = "đơn hàng không yêu cầu học mảng này".
- **Audit:** cả 2 dialog gọi `db.AddAudit(actorId, action, "training", entityId, null, newValueSnapshot)`.
- **Concurrency:** không có rowversion/concurrency token → xem OBS-M08-01.

## 6. Roles và Permissions

| Action | Role được phép | UI Permission | API/Server Permission | Business Condition | Source |
|---|---|---|---|---|---|
| Xem đào tạo | super_admin, director, recruitment_manager, consultant, agent, collaborator, parent, student | `training:read` (page `[Authorize]`) | Query lọc `AgentScope` (agent→ứng viên của agent; collaborator→ứng viên mình giới thiệu; parent/student→ứng viên của mình; staff→tất cả) | Ứng viên nằm trong scope | `DbSeeder.cs:37-112`, `Training.razor:180-183`, `TrainingDetail.razor:174-177` |
| Cập nhật tiến trình | super_admin, recruitment_manager, consultant | Nút Edit ẩn nếu thiếu `training:update` | `TrainingTrackDialog.SaveAsync` re-check | — | `TrainingTrackDialog.razor:64` |
| Thêm phiếu đánh giá | super_admin, recruitment_manager, consultant | Nút ẩn nếu thiếu `training:create` | `TrainingEvaluationDialog.SaveAsync` re-check | — | `TrainingEvaluationDialog.razor:78` |
| Xóa record/phiếu | *(không có UI)* | — | — | `training:delete` được seed cho RM/Consultant/SuperAdmin nhưng KHÔNG dùng → OBS-M08-03 | `DbSeeder.cs:51,67` |

## 7. Risk Analysis

| Rủi ro | Đánh giá | Kết luận |
|---|---|---|
| Broken authorization (đọc/ghi vượt quyền) | Page có `[Authorize]`; dialog re-check server-side create/update. Read-only role không thấy nút. | **Đóng** ở code. |
| IDOR (đổi `{id}` xem/sửa ứng viên ngoài scope) | Query detail lọc `AgentScope`; agent/collaborator/self không thấy ngoài scope → `_found=false`, không render nút. Dialog nhận `CandidateId` từ page đã scoped; Blazor Server không cho gọi dialog trực tiếp. | **Đóng** ở code (giống pattern M05/M07 đã verify). |
| Attribution sai (first-user anti-pattern như BUG_M04/M06) | Cả 2 dialog dùng `actorId = await AuthStateProvider.GetRequiredUserIdAsync(db)` cho `CreatedBy`. **KHÔNG** dùng `Users.FirstOrDefault`. | **Đúng** — không dính anti-pattern. |
| Progress ngoài [0..100] | `Math.Clamp(_progress,0,100)` ở `SaveAsync` (không chỉ dựa Min/Max UI). | **Đóng**. |
| Ghi trùng record 1 mảng | Save đọc `FirstOrDefault` rồi update/insert; unique index `(candidate,track)` là chốt cuối. | Đúng tuần tự; race → OBS-M08-01. |
| Lost update / 2 người sửa cùng mảng | Không rowversion → ghi đè im lặng (last-write-wins). | **OBS-M08-01** (Low, cùng lớp OBS-M07-01). |
| JSON đính kèm hỏng | Deserialize bọc `try/catch` → bỏ qua an toàn (không crash). | **Đóng**. |
| Notification sai người / badge | Module không phát notification. | Không áp dụng. |
| History/audit thiếu | Cả 2 save đều `AddAudit`. | **Đóng**. |
| Timezone `EvaluationDate` | Dùng `DateOnly` (không giờ) → không lệch timezone. `WeekStart` tính Monday-based. | **Đóng**. |
| Ngày đánh giá tương lai | Không chặn ngày tương lai → phiếu tương lai lên đầu timeline. | Quan sát nhẹ (TC_M08_015); nghiệp vụ nhập "tuần này" nên chấp nhận — cần user xác nhận nếu muốn chặn. |
| Upload file độc hại/quá lớn | Uỷ thác `IDocumentStorage` (accept image/pdf ở UI; MIME thật kiểm ở M18). | Chuyển rủi ro sang **M18**. |

## 8. Unknowns / Needs Requirement Clarification

- **U-M08-1 (OBS-M08-02):** `recruiter`, `document_staff`, `visa_staff`, `accountant` KHÔNG có `training:read` → không xem được đào tạo. Recruiter tạo/quản ứng viên nhưng không thấy đào tạo. **Cần user xác nhận** đây là thiết kế (đào tạo do RM + Consultant phụ trách) hay thiếu quyền. **Non-blocking** — mặc định coi là đúng thiết kế cho tới khi user chốt.
- **U-M08-2:** Có cần chặn `EvaluationDate` ở tương lai không? (Hiện không chặn.) Non-blocking.
- **U-M08-3:** Có cần chức năng **xóa/sửa phiếu đánh giá** đã tạo không? Hiện phiếu là immutable (không UI xóa/sửa) dù `training:delete` được seed. Non-blocking (immutable phù hợp audit).
