# M06 — Job Orders · Business Flows

## BF-M06-01 — Xem danh sách / chi tiết job
- **Role:** staff có `job_orders:read`.
- **Main:** `/jobs` → load tất cả → filter client (quốc gia/nhóm/tìm). `/jobs/{id}` → chi tiết + ứng viên trong job.
- **AuthZ:** `[Authorize job_orders:read]`. Agent/collaborator/parent/student không có quyền → không vào.
- **Risk:** perf client-filter (R4). Không data-scope (đúng — job company-wide).

## BF-M06-02 — Tạo job
- **Role:** `job_orders:create` + `CanEditJobOrder` (super_admin/RM).
- **Main:** "Thêm Job" (gate `job_orders:create`) → `JobOrderDialog` → nhập (Country bắt buộc) → `Save` re-check permission + role → sinh `Code` JO-YYYYMM-XXX → insert.
- **Validation:** Country bắt buộc (Snackbar nếu trống). Quantity ≥ 0, CostAmount ≥ 0.
- **DB:** insert `job_orders`; **`CreatedBy` PHẢI = actor** (hiện SAI → BUG_M06_01).
- **Risk:** attribution sai (R1/BUG_M06_01); duplicate submit (Save có `_saving` guard).

## BF-M06-03 — Sửa job
- **Role:** `job_orders:update` + `CanEditJobOrder`.
- **Main:** "Sửa Job" (gate `_canUpdate`) → dialog load job → sửa → `Save` re-check → `ApplyTo` + `UpdatedAt`.
- **DB:** update `job_orders`. Không đổi `CreatedBy` (giữ nguyên — đúng).
- **Risk:** lost update (R6).

## BF-M06-04 — Xóa job (cascade)
- **Role:** `job_orders:delete` + `CanDeleteJobOrder` (super_admin/RM).
- **Main:** "Xóa" → confirm → `DeleteJobOrder` **re-check** (230) → single-context: remove workflow records/assignments/visas/flights/commissions/commission-configs/notifications; unlink leads + payments; audit `delete/job_orders`; remove job.
- **DB:** nhiều bảng, một transaction. Giữ hồ sơ ứng viên + khoản thu (unlink).
- **Risk:** sót bảng nếu thêm quan hệ mới (R3).

### State transition (JobOrderStatus)
| Current | Action | Role | Next | Note |
|---|---|---|---|---|
| Recruiting/… | Sửa Status (dialog) | super_admin/RM | bất kỳ status | Đổi tự do qua dropdown; không state-machine ràng buộc (đúng thiết kế — job là master data) |

> Không có ràng buộc chuyển trạng thái phức tạp: JobOrderStatus là thuộc tính master-data đổi tự do bởi super_admin/RM. Workflow 20 bước của ứng viên (M07) mới có state-machine.
