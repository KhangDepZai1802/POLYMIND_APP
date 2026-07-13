# M17 — Dashboard · Test Cases

> `TC_M17_<NNN>`. Source-verified (authz + scope); runtime cần DB + đăng nhập theo role.

## Authorization / Scope
| TC | Name | Role | Expected | Kết quả |
|---|---|---|---|---|
| TC_M17_001 | Agent/CTV không xem dashboard công ty (High) | agent/collaborator | Mở `/` → **redirect** `/my-commissions` | **Pass** (Home:150-154 + không có dashboard:read) |
| TC_M17_002 | Parent/Student không vào `/` (High) | parent/student | `/` → access denied/redirect (`RedirectToLogin` → `/me`) | **Pass** (không có dashboard:read) |
| TC_M17_003 | Parent chỉ thấy hồ sơ mình ở `/me` (Critical) | parent/student | `/me` → chỉ dữ liệu `OwnedCandidateId` | **Pass** (Overview:181-196 dùng scope) |
| TC_M17_004 | Chưa gắn hồ sơ → thông báo (Med) | parent/student mới | `/me` → alert "chưa gắn hồ sơ" | **Pass** (Overview:13-18) |
| TC_M17_005 | Chưa đăng nhập (High) | anon | `/`, `/me` → redirect login | **Pass** (`[Authorize]`) |
| TC_M17_006 | Staff xem KPI công ty (Med) | recruiter/accountant | Recruiter xem KPI tuyển dụng; accountant xem thêm KPI tài chính | **Pass (Codex source/build) — runtime role render pending** |

## KPI tài chính (CR-M17-1)
| TC | Name | Role | Expected | Kết quả |
|---|---|---|---|---|
| TC_M17_010 | Staff không thuộc finance không thấy dữ liệu tài chính | RM/recruiter/consultant/document/visa | Không render 4 thẻ + 2 bảng tài chính; không query Payments/Commissions/Agents | **Pass (Codex source/build) — runtime query probe pending** |
| TC_M17_011 | Finance roles vẫn thấy đủ KPI tài chính | Director/Accountant/SuperAdmin | Có công nợ/quá hạn/doanh thu/quốc gia/top đại lý | **Pass (policy/source) — runtime role render pending** |

## Functional
| TC | Name | Expected | Kết quả |
|---|---|---|---|
| TC_M17_020 | Bấm thẻ KPI mở dialog chi tiết | Dialog StatDetailDialog đúng số liệu | Source-verified |
| TC_M17_021 | Tỷ lệ chia 0 an toàn | 0% khi tổng=0 (Rate/SameMonth) | Source-verified (Rate:272) |
| TC_M17_022 | Công nợ lọc theo FinanceEligibility | Chỉ ứng viên đủ điều kiện | Source-verified (Home:157,183,202) |
| TC_M17_023 | Overview tính đóng tiền 4 stage đúng | paid/paidSteps/nextStage đúng | Source-verified (Overview:221-231) |
