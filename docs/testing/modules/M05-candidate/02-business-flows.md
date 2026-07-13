# M05 — Candidate Management · Business Flows

> Nguồn: đọc `Candidates.razor`, `CandidateDetail.razor`, `CollaboratorInfoDialog.razor`, `BusinessRoleAccess.cs`, `AgentScope`. Không suy đoán; điểm chưa rõ ghi `Needs Requirement Clarification`.

---

## BF-M05-01 — Xem danh sách ứng viên theo phạm vi

- **Actor/Role:** staff (đầy đủ) · agent/collaborator (scoped) · parent/student (self)
- **Preconditions:** đăng nhập, có `candidates:read`.
- **Main flow:** `/candidates` → `Load` resolve `AgentScope` → query lọc theo scope → render list.
- **Authorization:** `candidates:read` + AgentScope (agent→AgentId, collaborator→CollaboratorId, self→OwnedCandidateId).
- **Alternate:** parent/student → `OnInitializedAsync` redirect thẳng `/candidates/{ownedId}`.
- **Error:** không quyền → không vào được trang (`[Authorize]`).
- **Final state:** chỉ thấy hồ sơ trong phạm vi.
- **Risk:** data-scope sai (đối chiếu §3.1 — đúng). Perf client-filter (R6).

## BF-M05-02 — Xem chi tiết ứng viên (IDOR-by-URL)

- **Actor/Role:** như trên.
- **Main flow:** `/candidates/{id}` → `Load` nạp candidate → **kiểm scope**; hợp lệ → hiển thị.
- **Alternate/Error:** out-of-scope (agent/collaborator/self không sở hữu) → `_accessDenied=true`, `_candidate=null` → UI "không có quyền".
- **Authorization:** AgentScope (dòng 1074-1082).
- **Final state:** chỉ chủ sở hữu/scope xem được.
- **Risk:** IDOR (R2 — chặn). REST API tách rời (BUG_M02_02).

## BF-M05-03 — Tạo hồ sơ ứng viên

- **Actor/Role:** super_admin/RM/recruiter/consultant (có `candidates:create`).
- **Main flow:** nút "Thêm" (gate `candidates:create`) → `CandidateDialog` → validate → lưu. Hoặc Convert từ Lead (BF-M04-05, `CreatedBy=actor`).
- **Validation:** field bắt buộc ở `CandidateDialog` (họ tên, mã…).
- **DB:** insert `candidates` (`CreatedBy`=actor).
- **Risk:** duplicate submit; convert race (R7).

## BF-M05-04 — Sửa hồ sơ ứng viên

- **Actor/Role:** `candidates:update` **và** `CanEditCandidateProfile` (super_admin/RM/recruiter/consultant/doc_staff).
- **Main flow:** nút "Sửa" (gate `_canEditCandidateProfile`) → `CandidateDialog` (edit) → lưu; `Save` re-check `_canEditCandidateProfile` (dòng 1394).
- **DB:** update `candidates`, `UpdatedAt`.
- **Authorization:** permission + role; agent/collaborator/parent/student KHÔNG có → không thấy nút, không gọi được handler.
- **Risk:** stale update; lost update (2 người sửa — no concurrency token → last-write-wins, obs).

## BF-M05-05 — Xóa hồ sơ ứng viên (manual cascade)

- **Actor/Role:** `candidates:delete` **và** `CanDeleteCandidate` (super_admin/doc_staff).
- **Main flow:** nút "Xóa" → confirm → `DeleteCandidate` **re-check quyền** (dòng 1409) → cascade xóa cjo/workflow records/documents/versions/loans/payments/receipts/visas/commissions → delete candidate → điều hướng `/candidates`.
- **DB:** xóa nhiều bảng trong **một DbContext**.
- **Risk:** sót bảng liên quan nếu thêm quan hệ mới (R5). Không FK cascade DB.

## BF-M05-06 — RB-1: ẩn thông tin nhạy cảm CTV với Phụ huynh/Học sinh

- **Actor/Role:** parent/student xem hồ sơ mình, bấm card CTV → `CollaboratorInfoDialog`.
- **Main flow:** dialog `OnInitialized` → `_hideSensitive = IsParent || IsStudent` → ẩn 2 dòng "Ứng viên đã giới thiệu" + "Tỷ lệ hoa hồng CTV %"; giữ tên/đại lý/SĐT/email/địa chỉ/trạng thái.
- **Authorization:** dựa `AgentScope.IsParent/IsStudent`.
- **Risk:** lộ commission/số ứng viên (R3 — chặn đúng).

## BF-M05-07 — RB-2: đổi Tư vấn viên / Cộng tác viên (chỉ super_admin + mật khẩu)

- **Actor/Role:** **super_admin only**.
- **Preconditions:** đăng nhập super_admin; hồ sơ tồn tại.
- **Main flow:** card "Đổi người phụ trách" (chỉ hiện `_isSuperAdmin`) → chọn TVV/CTV → "Lưu" → `ChangeAssigneesAsync` re-check `_isSuperAdmin` (dòng 1572) → `ConfirmWithPasswordAsync` (nhập lại mật khẩu chính mình) → cập nhật + audit.
- **Authorization:** `_isSuperAdmin` + password confirm.
- **DB:** update `consultant_id`/`collaborator_id`, audit.
- **Risk:** non-super_admin đổi (R4 — chặn). Ảnh hưởng hoa hồng/nhắc việc (cross M09/M13).

## BF-M05-08 — RB-2: đổi đơn hàng đã gắn (reset workflow, super_admin + mật khẩu)

- **Actor/Role:** **super_admin only**.
- **Main flow:** card đơn hàng (`_isSuperAdmin`) → chọn đơn hàng khác → `ChangeJobOrderAsync` re-check `_isSuperAdmin` (dòng 1606) → confirm password → gắn đơn mới + **reset tiến trình 20 bước** (1 ứng viên = 1 job active).
- **DB:** cập nhật `candidate_job_orders`, tạo record mới; audit.
- **Risk:** reset workflow có hoàn tiền/hoa hồng? → **U2 ĐÃ CHỐT (user 2026-07-10): KHÔNG hoàn → hành vi đúng, không bug**; verify chéo M09/M10.

## BF-M05-09 — Gắn/gỡ tài khoản cổng (Học viên/Phụ huynh)

- **Actor/Role:** `users:create` (`_canManageStudentAccount`).
- **Main flow:** card "Tài khoản đăng nhập" → "Tạo/Quản lý" → `Student/ParentAccountDialog` → tạo user (`OwnerUserId`/`ParentUserId`) hoặc **gỡ liên kết & khóa** (`IsActive=false` + `UpdateSecurityStampAsync` — verify BUG_M01_01).
- **DB:** set/clear `owner_user_id`/`parent_user_id`; xóa user → cleanup link (BUG_M03_01).
- **Risk:** khóa không đá phiên (BUG_M01_01 — fixed); xóa user để rác link (BUG_M03_01 — fixed).

---

### State/masking matrix (scope × dữ liệu nhạy cảm)

| Scope | List | Detail (mình) | Detail (khác) | SĐT UV | RB-1 (hoa hồng/#UV CTV) | Sửa/Xóa | RB-2 |
|---|---|---|---|---|---|---|---|
| staff (đủ quyền) | tất cả | ✔ | ✔ | hiện | hiện | theo role | super_admin |
| agent | AgentId | ✔ | **chặn** | hiện | hiện | không | không |
| collaborator | CollaboratorId | ✔ | **chặn** | **mask** | hiện | không | không |
| parent/student | OwnedCandidateId | ✔ (redirect) | **chặn** | hiện | **ẩn** | không | không |
