# M14 — Messaging / Chat · Business Flows

> Nguồn: `Messages.razor` (`LoadContacts`/`BuildAllowedRecipientsAsync`/`LoadThread`/`Send`/`RecallMessage`), `MessagingPolicy`, `MinioDocumentStorage`.

---

## BF-M14-01 — Mở trang & dựng danh bạ

- **Actor:** user có `messages:read`.
- **Main flow:** `OnInitializedAsync`→`LoadContacts`: resolve `_meId` (`GetRequiredUserIdAsync`), roles map, `_allowedRecipientIds` (self-scoped) → duyệt users active ≠ me, giữ theo self-scoped set / `MessagingPolicy.CanMessage`, tính last + unread; sắp xếp unread→last→tên.
- **Authorization:** policy `messages:read`; danh bạ lọc theo quyền.
- **Risk:** scoping bất đối xứng (OBS-M14-01).

## BF-M14-02 — Chọn người & xem hội thoại (mark read)

- **Main flow:** `SelectContact`→`LoadThread`: nạp tin me↔other theo `CreatedAt`; đánh dấu đã đọc tin gửi cho me (set `IsRead/ReadAt`); resolve attachment presigned URL (try/catch → null nếu lỗi); reload danh bạ để cập nhật badge.
- **Authorization:** chỉ nạp tin **me là participant** → không leak.
- **Risk:** không phân trang (OBS-M14-04); attachment URL lỗi → ẩn ảnh, vẫn hiện tên file.

## BF-M14-03 — Gửi tin (text/đính kèm)

- **Preconditions:** đã chọn người; có text hoặc file; `_sending==false`.
- **Main flow:** `Send`: **re-check quyền server** (self-scoped: `allowed.Contains(recipient)`; nội bộ: `MessagingPolicy.CanMessage(_myRoles, recipientRoles)`) → nếu có file: `UploadMessageAttachmentAsync` (validate size+ext) → thêm Message (Body JSON) → reload thread + danh bạ.
- **Validation:** empty text + no file → không gửi; file quá lớn/định dạng sai → `InvalidOperationException` → snackbar.
- **DB:** thêm 1 Message (`IsRead=false`).
- **Error flow:** không quyền → snackbar cảnh báo, không ghi; lỗi upload → snackbar lỗi.
- **Risk:** không re-check recipient IsActive (OBS-M14-03).

## BF-M14-04 — Thu hồi tin (recall)

- **Main flow:** `RecallMessage(id)`: tìm Message `Id==id && SenderId==me && (RecipientId==other||me)` → **xóa cứng** → reload.
- **Authorization:** chỉ tác giả (`SenderId==me`).
- **DB:** xóa Message (không audit — OBS-M14-02).
- **Risk:** attachment MinIO không bị xóa theo (orphan object) — obs.

## BF-M14-05 — Self-scoped (Phụ huynh/Học viên) giới hạn quan hệ

- **Actor:** parent/student (`AgentScope.IsSelfScoped`).
- **Main flow:** `BuildAllowedRecipientsAsync`: từ `OwnedCandidateId` lấy Candidate → allowed = CTV(`Collaborator.UserId`) + TVV(`ConsultantId`=user) + (parent→con `OwnerUserId`) / (student→phụ huynh `ParentUserId`); bỏ chính mình.
- **Hệ quả:** danh bạ + Send đều chặn ngoài allowed set.
- **Edge:** không có ứng viên/quan hệ → allowed rỗng → không nhắn ai (đúng).
- **Xác nhận:** parent/student **không thấy super admin** (không nằm allowed set) — khớp WORKLOG.

## BF-M14-06 — Đính kèm & tải file

- **Main flow:** chọn file (InputFile accept ảnh/pdf/office) → chip preview → Send upload → thread hiển thị ảnh inline/audio player/file; `DownloadAttachment` mở presigned URL (forceLoad).
- **Validation server:** size ≤ MaxUploadBytes, extension ∈ whitelist (pdf/jpg/png/webp/doc/xls/webm/ogg/mp3/m4a/wav).
- **Risk:** presigned URL không kiểm ownership object (an toàn trong M14 vì objectKey từ thread của mình; note M18 — OBS-M14-05).

---

## Kiểm tra state/duplicate/security

| Kiểm | Kết quả |
|---|---|
| Trạng thái không tới được | Message chỉ IsRead false→true (một chiều), ReadAt set. Không state phức tạp. |
| Đọc chéo user (IDOR) | Đóng — LoadThread scoped me↔other |
| Gửi trái quyền qua UI | Đóng — Send re-check server 2 nhánh |
| Thu hồi tin người khác | Đóng — SenderId==me |
| Upload file độc hại | Đóng — whitelist ext + size server |
| XSS body/filename | Đóng — Blazor auto-encode |
| Double submit | Đóng — `_sending` guard |
| Gửi cho user bị khóa | Hở nhẹ — Send không re-check IsActive (OBS-M14-03) |
| Scope staff→parent/student | **Hở (cần chốt)** — staff nhắn mọi parent/student (OBS-M14-01) |
