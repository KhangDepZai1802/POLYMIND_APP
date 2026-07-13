# M17 — Dashboard · Business Flows

## BF-M17-01 — Xem KPI toàn công ty (Home)
- **Actor:** staff có `dashboard:read`.
- **Main:** mở `/` → nếu partner → redirect `/my-commissions`; ngược lại load KPI toàn công ty → hiển thị 14 thẻ + biểu đồ; bấm thẻ → dialog chi tiết.
- **Auth:** `[Authorize(Policy="dashboard:read")]` + guard redirect partner.
- **DB:** chỉ đọc. Không notification/history.

## BF-M17-02 — Dashboard cá nhân hóa (Portal `/me`)
- **Actor:** parent/student (self-scoped).
- **Main:** mở `/me` → resolve `scope.OwnedCandidateId` → load **chỉ** hồ sơ đó (tiến trình/đóng tiền/đào tạo/đơn hàng/vay) → hiển thị.
- **Isolation:** không nhận id ngoài; chưa gắn hồ sơ → thông báo liên hệ tư vấn viên.

## BF-M17-03 — Phân quyền vào Dashboard
| Role | `/` Home | `/me` |
|---|---|---|
| Director/Accountant/SuperAdmin | ✓ (đủ KPI) | ✓ (staff cũng vào được nhưng không có OwnedCandidate → empty) |
| RM/Recruiter/Consultant/Document/Visa | ✓ (gồm KPI tài chính — **U-M17-1**) | ✓/empty |
| Agent/CTV | ✗ (redirect `/my-commissions`) | ✗ |
| Parent/Student | ✗ (không dashboard:read) | ✓ (chỉ hồ sơ mình) |

## BF-M17-04 — KPI tài chính (điểm cần chốt)
- Home hiển thị: Doanh thu tháng, Doanh thu theo quốc gia, Công nợ phải thu, Khoản quá hạn, **Top đại lý + hoa hồng**.
- Hiện áp cho **mọi** `dashboard:read`. **U-M17-1:** có ẩn với recruiter/consultant/document/visa không?
