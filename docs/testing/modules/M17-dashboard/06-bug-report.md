# M17 — Dashboard · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M17_<NN>`.

## Kết luận: **No Confirmed Bugs** (Verified code-level)

Thuộc tính bảo mật/nghiệp vụ lõi đúng ở source:
- **Authz Home:** `[Authorize(Policy="dashboard:read")]` — chỉ staff; **agent/CTV redirect** `/my-commissions` (Home:150-154) + không có dashboard:read; parent/student không có dashboard:read.
- **Portal `/me` cô lập:** load theo `scope.OwnedCandidateId` (Overview:181-196), không nhận id ngoài → parent/student chỉ thấy hồ sơ mình. Không IDOR.
- **Không nhận tham số id từ URL** → không có bề mặt IDOR.
- **Chia 0 an toàn** (`Rate`, `payPercent`).

---

## Observations (theo dõi — không handoff Codex trừ khi user chốt)

- **OBS-M17-01 / CR-M17-1 — RESOLVED by Codex, chờ Claude:** Home dùng `financial_reports:read`; chỉ Director/Accountant/SuperAdmin render và query công nợ/quá hạn/doanh thu/quốc gia/top đại lý. RM/recruiter/consultant/document/visa chỉ load KPI tuyển dụng. U-M17-1 đã chốt và được thực thi.
- **OBS-M17-02 — Perf (Low):** Home nạp nhiều bảng đầy đủ (candidateJobs/paidPayments/commissions/candidateAgents) rồi tính in-memory. Cùng lớp OBS-M16-02. Nên aggregate ở SQL khi dữ liệu lớn.

## Codex Handoff Queue

| Order | Bug ID | Severity | Status |
|---:|---|---|---|
| 1 | **CR-M17-1** | Change | **✅ Verified Fixed (code) — Claude phiên #8:** policy guard UI + query path (`_canReadFinance` gate cả render lẫn Payments/Commissions/Agents query); suite 122/122, Web 0/0. Xem `08-verification-report.md`. |

### CR-M17-1 — Ẩn KPI tài chính trên Dashboard theo role
- **Nguồn:** OBS-M17-01 (đã user chốt U-M17-1).
- **Hiện trạng:** `Home.razor` hiển thị mọi StatCard + bảng cho tất cả role có `dashboard:read` (gồm recruiter/consultant/document/visa/RM).
- **Hướng cho Codex:** ẩn các thẻ/bảng **tài chính** — Công nợ phải thu, Khoản thu quá hạn, Doanh thu tháng này, Quốc gia doanh thu cao, Dashboard doanh thu theo quốc gia, **Top đại lý (số tiền hoa hồng)** — chỉ hiển thị khi user là **Director/Accountant/SuperAdmin**. Recruiter/Consultant/DocumentStaff/VisaStaff/**RM** chỉ thấy KPI tuyển dụng (lead, ứng viên, phễu, tỷ lệ trúng tuyển/visa/xuất cảnh). Nhất quán CR-M16-1 (RM bỏ báo cáo tài chính).
- **Required Files:** `Home.razor` (kiểm role hiển thị nhóm thẻ tài chính; có thể dùng `AuthorizeView`/kiểm `IsInRole`).

> **Kết luận M17:** `QA=No Confirmed Bugs`, `Codex=Fixed`, `Verification=Waiting for Fix` cho CR-M17-1. **Đang chờ Claude xác minh độc lập.** OBS-M17-02 perf giữ backlog.
