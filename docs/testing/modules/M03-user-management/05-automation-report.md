# M03 — User & Account Management · Automation Report

## Framework & dependency
- **Test framework:** xUnit 2.9.2 (project `tests/Polymind.Tests`), như M01/M02.
- **Ràng buộc:** M03 gần như 100% là Blazor component (`AccountManagerPanel`, `UserEditDialog`, `ConfirmPasswordDialog`, `ChangePasswordDialog`, `Admin`, `ParentStudentAccounts`) + `UserManager<ApplicationUser>` trên DB Identity thật. Không có lớp logic thuần tách khỏi UI/DB để unit test.

## Automated Test IDs → Test Case
- **Không có test tự động mới ở session này.** Toàn bộ 24 test case M03 là **Manual** hoặc **Integration (blocked)**.

## Lý do chưa tự động được
1. **Logic nhánh nằm trong Razor component (Polymind.Web):** `ManagedSet`/`CreatableSet`/`IsRoleFixed`/`GroupedUsers` là `private` trong `AccountManagerPanel.razor`. Test project **không** tham chiếu `Polymind.Web` (build sẽ rebuild Web → khóa DLL khi dev server `:5177` đang chạy — `MSB3021`). Kể cả có ref, các thành viên private cần bUnit render component.
2. **Hành vi chính cần DB Identity + UserManager:** tạo/đổi role/khóa/xóa/đổi MK phải chạy trên `UserManager` + Postgres → cần integration harness (WebApplicationFactory + DB test riêng), chưa dựng (tránh side effect lên DB dev đang là DB thật của app).
3. **Kiểm khóa-đá-phiên & revalidate** (TC_M03_012, 010, 014) cần mô phỏng 2 phiên + security stamp → integration/E2E.

## Lệnh chạy (suite chung)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Hiện có: 11 pass (M01 config 4 + M02 PermissionRegistry 6 + smoke 1). M03 chưa thêm test.
```

## Kết quả
- **Pass:** 0 (không có test M03) · **Fail:** 0 · **Skipped:** 0 · **Blocked:** 2 integration (TC_012, TC_016) + phần lớn manual.
- **Environment issue:** dev server `:5177` khóa DLL `Polymind.Web` → không ref Web từ test.

## Automation backlog (đề xuất)
1. **bUnit** cho `AccountManagerPanel`: render với `ManagedRoles`/`CreatableRoles` khác nhau, assert dropdown role, chip "Cố định", ẩn/hiện nút theo policy (giả lập `AuthorizationContext`).
2. **Integration (WebApplicationFactory + DB test):**
   - Tạo user → đăng nhập → super admin khóa → assert phiên bị vô hiệu (regression BUG_M01_01).
   - Tạo parent + gắn `Candidate.ParentUserId` → xóa parent → assert `parent_user_id` được set null (regression BUG_M03_01).
   - Đổi role → assert security stamp đổi + claim mới.
3. Tách logic role-filter (CreatableSet/IsRoleFixed) ra helper thuần (Infrastructure/Domain) để unit test không cần Web.
