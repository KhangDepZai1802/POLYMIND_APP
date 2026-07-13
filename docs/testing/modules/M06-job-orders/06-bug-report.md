# M06 — Job Orders · Bug Report

Chỉ ghi bug có bằng chứng source code. Status ban đầu: `Ready for Codex`.

---

## BUG_M06_01 — `JobOrderDialog.Save` gán `JobOrder.CreatedBy` cho user đầu tiên trong DB thay vì actor

- **Bug ID:** BUG_M06_01
- **Module ID:** M06
- **Title:** Khi tạo Job mới, `CreatedBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()` (user ĐẦU TIÊN, không `OrderBy`) thay vì người đang thao tác → sai truy vết người tạo đơn hàng. **Cùng lỗi đã fix ở BUG_M04_01 nhưng ở caller khác.**
- **Severity:** Low (attribution; không sai phân quyền/dữ liệu nghiệp vụ)
- **Priority:** P2
- **Business Flow ID:** BF-M06-02
- **Test Case ID:** TC_M06_005
- **Automated Test ID:** — (cần integration)
- **Environment:** mọi môi trường
- **Role:** super_admin/RM (có `job_orders:create` + `CanEditJobOrder`) — nạn nhân truy vết: bất kỳ ai không phải "user đầu tiên".
- **Preconditions:** đăng nhập bằng RM (không phải user seed đầu tiên); tạo Job mới.
- **Steps to Reproduce:**
  1. Đăng nhập RM.
  2. `/jobs` → "Thêm Job" → nhập quốc gia → Lưu.
  3. Kiểm `job_orders.created_by` của job vừa tạo.
- **Expected Result:** `created_by` = id của RM đang thao tác (actor).
- **Actual Result:** `created_by` = id user đầu tiên `db.Users` trả về (không OrderBy → phụ thuộc thứ tự DB, thường là super admin seed) — KHÔNG phải actor.
- **UI Evidence:** —
- **API Evidence:** —
- **Database Evidence:** `job_orders.created_by` không khớp người đăng nhập tạo job.
- **Suspected Source Area:** `src/Polymind.Web/Components/Pages/JobOrders/JobOrderDialog.razor:154` (`CreatedBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()`). Actor ĐÃ có sẵn tại dòng 126 (`authState`) nhưng bị bỏ qua.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/JobOrders/JobOrderDialog.razor` (Save — create path)
  - `src/Polymind.Web/Auditing/AuditLogHelpers.cs` (`GetRequiredUserIdAsync` — pattern chuẩn)
- **Dependencies:** không chặn module khác (attribution cục bộ).
- **Regression Risk:** Thấp — thay bằng `CreatedBy = await AuthStateProvider.GetRequiredUserIdAsync(db)` (đúng pattern BUG_M04_01 đã dùng). `AuthStateProvider` đã inject sẵn trong dialog.
- **Confidence Level:** Cao (source rõ ràng; đối chiếu trực tiếp với BUG_M04_01 đã fix).
- **Status:** Verified Fixed (code-level) — Claude 2026-07-11 (`08-verification-report.md`); runtime create-as-RM pending harness
- **Gợi ý hướng sửa (không bắt buộc):** `CreatedBy = await AuthStateProvider.GetRequiredUserIdAsync(db);` thay cho query user đầu tiên.

---

## Regression sweep — cùng anti-pattern "first user attribution" (chuyển module tương ứng)

Rà `Users.Select(u => u.Id).FirstOrDefault/First` toàn `src` phát hiện các instance khác của **cùng lỗi BUG_M04_01/BUG_M06_01**:

| Vị trí | Field bị gán sai | Module | Xử lý |
|---|---|---|---|
| `Visas/VisaDialog.razor:136` | `Visa.HandledBy` | **M12** | File bug khi QA M12 (BUG_M12_xx) — actor có sẵn, bỏ qua |
| `Visas/FlightDialog.razor:128` | `Flight.AssignedTo` | **M12** | File bug khi QA M12 (BUG_M12_xx) |
| `Auditing/AuditLogHelpers.cs:33` | fallback trong `GetRequiredUserIdAsync` | shared | **Observation** — đây là FALLBACK (dùng actor trước, chỉ fallback khi không có actor). Nên `throw` thay vì gán user đầu tiên. Low, không phải defect trực tiếp. |
| `Infrastructure/DemoDataSeeder.cs:23` | `adminId` seed demo | seed | **Chấp nhận** — seeding, không phải hành động user thật |

→ **Đề xuất Codex:** khi sửa BUG_M06_01, sửa luôn cụm VisaDialog/FlightDialog (M12) để dứt điểm anti-pattern; cân nhắc để `GetRequiredUserIdAsync` fallback `throw` thay vì gán user đầu tiên. (M12 sẽ verify chính thức khi QA tới.)

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M06_01 | Low | TC_M06_005 | BF-M06-02 | JobOrderDialog.Save CreatedBy | JobOrderDialog.razor, AuditLogHelpers.cs | attribution regression (đề xuất tách Domain) + TC_M06_005 runtime | Verified Fixed (code) — Claude 2026-07-11 |
