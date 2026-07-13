# M08 — Training · 02 Business Flows

> QA: Claude · 2026-07-10. Nguồn: `Training.razor`, `TrainingDetail.razor`, `TrainingTrackDialog.razor`, `TrainingEvaluationDialog.razor`.

## BF-M08-01 — Xem danh sách đào tạo

- **Actor/Role:** staff có `training:read` (super_admin/director/RM/consultant) hoặc agent/collaborator (scoped).
- **Preconditions:** đăng nhập; có `training:read`.
- **Initial state:** có ≥0 ứng viên đã đặt cọc / có record / có phiếu / đang Orientation.
- **Main flow:** vào `/training` → `Load()` lấy candidates theo scope → gộp TrainingRecords + aggregate TrainingEvaluations → lọc `inTraining` (`PersonTitle.IsTitled(step) || recs>0 || evals>0 || step==Orientation`) → tính overall = TB các mảng `IsEnrolled` → hiển thị KPI + bảng, sort theo phiếu gần nhất rồi tên.
- **Alternate:** search theo tên/mã (client-side, `Filtered`). Không kết quả → empty alert.
- **Authorization:** page `[Authorize("training:read")]`; scope lọc ứng viên.
- **DB changes:** none (read). **Notification/Audit:** none.
- **Final state:** danh sách hiển thị.
- **Risk:** `jobCategories` nạp toàn bộ JobOrders (inefficiency nhẹ, không lộ dữ liệu nhạy cảm).

## BF-M08-02 — Self-scoped (Phụ huynh/Học viên) xem đào tạo của mình

- **Actor/Role:** `parent`, `student` (IsSelfScoped, `training:read`).
- **Main flow:** vào `/training` → `OnInitializedAsync` thấy `scope.IsSelfScoped` → nếu `OwnedCandidateId` có → redirect `replace` thẳng `/training/{ownedId}`; nếu null → return (không load bảng).
- **Authorization:** detail query thêm điều kiện `c.Id == scope.OwnedCandidateId` → chỉ hồ sơ của mình.
- **Error flow:** truy cập `/training/{idNgườiKhác}` → query gắn `OwnedCandidateId` → không khớp → `_found=false` → "Không tìm thấy / không có quyền".
- **Risk:** IDOR — **đóng** (scope fail-closed).

## BF-M08-03 — Agent/Collaborator xem đào tạo ứng viên trong scope

- **Actor/Role:** `agent` (IsAgentOnly) / `collaborator` (IsCollaboratorOnly), `training:read`.
- **Main flow:** list/detail query lọc `AgentId==scope.AgentId` hoặc `CollaboratorId==scope.CollaboratorId`. Chỉ xem, không có nút sửa (thiếu create/update).
- **Error flow:** ứng viên ngoài scope → không xuất hiện / `_found=false`.

## BF-M08-04 — Cập nhật tiến trình 1 mảng đào tạo

- **Actor/Role:** super_admin/RM/consultant (`training:update`).
- **Preconditions:** ở `/training/{id}`, `_canManage=true`.
- **Input:** enrolled (switch), level (text), progress (0..100), note.
- **Main flow:** bấm Edit mảng → `TrainingTrackDialog` nạp record hiện có (nếu có) → sửa → `SaveAsync`: re-check `training:update` → nếu chưa có record thì tạo mới (`CreatedBy=actorId`) else update → set `IsEnrolled/LevelLabel(trim/null)/ProgressPercent(clamp 0..100)/Note(trim/null)/UpdatedAt` → `AddAudit(create|update)` → SaveChanges → reload.
- **Alternate:** không có quyền (nút không hiện; nếu vẫn tới Save) → snackbar cảnh báo + return (không ghi).
- **Validation:** clamp 0..100; trim/null cho level/note.
- **Authorization:** re-check server-side.
- **DB changes:** insert/update `training_records` (+audit). **Notification:** none.
- **Final state:** tiến trình mảng cập nhật; overall trên list đổi theo.

### State/transition mảng đào tạo

| Current | Action | Allowed Role | Condition | Next | DB Change | Notification | History |
|---|---|---|---|---|---|---|---|
| (chưa có record) | Lưu tiến trình | SA/RM/Consultant | có `training:update` | record `IsEnrolled=switch` | insert training_records | — | audit create |
| enrolled, progress=X | Đổi progress→Y | SA/RM/Consultant | 0≤Y≤100 (clamp) | progress=Y | update | — | audit update |
| enrolled | Tắt "Có học" | SA/RM/Consultant | — | `IsEnrolled=false` (bỏ khỏi overall) | update | — | audit update |

## BF-M08-05 — Thêm phiếu đánh giá tuần (kèm minh chứng)

- **Actor/Role:** super_admin/RM/consultant (`training:create`).
- **Preconditions:** `/training/{id}`, `_canCreate=true`.
- **Input:** track (Chung/Language/Vocational), ngày (default hôm nay), 4 rating (default Good), note, 0..10 tệp (image/pdf).
- **Main flow:** "Thêm báo cáo tuần" → `TrainingEvaluationDialog` → `SaveAsync`: re-check `training:create` → upload từng tệp qua `IDocumentStorage` (lỗi 1 tệp → snackbar cảnh báo, tiếp tục) → tạo `TrainingEvaluation` (`CreatedBy=actorId`, `AttachmentsJson`= list nếu có) → `AddAudit(create)` → SaveChanges → reload → gộp vào tuần tương ứng.
- **Alternate:** không quyền → snackbar + return. Không đính kèm → `AttachmentsJson=null`.
- **Error flow:** upload lỗi → phiếu vẫn lưu (không đính kèm tệp lỗi). JSON đọc hỏng khi hiển thị → bỏ qua đính kèm (không crash).
- **Validation:** ngày null → hôm nay. Không chặn ngày tương lai (TC_M08_015 — cần user xác nhận).
- **DB changes:** insert `training_evaluations` (+audit). **Notification:** none.
- **Final state:** phiếu hiển thị trong timeline theo tuần (Monday-based, mới nhất trên đầu).

### Bảng kiểm nghiệp vụ (checklist)

| Điểm kiểm | Kết quả code |
|---|---|
| Thao tác trái quyền | Nút ẩn + re-check server-side → chặn |
| Sửa dữ liệu ứng viên ngoài scope (IDOR) | Query scoped → không tới được |
| Ghi trùng record 1 mảng | Unique `(candidate,track)` |
| Progress ngoài [0..100] | Clamp |
| Attribution sai người | `GetRequiredUserIdAsync` (actor thật) |
| Lịch sử thiếu | `AddAudit` cả 2 flow |
| Double click / duplicate submit | Nút Lưu `Disabled=_saving`; phiếu cho phép nhiều/ngày (đúng nghiệp vụ); record dùng unique index |
| Notification sai người | Không phát notification |
| 2 người sửa cùng mảng | Không rowversion → last-write-wins (OBS-M08-01) |
