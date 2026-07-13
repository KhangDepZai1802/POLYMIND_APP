# M16 — Reports & Export · Business Flows

## BF-M16-01 — Xem báo cáo trên màn (có lọc range)
- **Actor:** Director/RM/Accountant/SuperAdmin.
- **Main:** mở `/reports` → chọn khoảng thời gian → biểu đồ/bảng cập nhật theo range (client-side).
- **Auth:** `[Authorize(Policy="reports:read")]`.

## BF-M16-02 — Xuất file báo cáo (CSV/Excel/PDF)
- **Main:** bấm mục trong menu Excel/PDF/CSV → GET `/export/{slug}.{ext}` → stream file.
- **Auth:** group `RequireAuthorization("reports:read")`.
- **DEFECT:** không truyền range → file luôn toàn kỳ dù màn đang lọc (BUG_M16_01).

## BF-M16-03 — In phiếu thu/chi PDF
- **Main:** GET `/receipts/{id}.pdf` → load Receipt theo id → dựng PDF (thu/chi + bên nộp/nhận + số tiền + diễn giải).
- **Auth:** `receipts:read` (finance-only). **Không** kiểm receipt có thuộc phạm vi người gọi (OBS-M16-01, latent).

## BF-M16-04 — Phân quyền truy cập báo cáo
| Role | `/reports` + `/export/*` | `/receipts/{id}.pdf` |
|---|---|---|
| SuperAdmin/Director/Accountant | ✓ | ✓ |
| RecruitmentManager | ✓ (gồm tài chính — **U-M16-1**) | ✗ |
| Recruiter/Consultant/Document/Visa | ✗ | ✗ |
| Agent/CTV/Parent/Student | ✗ | ✗ |
