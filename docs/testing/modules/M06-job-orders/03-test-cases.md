# M06 — Job Orders · Test Cases

> `TC_M06_<n>`. Nhiều case UI/integration → `Blocked (no harness)`; điểm có bằng chứng dòng code ghi `Pass (code)`.

| TC | Tên | Flow | Type | Priority | Sev if fail | Role | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|---|
| TC_M06_001 | List staff xem tất cả job | BF-01 | Functional | High | Med | recruiter | Thấy toàn bộ job | UI/manual | Blocked (harness) |
| TC_M06_002 | Non-staff không vào `/jobs` | BF-01 | AuthZ | High | High | collaborator/parent | Chặn (`job_orders:read`) | UI/manual (code) | Pass (code) |
| TC_M06_003 | Filter quốc gia/nhóm/tìm | BF-01 | Functional | Med | Low | staff | Lọc đúng | UI/manual | Blocked (harness) |
| TC_M06_004 | Tạo job super_admin/RM | BF-02 | Functional | High | Med | RM | Tạo thành công, Code sinh đúng | UI/manual | Blocked (harness) |
| TC_M06_005 | **Tạo job `CreatedBy`=actor** | BF-02 | Data | High | **Med** | RM (không phải user đầu) | `created_by`=actor | Integration | **FAIL → BUG_M06_01** |
| TC_M06_006 | Tạo job thiếu quốc gia | BF-02 | Validation | High | Med | RM | Chặn "nhập quốc gia" | UI/manual (code 119) | Pass (code) |
| TC_M06_007 | Tạo job role không đủ (recruiter) | BF-02 | AuthZ | High | High | recruiter | Không thấy nút; Save re-check chặn (128) | UI/manual (code) | Pass (code) |
| TC_M06_008 | Sửa job super_admin/RM | BF-03 | Functional | High | Med | RM | Cập nhật, `UpdatedAt`, giữ `CreatedBy` | UI/manual | Blocked (harness) |
| TC_M06_009 | Sửa job role không đủ | BF-03 | AuthZ | High | High | consultant | Không thấy nút; OpenEdit re-check | UI/manual (code 215) | Pass (code) |
| TC_M06_010 | Xóa job super_admin/RM | BF-04 | Functional | High | High | RM | Cascade xóa, về `/jobs` | UI/manual | Blocked (harness) |
| TC_M06_011 | Xóa job role không đủ | BF-04 | AuthZ | High | High | recruiter | `DeleteJobOrder` re-check chặn (230) | UI/manual (code) | Pass (code) |
| TC_M06_012 | Xóa job — cascade + giữ hồ sơ/khoản thu | BF-04 | DB | High | High | super_admin | Gỡ cjo/visa/vé/hoa hồng; unlink lead/payment; hồ sơ + payment còn | Integration | Blocked (harness) |
| TC_M06_013 | Xóa job không tồn tại | BF-04 | Negative | Low | Low | super_admin | "không còn tồn tại", về `/jobs` | UI/manual (code 248) | Pass (code) |
| TC_M06_014 | REST `/api/job-orders` gate quyền | BF-01 | AuthZ | High | Med | JWT staff/non-staff | staff→200 paged; non-staff→403 | Integration | Blocked (harness) |
| TC_M06_015 | Deadline ≤7 ngày tô đỏ | BF-01 | UI | Low | Low | staff | Urgent style | UI/manual (code 178) | Pass (code) |
| TC_M06_016 | Duplicate submit tạo job | BF-02 | UI | Med | Med | RM | `_saving` guard chặn double | UI/manual (code 134) | Pass (code) |
| TC_M06_017 | Đổi Status job qua dialog | BF-03 | Functional | Med | Low | RM | Status cập nhật tự do | UI/manual | Blocked (harness) |

## Gap
- Toàn bộ CRUD UI + REST integration cần harness → Blocked.
- Logic thuần M06: không có class Domain tách riêng để unit-test (form/validation nằm trong `.razor`). Backlog: tách validation + attribution ra Domain.
