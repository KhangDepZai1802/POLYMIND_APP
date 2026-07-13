# M02 — Authorization · Traceability

| BF ID | Page/API | Role | State | Test Case IDs | Automated Test IDs | Coverage | Gap |
|---|---|---|---|---|---|---|---|
| BF-M02-01 Enforce policy | mọi trang `[Authorize]` | any | check | TC_M02_002, 008, 009, 010, 018 | `All_permission_names_are_wellformed...` | Unit vocab + manual enforce | Enforce runtime cần manual |
| BF-M02-02 super_admin bypass | mọi | super_admin | — | TC_M02_007 | — | Manual | — |
| BF-M02-03 Seed reconcile | — | — | — | TC_M02_001, 003, 005, 006, 017 | `Generates_20x5_100`, `Permission_names_are_unique`, `Registry_contains_all_resources...`, `Resources_are_unique` | Unit + manual restart | Reconcile runtime manual |
| BF-M02-04 Chỉnh phân quyền | `/admin` Phân quyền | super_admin | — | TC_M02_014, 015, 016, 019 | — | Manual | BUG_M02_01 |
| BF-M02-05 JWT quyền | `/api/*` | any | — | TC_M02_011, 012, 013 | — | Integration blocked | Cần harness |
| BF-M02-06 Data-scope | UI/query đối tác | agent/collab/parent/student | — | TC_M02_022 | — | Integration blocked | IDOR scope → M05/M20 |
| BF-M02-07 MessagingPolicy | tin nhắn | các role | — | TC_M02_020 | — | (chuyển M14) | Web ref bị khóa |
| (config actions) | — | — | — | TC_M02_004 | `Actions_are_exactly_crud_plus_approve` | Unit | — |

## Độ phủ tổng hợp

- **Test case tạo:** 22 (TC_M02_001 → 022).
- **Automated (Unit, PASS):** 6 test method (PermissionRegistry contract).
- **Manual cần chạy:** 9 (enforcement UI, tab Phân quyền, seed restart).
- **Integration blocked:** 4 (API 401/403, JWT claim, IDOR scope).
- **Requirement confirmed/source pass:** 1 (TC_M02_021 — accountant được approve thu/chi/hoa hồng/vay).
- **Bug phát hiện:** 1 (BUG_M02_01 Medium). + 1 rủi ro IDOR-scope chuyển M05/M20 kiểm.
- **Phạm vi CHƯA kiểm:** ma trận enforcement đầy đủ 12 role × 100 permission (chỉ kiểm mẫu); data-scope API.
