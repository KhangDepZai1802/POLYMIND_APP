# M05 — Candidate Management · Test Cases

> Quy ước `TC_M05_<n>`. Actual/Status: điền khi chạy. Nhiều case là **runtime/UI** → hiện `Blocked (no harness)`; case logic thuần scope đã phủ ở M02 (`CandidateAccessScope`). Không sửa expected để pass.

| TC | Tên | Flow | Type | Priority | Sev if fail | Role | Steps (tóm tắt) | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| TC_M05_001 | List staff thấy tất cả | BF-01 | Functional | High | High | staff | `/candidates` | Thấy toàn bộ hồ sơ trong quyền | UI/manual | Blocked (harness) |
| TC_M05_002 | List agent chỉ AgentId | BF-01 | AuthZ/scope | High | High | agent | `/candidates` | Chỉ hồ sơ `AgentId==scope` | Unit (CandidateAccessScope @M02) | **Pass (proxy)** |
| TC_M05_003 | List collaborator chỉ CollaboratorId | BF-01 | AuthZ/scope | High | High | collaborator | `/candidates` | Chỉ hồ sơ `CollaboratorId==scope` | Unit (@M02) | **Pass (proxy)** |
| TC_M05_004 | Parent/student redirect hồ sơ mình | BF-01/02 | Functional | High | Med | parent/student | login → `/candidates` | Redirect `/candidates/{ownedId}` | UI/manual | Blocked (harness) |
| TC_M05_005 | Detail IDOR: agent xem hồ sơ ngoài scope | BF-02 | AuthZ/IDOR | High | High | agent | `/candidates/{id_khác}` | `_accessDenied`, không lộ dữ liệu | UI/manual (code verified) | **Pass (code)** |
| TC_M05_006 | Detail IDOR: parent xem hồ sơ khác | BF-02 | AuthZ/IDOR | High | High | parent | `/candidates/{id_khác}` | Access denied | UI/manual (code verified) | **Pass (code)** |
| TC_M05_007 | Detail IDOR: collaborator xem hồ sơ khác | BF-02 | AuthZ/IDOR | High | High | collaborator | `/candidates/{id_khác}` | Access denied | UI/manual (code verified) | **Pass (code)** |
| TC_M05_008 | REST `/api/candidates` áp scope (IDOR) | BF-02 | AuthZ/IDOR | High | High | student/parent/collab | `GET /api/candidates` (JWT) | Chỉ hồ sơ trong scope (BUG_M02_02) | Unit (@M02) + runtime | **Pass (code @M02)**; runtime pending |
| TC_M05_009 | Tạo hồ sơ có `candidates:create` | BF-03 | Functional | High | Med | recruiter | Thêm ứng viên | Tạo thành công, `CreatedBy`=actor | UI/manual | Blocked (harness) |
| TC_M05_010 | Tạo hồ sơ không có quyền | BF-03 | AuthZ | High | High | collaborator | không thấy nút Thêm | Không tạo được | UI/manual (code) | **Pass (code)** |
| TC_M05_011 | Sửa hồ sơ role hợp lệ | BF-04 | Functional | High | Med | consultant | Sửa → lưu | Cập nhật, `UpdatedAt` | UI/manual | Blocked (harness) |
| TC_M05_012 | Sửa hồ sơ role không hợp lệ (accountant) | BF-04 | AuthZ | High | High | accountant | không thấy nút Sửa; `Save` re-check | Không sửa được | UI/manual (code 1394) | **Pass (code)** |
| TC_M05_013 | Xóa hồ sơ super_admin/doc_staff | BF-05 | Functional | High | High | doc_staff | Xóa → confirm | Cascade xóa, về `/candidates` | UI/manual | Blocked (harness) |
| TC_M05_014 | Xóa hồ sơ role không hợp lệ (recruiter) | BF-05 | AuthZ | High | High | recruiter | `DeleteCandidate` re-check (1409) | Chặn "không có quyền" | UI/manual (code) | **Pass (code)** |
| TC_M05_015 | Xóa cascade dữ liệu liên quan | BF-05 | DB | High | High | super_admin | Xóa hồ sơ có loan/payment/visa/cjo | Không orphan các bảng liệt kê | Integration | Blocked (harness) |
| TC_M05_016 | RB-1 ẩn 2 dòng CTV với parent | BF-06 | AuthZ/PII | High | Med | parent | Mở CollaboratorInfoDialog | Ẩn "đã giới thiệu" + "% hoa hồng"; hiện liên lạc | UI/manual (code 47-51,78) | **Pass (code)** |
| TC_M05_017 | RB-1 ẩn 2 dòng CTV với student | BF-06 | AuthZ/PII | High | Med | student | Mở dialog | Như trên | UI/manual (code) | **Pass (code)** |
| TC_M05_018 | RB-1 staff vẫn thấy 2 dòng | BF-06 | Functional | Med | Low | recruiter | Mở dialog | Hiện đủ 2 dòng | UI/manual (code) | **Pass (code)** |
| TC_M05_019 | RB-2 đổi TVV/CTV super_admin | BF-07 | Functional | High | High | super_admin | Đổi → nhập mật khẩu | Cập nhật + audit | UI/manual | Blocked (harness) |
| TC_M05_020 | RB-2 đổi TVV/CTV non-super_admin | BF-07 | AuthZ | High | High | RM | card không hiện; `ChangeAssignees` re-check (1572) | Không đổi được | UI/manual (code) | **Pass (code)** |
| TC_M05_021 | RB-2 sai mật khẩu khi đổi | BF-07 | AuthZ | High | Med | super_admin | Nhập sai mật khẩu | Hủy thao tác, không đổi | UI/manual | Blocked (harness) |
| TC_M05_022 | RB-2 đổi đơn hàng reset workflow | BF-08 | Functional | High | High | super_admin | Đổi job → mật khẩu | Gắn job mới, reset 20 bước | UI/manual | Blocked (harness) |
| TC_M05_023 | RB-2 đổi đơn hàng non-super_admin | BF-08 | AuthZ | High | High | RM | `ChangeJobOrder` re-check (1606) | Không đổi được | UI/manual (code) | **Pass (code)** |
| TC_M05_024 | Gắn tài khoản Học viên | BF-09 | Functional | Med | Med | super_admin | Tạo tài khoản học viên | `OwnerUserId` set, user tạo | UI/manual | Blocked (harness) |
| TC_M05_025 | Gỡ & khóa tài khoản (stamp) | BF-09 | Security | High | High | super_admin | Gỡ liên kết & khóa | `IsActive=false` + stamp đổi (BUG_M01_01) | UI/manual (code verified @M01) | **Pass (code)** |
| TC_M05_026 | Xóa user còn link → cleanup | BF-09 | DB | Med | Med | super_admin | Xóa parent/student user | `parent/owner_user_id`=null (BUG_M03_01) | Unit (@M03) | **Pass (code @M03)** |
| TC_M05_027 | Collaborator bị mask SĐT ứng viên | BF-02 | PII | Med | Low | collaborator | Xem detail | `MaskPhone` áp SĐT | UI/manual (code 1083) | **Pass (code)** |
| TC_M05_028 | CTV xem passport ứng viên | BF-02 | PII | Med | Low | collaborator | Xem detail | **CTV ĐƯỢC xem passport/CCCD (user chốt 2026-07-10)** — hành vi đúng | UI/manual (code) | **Pass (spec confirmed)** |
| TC_M05_029 | Duplicate submit tạo hồ sơ | BF-03 | UI | Med | Med | recruiter | Double click "Lưu" | Không tạo trùng | UI/manual | Blocked (harness) |
| TC_M05_030 | Convert race 2 request | BF-03 | Concurrency | Low | Med | 2 staff | Convert cùng lead | Không tạo 2 Candidate (R7) | Integration | Blocked (harness) |

## Nhóm bảo mật (chạy khi có harness local/test)
- IDOR REST `/api/candidates/{id}` per-record (BUG_M02_02 detail) — runtime.
- Mass assignment `CandidateDialog` (field nhạy cảm `CreatedBy`/`OwnerUserId` có bị set từ client?) — cần đọc dialog binding (obs cho session sau).
- XSS lưu trữ ở field họ tên/ghi chú hiển thị — runtime.

## Gap
- Toàn bộ luồng UI (tạo/sửa/xóa thực, RB-2 password) cần **harness bUnit/Playwright + DB test** → chưa có → `Blocked`.
- Logic thuần đã unit-test: `CandidateAccessScope` (@M02, 5 case), `CandidateAccountLinkRules` (@M03), `LeadConversionRules` (@M04). `BusinessRoleAccess`/`AgentScope` nằm ở `Polymind.Web` → chưa test được (không ref Web từ test project) — backlog: tách sang Domain/Application.
