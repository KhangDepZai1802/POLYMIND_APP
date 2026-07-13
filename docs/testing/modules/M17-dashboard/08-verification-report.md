# Verification Report — M17 Dashboard

- **Module:** M17 — Dashboard
- **Verifier:** Claude (Independent Verification) — 2026-07-11 phiên #8
- **Fix under review:** CR-M17-1 (U-M17-1) — `07-fix-report.md`
- **Verification Status:** **Verified (code-level)**

## Scope

CR-M17-1: ẩn KPI tài chính (doanh thu/công nợ/quá hạn/hoa hồng đại lý) trên Home dashboard — chỉ Director/Accountant/SuperAdmin thấy; guard cả render lẫn query (fail-closed), không phụ thuộc ẩn UI.

## Evidence Reviewed

- `src/Polymind.Web/Components/Pages/Home.razor`:
  - Partner redirect `/my-commissions` giữ nguyên (`:163-167`).
  - `_canReadFinance = AuthorizeAsync(principal, ReportAccessRules.FinancialPermission).Succeeded` (`:170-172`) — dùng chung permission `financial_reports:read` với M16.
  - **Query gated:** công nợ/quá hạn (`:200-211`), doanh thu tháng + theo quốc gia (`:225-245`), top đại lý + hoa hồng (`:269-...`) đều nằm trong `if (_canReadFinance)`; Payments/AgentCommissions/Agents không chạy cho non-finance staff và không lưu vào component state.
  - **Recruitment KPI vô điều kiện:** leads, candidate active/departed, job orders, funnel, nhắc xuất cảnh (`:177-195, :213-267`) — mọi staff `dashboard:read` vẫn xem.
  - **Render gated:** 4 StatCard tài chính + grid doanh thu/top đại lý (`:35, :44, :87`) trong `@if (_canReadFinance)`.
- Permission matrix (từ CR-M16-1): Director/Accountant có `financial_reports:read`; SuperAdmin bypass; RM và staff khác không có → fail-closed.

## Bug-by-bug Verdict

| Item | Verdict | Bằng chứng |
|---|---|---|
| CR-M17-1 | **Verified Fixed (code-level)** | Non-finance staff không render và không query dữ liệu tài chính; finance role giữ nguyên; guard capability fail-closed (không ẩn CSS/UI đơn thuần). |

## Tests / Regression

- M16 access/permission regression **6/6** (policy/registry dùng chung với M17).
- Full suite: **Passed 122, Failed 0, Skipped 0**.
- Web build: **0 Warning, 0 Error**.
- Không sửa test/expected; không đổi authorization Home/Portal; không làm yếu scope; không ghi DB.

## Residual / Not Measured

- Runtime probe render/đếm SQL query theo role (bUnit/WebApplicationFactory + EF interceptor) chưa có → pending.
- Cookie/claim cũ trước M16 có thể chưa chứa `financial_reports:read` cho tới khi seed + re-login/security-stamp refresh.
- OBS-M17-02 (perf KPI tuyển dụng): ngoài phạm vi.

## Conclusion

CR-M17-1 **Verified Fixed (code-level)**. → `QA=No Confirmed Bugs`, `Codex=Fixed`, `Verification=Verified (code)`.
