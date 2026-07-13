# M02 — Authorization, Roles & Permissions · Test Cases

Quy ước ID: `TC_M02_<NNN>`.

| TC | Tên | BF | Nguồn | Loại | Prio | Sev nếu fail | Role | Preconditions | Test Data | Expected | Automation | Layer | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| TC_M02_001 | Registry sinh 100 permission | BF-03 | PermissionRegistry | Functional | P1 | High | — | — | — | 20×5=100 | Unit | Unit | **Pass** |
| TC_M02_002 | Tên permission `resource:action` hợp lệ | BF-01 | PermissionRegistry | Functional | P1 | High | — | — | — | mọi name khớp | Unit | Unit | **Pass** |
| TC_M02_003 | Permission không trùng | BF-03 | PermissionRegistry | Functional | P2 | Medium | — | — | — | distinct | Unit | Unit | **Pass** |
| TC_M02_004 | Action = CRUD + approve | BF-01 | PermissionRegistry | Functional | P2 | Medium | — | — | — | đúng 5 action | Unit | Unit | **Pass** |
| TC_M02_005 | Đủ resource role map tham chiếu | BF-03 | PermissionRegistry/DbSeeder | Functional | P1 | High | — | — | — | 20 resource có mặt | Unit | Unit | **Pass** |
| TC_M02_006 | Resource không trùng | BF-03 | PermissionRegistry | Functional | P3 | Low | — | — | — | distinct | Unit | Unit | **Pass** |
| TC_M02_007 | super_admin truy cập mọi policy | BF-02 | Handler:25 | Security | P1 | Critical | super_admin | login | mọi trang | Succeed | Manual | Manual | Not Run |
| TC_M02_008 | Role có quyền X làm được X | BF-01 | Handler | Functional | P1 | High | accountant | login | `/finance` (payments:read) | render | Manual | Manual | Not Run |
| TC_M02_009 | Role thiếu quyền X bị chặn (UI) | BF-01 | Handler | Security | P1 | High | recruiter | login | `/admin` (users:read) | `/access-denied` | Manual | Manual | Not Run |
| TC_M02_010 | Vertical escalation qua URL trực tiếp | BF-01 | Handler | Security | P1 | Critical | recruiter | login | gõ URL `/admin` | chặn | Manual | Manual | Not Run |
| TC_M02_011 | API thiếu quyền → 403 | BF-05 | ApiAuth | Security | P1 | High | agent (JWT) | token | `POST`? (chỉ có GET) / policy cao | 403 | Integration | Integration | Blocked |
| TC_M02_012 | API không token → 401 | BF-05 | ApiAuth | Security | P1 | High | — | — | `/api/candidates` no token | 401 | Integration | Integration | Blocked |
| TC_M02_013 | JWT mang đúng permission claim | BF-05 | JwtTokenService | Functional | P2 | Medium | any | token | decode | có claim `permission` | Integration | Integration | Blocked |
| TC_M02_014 | Chỉnh phân quyền lưu DB + audit | BF-04 | Admin.SaveRolePermissions | Functional | P1 | High | super_admin | login | tick/bỏ + Lưu | role_permissions đổi + audit `update role_permissions` | Manual | Manual | Not Run |
| TC_M02_015 | Không chỉnh được super_admin | BF-04 | Admin:271 | Security | P1 | High | super_admin | login | chọn super_admin role | nút Lưu Disabled + cảnh báo | Manual | Manual | Not Run |
| TC_M02_016 | **Thu quyền runtime → phiên cũ mất quyền ngay** | BF-04 | Admin + Revalidate | Security | P1 | High | super_admin + nạn nhân | nạn nhân đang login | bỏ 1 quyền của role nạn nhân | nạn nhân mất quyền ≤30' | Manual | Manual | **Fail → BUG_M02_01** |
| TC_M02_017 | Seed reconcile ghi đè chỉnh tay khi restart | BF-03 | DbSeeder | Functional | P2 | Medium | — | chỉnh tay 1 role | restart app | role_permissions về đúng map code | Manual | Manual | Not Run |
| TC_M02_018 | Policy name không hợp lệ → không tạo policy | BF-01 | PolicyProvider:54 | Negative | P3 | Low | — | — | `[Authorize(Policy="foo:bar")]` | policy null → chặn | Manual | Manual | Not Run |
| TC_M02_019 | tab Phân quyền gate `roles:update` | BF-04 | Admin:38 | Security | P2 | Medium | director (không roles:update) | login | mở tab | "không có quyền" | Manual | Manual | Not Run |
| TC_M02_020 | MessagingPolicy bảng chân trị | BF-07 | MessagingPolicy | Security | P1 | High | các cặp role | — | matrix | đúng rule | Manual (logic ở Web, không ref được) | Manual | Not Run → chuyển M14 |
| TC_M02_021 | accountant có approve thu/chi/hoa hồng/vay | — | DbSeeder:84 | Requirement | P2 | — | accountant | login | duyệt thu/chi/hoa hồng/vay | Được phép approve cả 4 nhóm theo quyết định user 2026-07-10; role map `AllActions` hiện tại đúng | Source review | Static | **Pass (requirement + source confirmed)** |
| TC_M02_022 | parent/student/agent đọc API ứng viên ngoài scope (IDOR/PII) | BF-06 | ResourceEndpoints | Security | P1 | High | parent/student/agent/collab (JWT) | token | `/api/candidates` + `/{id_khác}` | **kỳ vọng chặn/lọc; hiện KHÔNG lọc scope, trả cả PassportNumber** | Integration | Integration | **Fail (source-confirmed) → BUG_M02_02; PoC runtime hoãn tới harness M05/M20** |

## Gap analysis

- **Tự động (Unit, PASS):** hợp đồng PermissionRegistry (6 TC) — nền tảng RBAC vocabulary.
- **Manual bắt buộc:** enforcement UI (super_admin bypass, chặn role thiếu quyền, escalation URL), tab Phân quyền, seed reconcile, thu-quyền-runtime (BUG_M02_01).
- **Integration blocked:** API 401/403, JWT claim, IDOR scope (TC_M02_022) — cần harness.
- **Requirement confirmed:** TC_M02_021 pass ở mức requirement + source; runtime UI vẫn có thể kiểm lại tại M09/M10/M11.
- **Chuyển module:** TC_M02_020 (MessagingPolicy) → QA sâu ở M14; TC_M02_022 (IDOR scope) → M05/M20.
- **Rủi ro còn lại:** data-scope enforcement chỉ ở UI/query (R7) — REST API chưa lọc theo đại lý.
