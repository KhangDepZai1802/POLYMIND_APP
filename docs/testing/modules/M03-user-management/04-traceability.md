# M03 — User & Account Management · Traceability

| Business Flow | Page/Component | Điểm phân quyền | Role | State/Action | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M03-01 Tạo tài khoản | AccountManagerPanel (form) | `users:create` | super_admin | create | TC_M03_001,002,003,004,024 | — | Manual only | Chưa automate (UI+UserManager) |
| BF-M03-02 Đổi vai trò | AccountManagerPanel + ConfirmPasswordDialog | `users:update` + xác nhận MK | super_admin | update_role | TC_M03_005,006,007,008,009,010 | — | Manual | Revalidate cần integration |
| BF-M03-03 Khóa/Mở | AccountManagerPanel | `users:update` | super_admin | lock/unlock | TC_M03_011,012 | — | Manual/Integration | TC_012 → BUG_M01_01 |
| BF-M03-04 Sửa/Reset MK | UserEditDialog | `Roles=super_admin` | super_admin | update | TC_M03_013,014 | — | Manual | — |
| BF-M03-05 Xóa | AccountManagerPanel (AllowDelete) | `Roles=super_admin` | super_admin | delete | TC_M03_015,016,017 | — | Manual/Integration | TC_016 → BUG_M03_01 |
| BF-M03-06 Tự đổi MK | ChangePasswordDialog | authenticated | mọi user | change_password | TC_M03_018,019,020 | — | Manual | — |
| (phân quyền trang) | Admin / ParentStudentAccounts | `users:read` | super_admin/director | read | TC_M03_021,022,023 | — | Manual | Escalation cần integration |

## Tổng hợp coverage
- **Business flows:** 6/6 có test case.
- **Test cases:** 24; **Automated:** 0 (lý do ở automation report); **Manual:** 22; **Integration-blocked:** 2 (TC_012, TC_016) + vài case revalidate.
- **Bug phát hiện:** BUG_M03_01 (Medium). Cross-ref BUG_M01_01 (khóa-đá-phiên phát sinh từ thao tác M03).
- **Gap chính:** không có automated test cho M03 ở session này (UI-heavy). Đề xuất harness bUnit cho component + integration DB để phủ TC_012/016 và revalidate.
