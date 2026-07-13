# M03 — User & Account Management · Phân tích

## 1. Module Overview

- **Module ID:** M03
- **Module name:** User & Account Management (Quản lý tài khoản)
- **Business purpose:** Super admin tạo/sửa/khóa/mở/xóa tài khoản, gán & đổi vai trò, đặt lại mật khẩu; người dùng tự đổi mật khẩu (RB-4). Tách 2 nhóm quản lý: tài khoản nội bộ/đối tác (`/admin`) và phụ huynh/học viên (`/admin/parents-students`).
- **Actor:** super_admin (toàn quyền tài khoản); director (chỉ xem `/admin` — `users:read`); người dùng bất kỳ (tự đổi mật khẩu).
- **Role liên quan:** tạo/sửa/khóa/xóa cần `users:create`/`users:update` + `Roles=super_admin`; xem cần `users:read`.
- **Dependencies:** M01 (đăng nhập), M02 (permission `users:*`, `roles:*`). Ghi audit (M19).
- **Entry point:** `/admin` (tab Tài khoản), `/admin/parents-students`, user-menu → "Đổi mật khẩu"; chi tiết ứng viên → tạo tài khoản học viên/phụ huynh (M05).
- **Exit point:** Tài khoản được tạo/cập nhật/khóa/xóa trong DB Identity + audit log.

## 2. Source Code Map

| # | File | Symbol | Method | Mục đích | Dependency |
|---|---|---|---|---|---|
| 1 | [AccountManagerPanel.razor](../../../../src/Polymind.Web/Components/Pages/Admin/AccountManagerPanel.razor) | `AccountManagerPanel` | `CreateUserAsync`, `SaveUserRoleAsync`, `ToggleUserAsync`, `DeleteUserAsync`, `LoadUsersAsync`, `GroupedUsers` | Panel dùng chung tạo/sửa/khóa/đổi role/xóa tài khoản trong phạm vi `ManagedRoles`/`CreatableRoles` | `UserManager`, DbFactory, DialogService |
| 2 | [Admin.razor](../../../../src/Polymind.Web/Components/Pages/Admin/Admin.razor) | `Admin` | `LoadAsync`, `SaveRolePermissionsAsync`(M02), `LoadAuditAsync`(M19) | Trang `/admin` gate `users:read`; tab Tài khoản (dùng panel, `StaffRoles`/`StaffCreatableRoles`), Phân quyền (M02), Nhật ký (M19) | DbFactory, UserManager |
| 3 | [ParentStudentAccounts.razor](../../../../src/Polymind.Web/Components/Pages/Admin/ParentStudentAccounts.razor) | `ParentStudentAccounts` | — | Trang `/admin/parents-students` gate `users:read`; panel với `ManagedRoles={parent,student}`, `AllowDelete=true`, `ShowCreateForm=false` | AccountManagerPanel |
| 4 | [UserEditDialog.razor](../../../../src/Polymind.Web/Components/Pages/Admin/UserEditDialog.razor) | `UserEditDialog` | `SaveAsync` | Super admin sửa họ tên/email/mật khẩu 1 tài khoản (reset password bằng token) | `UserManager`, DbFactory |
| 5 | [ConfirmPasswordDialog.razor](../../../../src/Polymind.Web/Components/Pages/Admin/ConfirmPasswordDialog.razor) | `ConfirmPasswordDialog` | `ConfirmAsync` | Xác nhận thao tác nhạy cảm (đổi role): nhập lại mật khẩu của chính actor (`CheckPasswordAsync`) | `UserManager` |
| 6 | [ChangePasswordDialog.razor](../../../../src/Polymind.Web/Components/Shared/ChangePasswordDialog.razor) | `ChangePasswordDialog` | `SaveAsync` | RB-4: user tự đổi mật khẩu (`ChangePasswordAsync` — kiểm mật khẩu cũ, lưu hash) | `UserManager`, DbFactory |
| 7 | [DbSeeder.cs](../../../../src/Polymind.Infrastructure/Persistence/DbSeeder.cs) | `EnsureSeedUserAsync` | — | Seed 13 tài khoản demo (dev) / 1 super admin (prod) | Identity |

## 3. UI Inventory

- **Trang:** `/admin` (tab Tài khoản/Phân quyền/Nhật ký), `/admin/parents-students`.
- **Form tạo tài khoản:** Email, Họ tên, Vai trò (dropdown `_assignableRoles` = `CreatableRoles`), Mật khẩu (mặc định `Admin@123`), nút Tạo (gate `users:create`).
- **Bảng theo vai trò:** Email, Họ tên, Chuyển vai trò (dropdown hoặc chip "Cố định"), Trạng thái (Đang hoạt động/Đã khóa), Thao tác (Sửa/Xóa/Lưu role/Khóa-Mở).
- **Tìm kiếm:** ô "Tìm theo tên hoặc email" (RB-3) lọc `GroupedUsers`.
- **Dialog:** UserEditDialog (sửa), ConfirmPasswordDialog (đổi role), ChangePasswordDialog (tự đổi MK), MessageBox (xóa).
- **State:** loading (MudProgressLinear), empty ("Chưa có tài khoản nào ở vai trò này"), responsive (bảng ≥Md, card ≤Sm).

## 4. API Inventory

- **Không có REST endpoint riêng cho user management** — toàn bộ qua Blazor Server + `UserManager`. (REST `/api/auth/me` chỉ đọc thông tin token — thuộc M01.)

## 5. Database Impact

| Bảng | Thao tác |
|---|---|
| `asp_net_users` | create/update (FullName, Email, UserName, IsActive, PasswordHash, SecurityStamp), delete |
| `asp_net_user_roles` | AddToRole/RemoveFromRoles khi tạo/đổi role |
| `candidates` | `DeleteUserAsync` set `OwnerUserId=null` cho ứng viên gắn user bị xóa (**KHÔNG** xử lý `ParentUserId` — xem R1) |
| `audit_logs` | ghi `create`/`update_role`/`lock`/`unlock`/`update`/`delete`/`change_password` |

- **Ràng buộc:** `OwnerUserId`/`ParentUserId` trên `candidates` **chỉ có index, KHÔNG có FK constraint** tới `asp_net_users` (migration `AddCandidateParentUser`/`AddCandidateOwnerUser` chỉ thêm cột + index). → xóa user không lỗi FK nhưng để lại tham chiếu rác nếu không dọn tay.
- **Security stamp:** `ResetPasswordAsync`/`ChangePasswordAsync`/`RemoveFromRoles`+`AddToRole`/`SetUserNameAsync` đều đổi security stamp → revalidation (30') buộc re-auth. **`UpdateAsync` (khóa IsActive) KHÔNG đổi stamp** → xem BUG_M01_01.

## 6. Roles và Permissions

| Action | Role | UI gate | Điều kiện nghiệp vụ | Source |
|---|---|---|---|---|
| Xem `/admin` + danh sách tài khoản | super_admin, director (`users:read`) | `[Authorize(users:read)]` | — | Admin.razor:2 |
| Tạo tài khoản | super_admin (`users:create`) | `<AuthorizeView users:create>` | role ∈ CreatableRoles | AccountManagerPanel:21,276 |
| Sửa / Xóa tài khoản | super_admin (`Roles=super_admin`) | `<AuthorizeView Roles=super_admin>` | Xóa: `AllowDelete` + không phải super_admin | :117,120,389 |
| Lưu role / Khóa-Mở | super_admin (`users:update`) | `<AuthorizeView users:update>` | role không "cố định"; đổi role cần ConfirmPasswordDialog | :126,333,352 |
| Tự đổi mật khẩu | mọi user | user-menu | kiểm mật khẩu cũ | ChangePasswordDialog |

## 7. Risk Analysis (đã đối chiếu source)

1. **[XÁC NHẬN — Medium] Xóa tài khoản Phụ huynh để lại tham chiếu rác.** `DeleteUserAsync` dọn `Candidate.OwnerUserId==user.Id` (set null) nhưng **KHÔNG** dọn `Candidate.ParentUserId==user.Id`. Trang `/admin/parents-students` có `AllowDelete=true` cho cả parent/student → xóa tài khoản **phụ huynh** (gắn qua `ParentUserId`) để lại `parent_user_id` trỏ tới user đã xóa (không có FK nên không lỗi, nhưng rác dữ liệu; chi tiết ứng viên "Tài khoản đăng nhập" có thể hiển thị/thao tác sai). Không đối xứng với xử lý `OwnerUserId`. → **BUG_M03_01**.
2. **[Theo dõi — Low] Mật khẩu mặc định khi tạo = `Admin@123`** (`_newPassword = DbSeeder.DefaultAdminPassword`). Nếu super admin không đổi, tài khoản mới có mật khẩu phổ biến, dễ đoán; không ép đổi lần đầu. Rủi ro thấp (super admin chủ động, hiển thị trên form) nhưng nên cảnh báo.
3. **[Theo dõi — Low] `ConfirmPasswordDialog` dùng `CheckPasswordAsync` không lockout** → không giới hạn số lần thử mật khẩu xác nhận (khác login có `lockoutOnFailure`). Actor đã đăng nhập nên rủi ro thấp.
4. **[Cross-ref BUG_M01_01] Khóa tài khoản không đá phiên** — thao tác `ToggleUserAsync` thuộc M03 nhưng bản chất session-invalidation đã ghi ở BUG_M01_01 (không lặp lại bug ở đây; M03 tham chiếu).
5. **[OK] Reset password (admin) + đổi role đều đổi security stamp** → revalidation hoạt động. Đổi role bắt buộc `ConfirmPasswordDialog` (nhập lại MK super admin) — tốt.
6. **[OK] Enforcement server-authoritative (Blazor Server):** nút thao tác chỉ render khi có quyền; không thể kích hoạt handler từ client → không escalation qua UI. Không có REST endpoint user-management để bypass.
7. **[Theo dõi] Xóa dọn `OwnerUserId` ở DbContext riêng, SaveChanges TRƯỚC `DeleteAsync`** → nếu `DeleteAsync` fail sau đó, `OwnerUserId` đã null nhưng user còn tồn tại (bất nhất nhỏ, hiếm).

## 8. Unknowns / Cần làm rõ

- **U1:** Khi tạo tài khoản mới có nên ép đổi mật khẩu lần đầu (thay vì mặc định `Admin@123`) không? (nghiệp vụ/bảo mật).
- **U2:** Xóa tài khoản phụ huynh/học viên: nghiệp vụ mong muốn xử lý tham chiếu ứng viên thế nào (gỡ link + giữ hồ sơ ứng viên — giả định QA) — xác nhận trước khi Codex sửa BUG_M03_01.
