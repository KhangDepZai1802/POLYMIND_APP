# M06 — Job Orders · Traceability

| Business Flow | Page / API | Role | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M06-01 List/detail | `JobOrders.razor`, `JobOrderDetail.razor`, `GET /api/job-orders` | staff | TC_001–003, 014, 015 | — | AuthZ gate ✔ (code) | List/REST runtime |
| BF-M06-02 Tạo | `JobOrderDialog.Save` | super_admin/RM | TC_004–007, 016 | — | AuthZ+validation ✔ (code); **attribution ✗** | **BUG_M06_01** + create UI |
| BF-M06-03 Sửa | `JobOrderDialog.Save` edit | super_admin/RM | TC_008–009, 017 | — | AuthZ ✔ (code) | Edit UI + lost-update |
| BF-M06-04 Xóa | `DeleteJobOrder` | super_admin/RM | TC_010–013 | — | AuthZ re-check ✔ (code) | Cascade integrity runtime |

## Coverage summary
- **Verified (code review):** AuthZ gate list/create/edit/delete (permission + `BusinessRoleAccess` role, re-check server-side); validation Country; duplicate-submit guard; deadline urgent style; REST gate.
- **Confirmed defect:** BUG_M06_01 — `CreatedBy` = user đầu tiên thay vì actor (create path).
- **Blocked (no harness):** mọi CRUD UI, REST integration, cascade delete integrity.
- **No unit test added:** M06 không có logic thuần tách Domain; backlog tách attribution/validation.

## Rủi ro còn lại
- Attribution `CreatedBy` sai (BUG_M06_01) → audit "ai tạo job" không chính xác.
- Cùng anti-pattern tồn tại ở M12 (`VisaDialog`, `FlightDialog`) — ghi ở `06-bug-report.md` để M12 xử lý.
