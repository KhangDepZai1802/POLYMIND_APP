# M17 — Dashboard · Analysis

## 1. Module Overview
- **Module ID:** M17 · **Name:** Dashboard (Tổng quan)
- **Purpose:** (a) `/` Home — bảng KPI toàn công ty (lead, ứng viên, phễu tuyển dụng, công nợ, doanh thu, top đại lý) cho nhân sự; (b) `/me` Portal Overview — bảng tổng quan cá nhân hóa cho Phụ huynh/Học viên (tiến trình, đóng tiền, đào tạo, đơn hàng, vay).
- **Actor/Role:** Home = `dashboard:read` (staff). Portal `/me` = `candidates:read` (parent/student self-scoped).
- **Dependencies:** M02 (AgentScope + dashboard:read), M04 Lead, M05 Candidate, M07 Workflow, M09 Commission, M10 Finance, M12 Visa/Flight, M08 Training.
- **Entry:** `/` (Home), `/me` (Overview). Partner (agent/CTV) → redirect `/my-commissions`.

## 2. Source Code Map
| File | Vai trò |
|---|---|
| `Components/Pages/Home.razor` | KPI toàn công ty; `OnInitialized` load thống kê; `ShowStat` mở `StatDetailDialog`; **redirect partner** (dòng 148-154) |
| `Components/Pages/Portal/Overview.razor` | Dashboard self-scoped `/me`; load **chỉ** `scope.OwnedCandidateId` |
| `Components/Pages/Portal/MyCommissions.razor` | Đích redirect của partner (đại lý/CTV) — thuộc M09 |
| `Display/FinanceEligibility.cs` | `CandidateIdsAsync` lọc ứng viên đủ điều kiện tài chính |
| `StatCard` / `StatDetailDialog` | Component hiển thị thẻ KPI + dialog chi tiết |

## 3. UI Inventory
- **Home:** 14 StatCard (lead/candidate/finance/funnel KPIs) + bảng Lead theo trạng thái/nguồn + Doanh thu theo quốc gia + **Top đại lý (kèm số tiền hoa hồng)**; dialog chi tiết mỗi thẻ; loading state.
- **Portal `/me`:** hero chào; card Tiến trình/Đóng tiền/Đào tạo/Đơn hàng/Vay(nếu có)/Liên hệ; empty state "chưa gắn hồ sơ".

## 4. API / Endpoint Inventory
- Không có REST endpoint. Toàn bộ đọc DB qua Blazor Server circuit.

## 5. Database Impact
- Chỉ đọc: Leads, CandidateJobOrders, JobOrders, Payments, Visas, Flights, Agents, Candidates, AgentCommissions, TrainingRecords/Evaluations, Loans. Không ghi, không migration.

## 6. Roles & Permissions
| Trang | Policy | Ai có |
|---|---|---|
| `/` Home | `dashboard:read` | Director, RM, Recruiter, Consultant, Document, Visa, Accountant, SuperAdmin |
| `/` Home (partner) | — | Agent/CTV **không** có dashboard:read + **redirect** `/my-commissions` (dòng 150-154) |
| `/me` Overview | `candidates:read` | Parent/Student (self-scoped) + staff; self-scoped chỉ thấy `OwnedCandidateId` |

## 7. Risk Analysis
- **Authz Home (đúng):** `dashboard:read` staff-only; partner (agent/CTV) redirect; self-scoped (parent/student) không có dashboard:read → không vào `/`. Không IDOR (không nhận id từ URL).
- **Portal `/me` (đúng, cô lập):** load theo `scope.OwnedCandidateId`; không nhận id ngoài → parent/student chỉ thấy hồ sơ mình. Không leak.
- **[OBS-M17-01 → req U-M17-1] Dashboard phơi bày KPI TÀI CHÍNH cho MỌI staff:** Home hiển thị Doanh thu tháng, Công nợ phải thu, Khoản quá hạn, Doanh thu theo quốc gia, **Top đại lý + số tiền hoa hồng** cho **tất cả** role có `dashboard:read` — gồm recruiter/consultant/document/visa (không phải tài chính/quản lý). Song song quyết định **U-M16-1** (vừa giới hạn RM khỏi báo cáo tài chính). Cần user chốt: có lọc KPI tài chính theo role không?
- **[OBS-M17-02] Perf (Low):** Home `ToListAsync` nhiều bảng (candidateJobs, paidPayments, commissions, candidateAgents) rồi tính in-memory. Nặng khi dữ liệu lớn. Cùng lớp OBS-M16-02.

## 8. Unknowns
- **U-M17-1:** KPI tài chính trên dashboard (doanh thu/công nợ/hoa hồng đại lý) có nên **giới hạn** chỉ Director/Accountant/(RM?) — ẩn với recruiter/consultant/document/visa — hay giữ cho mọi staff? (Liên quan trực tiếp U-M16-1.)
