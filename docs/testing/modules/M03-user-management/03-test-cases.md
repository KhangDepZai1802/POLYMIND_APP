# M03 — User & Account Management · Test Cases

Quy ước ID: `TC_M03_<NNN>`. Automation Layer: **Manual** (UI + UserManager qua DB), **Integration** (bUnit/WebApplicationFactory — chưa có harness). M03 hầu như không có logic thuần tách khỏi Blazor/UserManager → chưa tự động được ở session này (xem `05-automation-report.md`).

| TC | Tên | BF | Nguồn | Loại | Prio | Sev nếu fail | Role | Preconditions | Test Data | Expected | Automation | Layer | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| TC_M03_001 | Tạo tài khoản hợp lệ | BF-01 | AccountManagerPanel:267 | Functional | P1 | High | super_admin | tab Tài khoản | email mới, role staff | tạo OK + audit create | Manual | Manual | Not Run |
| TC_M03_002 | Tạo email trùng bị chặn | BF-01 | UserManager | Negative | P2 | Medium | super_admin | email đã tồn tại | trùng | lỗi Identity | Manual | Manual | Not Run |
| TC_M03_003 | Tạo mật khẩu yếu bị chặn | BF-01 | DI password policy | Boundary | P2 | Medium | super_admin | — | "abc" | lỗi policy | Manual | Manual | Not Run |
| TC_M03_004 | Tạo role ngoài CreatableSet bị chặn | BF-01 | :276 | Security | P1 | High | super_admin | `/admin` chọn agent | agent | "Vai trò không hợp lệ" | Manual | Manual | Not Run |
| TC_M03_005 | Đổi role có xác nhận mật khẩu | BF-02 | :352 | Functional | P1 | High | super_admin | user staff | đổi role + nhập MK | ConfirmPasswordDialog → đổi + audit update_role | Manual | Manual | Not Run |
| TC_M03_006 | Xác nhận mật khẩu sai → không đổi | BF-02 | ConfirmPasswordDialog:51 | Security | P1 | High | super_admin | — | MK sai | "Mật khẩu không đúng", role giữ nguyên | Manual | Manual | Not Run |
| TC_M03_007 | Không đổi role super_admin | BF-02 | :318,333 | Security | P1 | High | super_admin | dòng super_admin | thử đổi | chip "Cố định", không đổi | Manual | Manual | Not Run |
| TC_M03_008 | Đại lý/CTV "Cố định" ở /admin | BF-02 | :319 | Security | P2 | Medium | super_admin | `/admin` | dòng agent | chip Cố định (ngoài CreatableSet) | Manual | Manual | Not Run |
| TC_M03_009 | parent↔student đổi qua lại ở P&S | BF-02 | ParentStudentAccounts | Functional | P2 | Medium | super_admin | `/admin/parents-students` | parent→student | đổi OK (xác nhận MK) | Manual | Manual | Not Run |
| TC_M03_010 | Đổi role → nạn nhân revalidate ≤30' | BF-02 | Revalidate | Security | P2 | Medium | super_admin | nạn nhân đang login | đổi role | claim mới sau ≤30' | Manual | Manual | Not Run |
| TC_M03_011 | Khóa tài khoản chặn đăng nhập MỚI | BF-03 | :369 + Login | Security | P1 | High | super_admin | user active | khóa → user thử login | "Tài khoản đang bị khóa" | Manual | Manual | Not Run |
| TC_M03_012 | Khóa KHÔNG đá phiên đang mở | BF-03 | ToggleUser + Revalidate | Security | P1 | High | super_admin | user đang login tab khác | khóa | **kỳ vọng đá ≤30'; thực tế vẫn chạy tới 8h** | Manual/Integration | Integration | **Fail → BUG_M01_01 (cross-ref)** |
| TC_M03_013 | Sửa họ tên/email | BF-04 | UserEditDialog:63 | Functional | P2 | Medium | super_admin | user | đổi tên+email | cập nhật OK + audit update | Manual | Manual | Not Run |
| TC_M03_014 | Reset mật khẩu (admin) đổi stamp | BF-04 | :112 | Security | P1 | High | super_admin | user đang login | reset MK | user re-auth ≤30' | Manual | Manual | Not Run |
| TC_M03_015 | Xóa tài khoản học viên dọn OwnerUserId | BF-05 | :411 | Functional | P2 | Medium | super_admin | student gắn ứng viên | xóa | user xóa, candidate.owner_user_id=null | Manual | Manual | Not Run |
| TC_M03_016 | **Xóa tài khoản phụ huynh để lại ParentUserId rác** | BF-05 | :408-414 | Database | P1 | Medium | super_admin | parent gắn ứng viên (parent_user_id) | xóa | **kỳ vọng parent_user_id=null; thực tế còn rác** | Manual/Integration | Integration | **Fail → BUG_M03_01** |
| TC_M03_017 | Không xóa được super_admin | BF-05 | :389 | Security | P1 | High | super_admin | dòng super_admin | — | không có nút Xóa / chặn | Manual | Manual | Not Run |
| TC_M03_018 | Tự đổi mật khẩu (RB-4) | BF-06 | ChangePasswordDialog:75 | Functional | P1 | High | mọi user | login | MK cũ đúng + mới hợp lệ | đổi OK, không lưu plaintext, audit change_password | Manual | Manual | Not Run |
| TC_M03_019 | Tự đổi MK: sai mật khẩu cũ | BF-06 | :75 | Negative | P2 | Medium | mọi user | — | MK cũ sai | lỗi, không đổi | Manual | Manual | Not Run |
| TC_M03_020 | Tự đổi MK: nhập lại không khớp | BF-06 | :53 | Boundary | P3 | Low | mọi user | — | new≠confirm | "nhập lại không khớp" | Manual | Manual | Not Run |
| TC_M03_021 | director xem /admin read-only | BF- | Admin:2 | Security | P1 | High | director | login | mở `/admin` | xem được; nút tạo/sửa/khóa ẩn | Manual | Manual | Not Run |
| TC_M03_022 | recruiter bị chặn /admin | BF- | Admin:2 | Security | P1 | Critical | recruiter | login | gõ URL `/admin` | `/access-denied` | Manual | Manual | Not Run |
| TC_M03_023 | RB-3 tìm theo tên/email | BF- | :455 | Functional | P3 | Low | super_admin | nhiều user | gõ tên | lọc đúng | Manual | Manual | Not Run |
| TC_M03_024 | Mật khẩu mặc định tạo = Admin@123 | BF-01 | :219 | Security | P3 | Low | super_admin | tạo không đổi pwd | — | tài khoản dùng Admin@123 (rủi ro R2) | Manual | Manual | Not Run |

## Gap analysis
- **Tự động (session này):** 0 — M03 là UI Blazor + UserManager (cần DB Identity + render component). Logic phân nhánh (ManagedSet/CreatableSet/IsRoleFixed) là private trong component Web → không ref/unit test được khi dev server khóa DLL Web.
- **Manual bắt buộc:** toàn bộ CRUD tài khoản, đổi role + xác nhận MK, khóa/mở, reset/đổi MK, phân quyền xem trang.
- **Integration (backlog):** TC_M03_012 (khóa-đá-phiên → BUG_M01_01), TC_M03_016 (xóa parent dọn FK → BUG_M03_01), TC_M03_010/014 (revalidate stamp) — cần harness bUnit/WebApplicationFactory + DB test.
- **Rủi ro còn lại:** mật khẩu mặc định Admin@123; ConfirmPasswordDialog không lockout.
