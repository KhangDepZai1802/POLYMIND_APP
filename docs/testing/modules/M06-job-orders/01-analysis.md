# M06 — Job Orders · Phân tích

> Nguồn đã đọc: `JobOrders.razor` (248), `JobOrderDetail.razor` (317), `JobOrderDialog.razor` (237), `ResourceEndpoints.MapJobOrdersApi`, entity `JobOrder`, `BusinessRoleAccess`. Workflow gắn ứng viên (CandidateJobOrder) → M07; visa/vé → M12; hoa hồng → M09.

## 1. Module Overview
- **Module ID:** M06 · **Name:** Job Orders (Đơn hàng tuyển dụng)
- **Business purpose:** Quản lý đơn hàng tuyển dụng (company-wide, không gắn ứng viên riêng): mã JO, quốc gia, nhóm việc làm, công ty/nghiệp đoàn, số lượng, lương/chi phí, hạn ứng tuyển, trạng thái. Ứng viên gắn vào job qua `CandidateJobOrder` (M07).
- **Actor/Role:** chỉ **staff nội bộ** có `job_orders:*` (super_admin/RM tạo-sửa-xóa; các staff khác chỉ đọc). Agent/collaborator/parent/student **không** có `job_orders:read` (đối chiếu M02).
- **Dependencies:** M02 (permission). Không cần AgentScope (job không phải dữ liệu per-candidate).
- **Entry:** `/jobs` (`/job-orders`), `/jobs/{id}`, REST `GET /api/job-orders`. **Exit:** xóa job (cascade gỡ liên kết).

## 2. Source Code Map
| File | Vai trò | Ghi chú |
|---|---|---|
| `JobOrders.razor` | List + filter (quốc gia/nhóm/tìm) | `[Authorize job_orders:read]`; client-side filter (perf obs) |
| `JobOrderDetail.razor` | Chi tiết + ứng viên trong job + sửa/xóa | edit/delete gate permission + `CanEdit/DeleteJobOrder` (super_admin/RM), re-check |
| `JobOrderDialog.razor` | Form tạo/sửa | Save re-check permission + role; **BUG_M06_01** `CreatedBy` |
| `ResourceEndpoints.MapJobOrdersApi` | REST `GET /api/job-orders` | gate `job_orders:read` (staff only), paged; DTO không PII |
| `JobOrder.cs` (Domain) | Entity | Code, Country, Category, Status, CostAmount, dates, `CreatedBy` |

## 3. UI Inventory
- List: search (mã/công ty/nghề/quốc gia), filter nhóm việc làm + quốc gia, group theo Category, card job, deadline đỏ ≤7 ngày, empty state.
- Detail: thông tin job, đãi ngộ/thưởng, danh sách ứng viên trong job (link `/candidates/{id}`), nút Sửa/Xóa (theo quyền).
- Dialog: form (Category, Country*, Status, company, union, field, quantity, salary, cost, 4 dates, requirements, benefits, bonus).

## 4. API Inventory
| Method | Route | AuthZ | Validation | DB side effect | Notes |
|---|---|---|---|---|---|
| GET | `/api/job-orders` | Bearer `job_orders:read` | country filter, paging | đọc | DTO không PII; staff-only quyền |

## 5. Database Impact
- `job_orders`: `Code` (JO-YYYYMM-XXX), `Country`, `Category`, `Status`, `CostAmount`, các mốc ngày, `CreatedBy`.
- Xóa job → manual cascade single-context: `candidate_job_orders`, `workflow_step_records`, `visas`, `flights`, `agent_commissions`, `agent_commission_configs`, `notifications`; **unlink** `leads.interested_job_order_id`=null, `payments.job_order_id`=null (giữ khoản thu + hồ sơ ứng viên). Không FK cascade DB.

## 6. Roles & Permissions
| Action | Permission | Role gate | Extra | Source |
|---|---|---|---|---|
| Xem list/detail | `job_orders:read` | — (staff) | — | `[Authorize]` |
| Tạo | `job_orders:create` | `CanEditJobOrder` (super_admin/RM) | — | Dialog.Save 125-132 |
| Sửa | `job_orders:update` | `CanEditJobOrder` | — | Dialog.Save + OpenEdit re-check |
| Xóa | `job_orders:delete` | `CanDeleteJobOrder` (super_admin/RM) | confirm | DeleteJobOrder 230 (re-check) |

## 7. Risk Analysis
| # | Risk | Mức | Trạng thái |
|---|---|---|---|
| R1 | `CreatedBy` = user đầu tiên thay vì actor (create) | Low | **BUG_M06_01** (confirmed) |
| R2 | REST `/api/job-orders` lộ dữ liệu ngoài quyền | Low | **Không** — chỉ staff có `job_orders:read`; job không per-candidate; DTO không PII |
| R3 | Xóa job sót bảng liên quan (orphan) | Med | Obs — cascade đầy đủ hiện tại; rủi ro bảo trì (như OBS-M05-01) |
| R4 | List client-side filter (perf) | Low | Obs |
| R5 | Non-super_admin/RM sửa/xóa | High | **Chặn** (permission + role re-check) — không bug |
| R6 | Lost update (2 người sửa 1 job) | Low | Obs — no concurrency token, last-write-wins |

## 8. Unknowns
- Không có điểm nghiệp vụ mơ hồ ở M06. Ràng buộc "1 ứng viên = 1 job active" thực thi ở M05/M07 (RB-2), không ở đây.
