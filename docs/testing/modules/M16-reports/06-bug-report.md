# M16 — Reports & Export · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M16_<NN>`.

## BUG_M16_01 — Xuất báo cáo BỎ QUA khoảng thời gian đang chọn trên màn

- **Bug ID:** BUG_M16_01
- **Module ID:** M16
- **Title:** Trang `/reports` có bộ lọc khoảng thời gian (Tất cả/Tháng này/Quý này/Năm nay/Tùy chọn) và biểu đồ trên màn cập nhật theo lọc, **nhưng** menu Excel/PDF/CSV dùng `Href` **tĩnh** (`/export/finance-monthly.xlsx`, …) **không** truyền khoảng thời gian. Endpoint builder cũng không nhận tham số range → file xuất **luôn toàn kỳ** (finance-monthly = 12 tháng gần nhất; các báo cáo khác = all-time). Người dùng chọn "Tháng này" rồi bấm xuất vẫn nhận số liệu toàn bộ → **hiểu nhầm số liệu / sai kỳ báo cáo**.
- **Severity:** **Low** (không sai quyền, không sai DB; là sai lệch dữ liệu hiển thị vs file xuất → rủi ro ra quyết định theo file sai kỳ).
- **Priority:** P3
- **Business Flow ID:** BF-M16-02
- **Test Case ID:** TC_M16_012
- **Environment:** mọi môi trường.
- **Role:** Director/RM/Accountant/SuperAdmin (ai có `reports:read`).
- **Steps to Reproduce:**
  1. Mở `/reports`, chọn "Tháng này" (hoặc custom range).
  2. Bấm Excel → "Thu/chi theo tháng" (hoặc bất kỳ báo cáo nào).
  3. So file tải về với biểu đồ đang xem.
- **Expected:** file xuất phản ánh **đúng** khoảng thời gian đang chọn (giống biểu đồ).
- **Actual:** file luôn toàn kỳ, không theo range.
- **Source Evidence:**
  - `Components/Pages/Reports/Reports.razor:12-41` — `MudMenuItem Href="/export/{slug}.{ext}"` tĩnh, không kèm query range; `_rangeKey`/`_customFrom`/`_customTo` chỉ dùng cho biểu đồ.
  - `Reporting/CsvExportEndpoints.cs:24-31,49-55,66-289` — builders nhận **chỉ** `ApplicationDbContext`, không tham số range; finance-monthly cứng 12 tháng, còn lại all-time.
- **Suspected Source Area:** thiếu truyền + nhận tham số range (from/to) giữa Reports.razor và `/export/*`.
- **Required Files for Codex to Inspect:** `Reports.razor` (menu export + state range), `CsvExportEndpoints.cs` (thêm query `from`/`to` cho builders).
- **Dependencies:** không.
- **Regression Risk:** Thấp — thêm tham số optional, mặc định giữ hành vi cũ.
- **Confidence Level:** Cao (source rõ).
- **Status:** **✅ Verified Fixed (code-level) — Claude phiên #8 (2026-07-11).** Link truyền `from/to`; 8 builders áp range; invalid range trả 400; không tham số giữ hành vi cũ; financial slug re-check `financial_reports:read` → 403. M16 6/6, suite 122/122, Web 0/0. Xem `08-verification-report.md`.
- **Gợi ý hướng sửa:** thêm query `?from=yyyy-MM-dd&to=yyyy-MM-dd` (optional) vào link export theo `_rangeKey`; builders lọc `PaidDate/ExpenseDate/CreatedAt` trong [from,to] khi có; không có tham số → giữ hành vi hiện tại (tương thích ngược).

---

## Observations (theo dõi)

- **OBS-M16-01 — `/receipts/{id}.pdf` không kiểm ownership từng phiếu (latent IDOR, Med nếu quyền mở rộng):** endpoint trả PDF theo GUID chỉ gated `receipts:read`. Hiện `receipts:read` = finance-only (Accountant/Director/SuperAdmin) — được xem MỌI phiếu → **không khai thác được**. Nếu tương lai cấp `receipts:read` cho self-scoped/agent (để tự xem phiếu của mình) thì thành IDOR đọc phiếu người khác. **Defense-in-depth:** thêm kiểm receipt thuộc scope người gọi khi mở quyền. (Trùng OBS-M10-03.)
- **OBS-M16-02 — Perf (Low):** builders `ToListAsync` toàn bảng rồi group in-memory (Payments/Candidates/Commissions/JobOrders). Nặng khi dữ liệu lớn; nên aggregate ở SQL.
- **OBS-M16-03 / CR-M16-1 — RESOLVED by Codex, chờ Claude:** thêm `financial_reports:read`; Director/Accountant/SuperAdmin có, RM không. RM chỉ thấy/export `lead-by-province` + `recruitment-funnel`; sáu slug còn lại (gồm top-agents có cột hoa hồng) yêu cầu permission tài chính ở server. UI không query/render finance khi thiếu quyền.
- **OBS-M16-04 — Filename `DateTime.Now`** (giờ local server) không đồng nhất UTC toàn hệ thống. Cosmetic.
- **OBS-M16-05 — CSV formula injection (Low):** `EscapeCsv` bọc field có `,"`/newline nhưng không tiền tố an toàn cho ô bắt đầu `= + - @`. Dữ liệu xuất là tên ứng viên/đại lý/tỉnh (rủi ro thấp), nhưng nếu tên chứa công thức, mở bằng Excel có thể thực thi. Đề xuất prefix `'` hoặc chặn ký tự đầu.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Status |
|---:|---|---|---|---|---|---|---|
| 1 | BUG_M16_01 | Low | TC_M16_012/032 | BF-M16-02 | Export range | Reports.razor, CsvExportEndpoints.cs, ReportAccessRules.cs | **✅ Verified Fixed (code) — Claude phiên #8** |
| 2 | CR-M16-1 | Change | TC_M16_004/006 | BF-M16-04 | RM recruitment-only | DbSeeder.cs, Reports.razor, CsvExportEndpoints.cs, PermissionRegistry.cs | **✅ Verified Fixed (code) — Claude phiên #8** |

> **Kết luận M16:** `QA=Bugs Found`, `Codex=Fixed`, `Verification=Waiting for Fix`. **Đang chờ Claude xác minh độc lập.** OBS-M16-01/02/04/05 giữ nguyên.
