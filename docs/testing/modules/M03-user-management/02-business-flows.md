# M03 — User & Account Management · Business Flows

## BF-M03-01 — Tạo tài khoản (super admin)
- **Actor/Role:** super_admin (`users:create`). **Preconditions:** đăng nhập, ở `/admin` tab Tài khoản. **Initial state:** email chưa tồn tại.
- **Input:** Email, Họ tên, Vai trò (∈ CreatableRoles), Mật khẩu (mặc định `Admin@123`).
- **Main flow:** `CreateUserAsync` → validate email/họ tên + role ∈ `CreatableSet` → `UserManager.CreateAsync(user, pwd)` (áp password policy) → `AddToRoleAsync(role)` → audit `create` → reload.
- **Alternate/Error:** email/họ tên rỗng → warning; role ngoài CreatableSet → "Vai trò không hợp lệ với trang này"; email trùng/pwd yếu → `CreateAsync` fail → hiển thị lỗi Identity.
- **Validation:** email format (Identity), password ≥8 digit/upper/lower, unique email. **Authorization:** `users:create`.
- **DB changes:** `asp_net_users` + `asp_net_user_roles`. **Notification:** không. **Audit:** `create/users`.
- **Final state:** tài khoản active, có role.
- **Risk:** mật khẩu mặc định phổ biến (R2). **Source:** AccountManagerPanel:267-305.

## BF-M03-02 — Đổi vai trò (super admin, có xác nhận mật khẩu)
- **Main flow:** chọn role mới → `SaveUserRoleAsync` → chặn nếu role "cố định" (super_admin / RolesFixed / ngoài CreatableSet) → chặn nếu ngoài CreatableSet → nếu role không đổi → info → mở `ConfirmPasswordDialog` (nhập lại MK super admin, `CheckPasswordAsync`) → `RemoveFromRolesAsync(old)` + `AddToRoleAsync(new)` → audit `update_role` → reload.
- **Authorization:** `users:update` + xác nhận danh tính. **Security stamp:** đổi (AddToRole/RemoveFromRole) → nạn nhân bị revalidate ≤30' (nạp claim mới). **Đúng.**
- **Kiểm:** không đổi được super_admin (chip Cố định); parent↔student đổi qua lại được ở `/admin/parents-students` (không RolesFixed); Đại lý/CTV ở `/admin` là "Cố định" (ngoài CreatableSet).
- **Source:** AccountManagerPanel:328-367.

## BF-M03-03 — Khóa / Mở tài khoản
- **Main flow:** `ToggleUserAsync` → đảo `IsActive` → `UserManager.UpdateAsync` → audit `lock`/`unlock` → reload.
- **DB changes:** `is_active`. **Security stamp:** **KHÔNG đổi** → phiên đang mở của nạn nhân KHÔNG bị đá (chỉ chặn đăng nhập MỚI). → khiếm khuyết đã ghi ở **BUG_M01_01** (M03 là điểm phát sinh hành động).
- **Source:** AccountManagerPanel:369-384.

## BF-M03-04 — Sửa tài khoản (họ tên / email / reset mật khẩu)
- **Main flow:** `UserEditDialog.SaveAsync` → validate → `UpdateAsync(FullName)` → nếu email đổi: `SetEmailAsync` + `SetUserNameAsync` → nếu có mật khẩu mới: `GeneratePasswordResetTokenAsync` + `ResetPasswordAsync` → audit `update` (kèm `PasswordChanged`).
- **Security stamp:** đổi email/mật khẩu đều đổi stamp → nạn nhân re-auth ≤30'. **Đúng.**
- **Error:** email trùng, mật khẩu yếu → lỗi Identity hiển thị.
- **Source:** UserEditDialog:63-134.

## BF-M03-05 — Xóa tài khoản (chỉ super admin, chỉ khi AllowDelete)
- **Preconditions:** `AllowDelete=true` (chỉ trang `/admin/parents-students`), không phải super_admin.
- **Main flow:** `DeleteUserAsync` → MessageBox xác nhận → dọn `Candidate.OwnerUserId==user.Id` (set null, SaveChanges) → `UserManager.DeleteAsync` → audit `delete` → reload.
- **LỖI:** KHÔNG dọn `Candidate.ParentUserId==user.Id` → xóa **phụ huynh** để lại `parent_user_id` rác. → **BUG_M03_01**.
- **Source:** AccountManagerPanel:387-426.

## BF-M03-06 — Tự đổi mật khẩu (RB-4)
- **Main flow:** user-menu → ChangePasswordDialog → validate (cũ≠mới, nhập lại khớp) → `ChangePasswordAsync(current,new)` (kiểm mật khẩu cũ + policy) → audit `change_password` (KHÔNG lưu giá trị) → đóng.
- **Security stamp:** đổi → các phiên khác của user revalidate ≤30'. **Đúng, không lưu plaintext.**
- **Source:** ChangePasswordDialog:45-94.

## Bảng trạng thái tài khoản

| Current State | Action | Allowed Role | Condition | Next State | DB Change | Notification | History |
|---|---|---|---|---|---|---|---|
| (chưa có) | Tạo | super_admin | role∈CreatableSet, email unique | Active | users + user_roles | — | audit create |
| Active | Đổi role | super_admin | không cố định + xác nhận MK | Active (role mới) | user_roles, security_stamp | — | audit update_role |
| Active | Khóa | super_admin | — | Locked (login mới bị chặn) — **phiên đang mở VẪN chạy (BUG_M01_01)** | is_active | — | audit lock |
| Locked | Mở | super_admin | — | Active | is_active | — | audit unlock |
| Active | Reset MK (admin) | super_admin | — | Active (MK mới) | password_hash, security_stamp | — | audit update |
| Active | Xóa (parent) | super_admin | AllowDelete | Deleted — **candidate.parent_user_id còn rác (BUG_M03_01)** | users xóa; owner_user_id dọn, parent_user_id KHÔNG | — | audit delete |
| Active | Tự đổi MK | chính chủ | biết MK cũ | Active (MK mới) | password_hash, security_stamp | — | audit change_password |

**Kiểm tra vấn đề điển hình:**
- Thao tác trái quyền: director xem `/admin` nhưng nút tạo/sửa/khóa ẩn (thiếu users:create/update + không super_admin) → không escalation.
- Xóa khi đang tham chiếu: xóa parent để lại candidate.parent_user_id rác (BUG_M03_01); xóa student dọn owner_user_id đúng.
- Double click: nút Lưu/Xóa có `Disabled` khi `_saving`/`_checking` (UserEditDialog/ConfirmPasswordDialog) — panel tạo/khóa không disable rõ (rủi ro nhẹ double submit tạo trùng — email unique chặn trùng).
- Notification/badge: N/A.
- Lịch sử: mọi thao tác tài khoản đều có audit (tốt).
