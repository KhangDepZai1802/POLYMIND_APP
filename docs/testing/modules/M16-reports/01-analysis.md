# M16 — Reports & Export · Analysis

## 1. Module Overview
- **Module ID:** M16 · **Name:** Reports & Export (Báo cáo & Xuất file)
- **Purpose:** Trang `/reports` (biểu đồ marketing/tuyển dụng/tài chính, lọc theo khoảng thời gian) + xuất CSV/Excel/PDF 8 báo cáo + in phiếu thu/chi PDF.
- **Actor/Role:** `reports:read` = **Director, RecruitmentManager, Accountant, SuperAdmin**. Receipt PDF: `receipts:read` = **Accountant, Director, SuperAdmin**.
- **Dependencies:** M10 Finance (Payments/Expenses/Receipts), M09 Commissions, M04 Leads, M06 JobOrders, M07 Workflow (funnel), M12 Visa/Flight (departures).
- **Entry:** `/reports`; `/export/{slug}.{csv|xlsx|pdf}`; `/receipts/{id}.pdf`.

## 2. Source Code Map
| File | Vai trò |
|---|---|
| `Components/Pages/Reports/Reports.razor` | Trang biểu đồ + menu export; lọc range (`_rangeKey`: all/month/quarter/year/custom) |
| `Reporting/CsvExportEndpoints.cs` | 8 builder báo cáo + formatter CSV/Xlsx/Pdf + `ReceiptPdf`; map `/export` (gated `reports:read`) và `/receipts/{id}.pdf` (gated `receipts:read`) |
| `Program.cs:254-255` | `app.MapCsvExportEndpoints()` |
| `Infrastructure/.../DbSeeder.cs:37-112` | Role→permission (nguồn xác định ai có reports:read/receipts:read) |

## 3. UI Inventory
- Menu Excel/PDF/CSV (8 mục mỗi loại), bộ chọn khoảng thời gian (all/month/quarter/year/custom + date pickers), biểu đồ (MudChart), bảng số liệu.

## 4. API / Endpoint Inventory
| Method | Route | Auth | Side effect | Ghi chú |
|---|---|---|---|---|
| GET | `/export/{slug}.csv\|xlsx\|pdf` (8 slug) | `reports:read` | chỉ đọc, stream file | **Không** nhận tham số range → luôn toàn kỳ |
| GET | `/receipts/{id:guid}.pdf` | `receipts:read` | chỉ đọc, stream PDF | Không kiểm ownership từng phiếu (chi tiết §7) |

- 8 báo cáo: finance-monthly (12 tháng gần nhất), commissions (theo đại lý), overdue-payments, revenue-by-country, revenue-by-job-order, lead-by-province, recruitment-funnel, top-agents. Tất cả **company-wide, không lọc AgentScope** (đúng, vì chỉ management roles có `reports:read`).

## 5. Database Impact
- Chỉ đọc: Payments, Expenses, AgentCommissions, Agents, Candidates, JobOrders, Leads, CandidateJobOrders, Visas, Flights, Receipts. Không ghi, không migration.

## 6. Roles & Permissions (từ DbSeeder)
| Permission | Role có | Role KHÔNG có |
|---|---|---|
| `reports:read` | Director, RM, Accountant, SuperAdmin | Recruiter, Consultant, Document, Visa, **Agent, CTV, Parent, Student** |
| `receipts:read` | Accountant, Director, SuperAdmin | tất cả role còn lại (gồm Parent/Student/Agent/CTV) |

## 7. Risk Analysis
- **Export data-scope:** builders trả toàn bộ dữ liệu công ty **không lọc** → an toàn vì `reports:read` chỉ management (agent/CTV/parent/student **không** có). **Không phải bug** (khác M15: M15 `[Authorize]` không policy nên partner vào được).
- **[BUG_M16_01 — Low] Export bỏ qua khoảng thời gian đang chọn:** menu export dùng `Href` tĩnh (`/export/finance-monthly.xlsx`…) không truyền `_rangeKey`/custom range. Biểu đồ trên màn honor range, nhưng file xuất luôn toàn kỳ (finance-monthly=12 tháng; còn lại=all-time) → lệch kỳ vọng, dễ hiểu nhầm số liệu.
- **[OBS-M16-01] `/receipts/{id}.pdf` không kiểm ownership từng phiếu (latent IDOR):** bất kỳ ai có `receipts:read` (hiện: finance-only, được xem MỌI phiếu) tải được PDF của phiếu bất kỳ theo GUID. **Không khai thác được** với seed hiện tại; sẽ thành IDOR nếu `receipts:read` mở cho self-scoped/partner. Defense-in-depth: thêm kiểm scope nếu mở quyền.
- **[OBS-M16-02] Perf:** builders `ToListAsync` toàn bảng (Payments/Candidates/Commissions) vào RAM rồi group in-memory → nặng khi dữ liệu lớn. Low.
- **[OBS-M16-03 — req U-M16-1] RM thấy toàn bộ P&L:** RecruitmentManager có `reports:read` → xuất được doanh thu/chi phí/lợi nhuận, hoa hồng theo đại lý, top đại lý. Có đúng ý muốn cho vai trò tuyển dụng thấy tài chính tổng? Cần user chốt.
- **[OBS-M16-04] Filename `DateTime.Now`** (giờ local server) không nhất quán UTC. Cosmetic.

## 8. Unknowns
- **U-M16-1:** RecruitmentManager có nên xem/xuất báo cáo tài chính tổng (doanh thu/lợi nhuận/hoa hồng toàn công ty) không, hay giới hạn báo cáo tuyển dụng (funnel/lead/candidate)?
