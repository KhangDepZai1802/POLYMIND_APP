# M03 — User & Account Management · Bug Report

Chỉ ghi bug có bằng chứng source code. Status ban đầu: `Ready for Codex`.

---

## BUG_M03_01 — Xóa tài khoản Phụ huynh để lại `Candidate.ParentUserId` rác (không dọn như OwnerUserId)

- **Bug ID:** BUG_M03_01
- **Module ID:** M03
- **Title:** `AccountManagerPanel.DeleteUserAsync` dọn `Candidate.OwnerUserId` trước khi xóa user nhưng KHÔNG dọn `Candidate.ParentUserId` → xóa tài khoản phụ huynh để lại `parent_user_id` trỏ tới user đã xóa (tham chiếu rác, bất đối xứng).
- **Severity:** Medium
- **Priority:** P1
- **Business Flow ID:** BF-M03-05
- **Test Case ID:** TC_M03_016
- **Automated Test ID:** — (cần integration; xem backlog)
- **Environment:** mọi môi trường (đặc biệt qua `/admin/parents-students` có `AllowDelete=true`)
- **Role:** super_admin (thực hiện xóa)
- **Preconditions:** Có tài khoản **phụ huynh** (role `parent`) đã gắn với 1 ứng viên qua `Candidate.ParentUserId` (tạo ở chi tiết ứng viên — `ParentAccountDialog`).
- **Test Data:** ứng viên có `parent_user_id = <parentUser.Id>`; VD hồ sơ demo UV-20260608-2001 gắn tài khoản phụ huynh.
- **Steps to Reproduce:**
  1. `/admin/parents-students` → bảng Phụ huynh → chọn tài khoản phụ huynh đã gắn ứng viên.
  2. Bấm **Xóa** → xác nhận.
  3. Kiểm `candidates.parent_user_id` của ứng viên liên quan.
- **Expected Result:** `parent_user_id` được set null (gỡ liên kết) như cách xử lý `owner_user_id`; hồ sơ ứng viên giữ nguyên, không còn trỏ tới user đã xóa.
- **Actual Result:** `parent_user_id` vẫn giữ GUID của user vừa xóa. `DeleteUserAsync` chỉ dọn `OwnerUserId`:
  ```csharp
  var owned = await db.Candidates.Where(c => c.OwnerUserId == user.Id).ToListAsync();
  foreach (var c in owned) c.OwnerUserId = null;   // KHÔNG có nhánh ParentUserId
  ```
- **UI Evidence:** `AccountManagerPanel.razor:408-414`.
- **API Evidence:** —
- **Database Evidence:** `candidates.parent_user_id` **không có FK constraint** (migration `AddCandidateParentUser` chỉ thêm cột + index) → xóa user không lỗi FK, nhưng để lại giá trị rác; `AgentScope`/UI chi tiết ứng viên ("Tài khoản đăng nhập") có thể hiển thị/thao tác sai với phụ huynh đã xóa.
- **Log Evidence:** audit `delete/users` được ghi bình thường (không phản ánh việc còn tham chiếu).
- **Suspected Source Area:** `AccountManagerPanel.DeleteUserAsync`.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor` (DeleteUserAsync)
  - `src/Polymind.Domain/Entities/Candidate.cs` (OwnerUserId/ParentUserId)
  - `src/Polymind.Web/Components/Pages/Candidates/ParentAccountDialog.razor` (nơi gắn parent — kiểm luồng gỡ)
- **Dependencies:** không chặn module khác.
- **Regression Risk:** Thấp — thêm nhánh dọn `ParentUserId` đối xứng với `OwnerUserId` (cùng transaction). Cần bảo đảm dọn cả 2 trước `DeleteAsync`.
- **Confidence Level:** Cao (source rõ ràng; bất đối xứng lộ liễu — code viết cho OwnerUserId trước khi thêm ParentUserId ở Session 60).
- **Status:** Fixed
- **Codex resolution:** Query mọi Candidate có `OwnerUserId` hoặc `ParentUserId` trùng user sắp xóa, gỡ đồng thời cả hai loại liên kết, cập nhật `UpdatedAt` rồi mới gọi `UserManager.DeleteAsync`. Hồ sơ ứng viên được giữ nguyên theo quyết định user.
- **Gợi ý hướng sửa (không bắt buộc):**
  ```csharp
  var linked = await db.Candidates
      .Where(c => c.OwnerUserId == user.Id || c.ParentUserId == user.Id).ToListAsync();
  foreach (var c in linked)
  {
      if (c.OwnerUserId == user.Id) c.OwnerUserId = null;
      if (c.ParentUserId == user.Id) c.ParentUserId = null;
  }
  if (linked.Count > 0) await db.SaveChangesAsync();
  ```

---

## Ghi chú không nâng thành bug ở M03

- **Cross-ref BUG_M01_01 (High):** khóa tài khoản (`ToggleUserAsync`) không đá phiên đang mở — hành động phát sinh ở M03 nhưng bản chất session-invalidation đã ghi ở M01. Không lặp lại; theo dõi cùng BUG_M01_01.
- **R2 (Low) mật khẩu mặc định `Admin@123`:** rủi ro cấu hình/bảo mật, super admin chủ động; đề xuất cải tiến (ép đổi lần đầu) hơn là bug. Chờ chốt U1.
- **R3 (Low) ConfirmPasswordDialog không lockout:** actor đã đăng nhập; rủi ro thấp, chưa filing.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M03_01 | Medium | TC_M03_016 | BF-M03-05 | DeleteUserAsync thiếu dọn ParentUserId | AccountManagerPanel.razor, Candidate.cs | 4 unit regression + TC_M03_015/016 runtime | **Verified Fixed** (Claude 2026-07-10) |
