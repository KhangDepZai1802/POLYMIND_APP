# M19 — Audit Log · Traceability

## Business Flow → Test → Coverage

| Business Flow ID | Page/Source | API | Role | State | Test Case IDs | Automated Test IDs | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M19-01 (ghi audit) | `AuditLogHelpers.AddAudit` + ~40 call sites | — | mọi actor thao tác | immutable insert | TC_M19_010..018, 030-032 | `M19_AuditLogTests` (030/031) | Cao (static + entity contract) | Runtime E2E write→persist thật (harness) |
| BF-M19-02 (xem/lọc) | `Admin.razor` tab Nhật ký | — | Director, super_admin | read-only | TC_M19_001..006, 020..028 | — | Trung bình-Cao (static) | Runtime filter/sort/paging thật (harness) |

## Permission trace

| Permission | Cấp cho | Dùng ở | Test |
|---|---|---|---|
| `audit:read` | Director, SuperAdmin | `Admin.razor:128` tab | TC_M19_001/002/003/004/005 |
| `users:read` | Director, SuperAdmin | `Admin.razor:2` page gate | TC_M19_003/004 |

## Requirement/Observation trace

| ID | Loại | Test | Trạng thái |
|---|---|---|---|
| OBS-M19-01 | Ip/UserAgent không ghi | TC_M19_042, 032 | Requirement U-M19-1 |
| OBS-M19-02 | Login/logout không audit | TC_M19_040/041 | Requirement U-M19-1 |
| OBS-M19-03 | Fallback first-user mis-attribution | TC_M19_043 | Observation (Low, khuyến nghị throw/null) |
| OBS-M19-04 | Take(200), không paging/range/export | TC_M19_021 | Requirement U-M19-2 |
| OBS-M19-05 | Không enforce immutability DB-level | TC_M19_044 | Observation (Low) |
| OBS-M19-06 | Nhãn action chưa phủ hết (create_receipt/reset_password) | TC_M19_028 | Observation (cosmetic) |

## Gap analysis
- **Đã phủ (static/contract):** authorization xem nhật ký (2 lớp), atomicity ghi (cùng SaveChanges), actor attribution đúng, không log secret, không IDOR (không REST + admin-only), hiển thị/timezone/null-user, entity contract.
- **Chưa phủ (cần harness):** E2E thật cho write→view; filter runtime với dữ liệu thật; kịch bản actor-null runtime (OBS-M19-03); PoC append-only ở DB.
- **Requirement mở:** U-M19-1 (login/logout + Ip/UA), U-M19-2 (paging/range/export). **Không tự đoán → không tạo bug, ghi observation.**
