# M04 — Lead CRM · Bug Report

Chỉ ghi bug có bằng chứng source code. Status ban đầu: `Ready for Codex`.

---

## BUG_M04_01 — `Convert()` gán `Candidate.CreatedBy` cho user đầu tiên trong DB thay vì người thao tác

- **Bug ID:** BUG_M04_01
- **Module ID:** M04 (tạo dữ liệu M05)
- **Title:** Khi chuyển Lead → Ứng viên, `Candidate.CreatedBy` được gán `= db.Users.Select(u => u.Id).FirstOrDefaultAsync()` (user ĐẦU TIÊN, không `OrderBy`) thay vì actor thật → sai truy vết người tạo hồ sơ.
- **Severity:** Low
- **Priority:** P2
- **Business Flow ID:** BF-M04-05
- **Test Case ID:** TC_M04_008
- **Automated Test ID:** — (cần integration)
- **Environment:** mọi môi trường
- **Role:** người thực hiện convert (super_admin/RM/recruiter/consultant có `candidates:create`)
- **Preconditions:** đăng nhập bằng user KHÔNG phải "user đầu tiên" trong bảng; có 1 Lead chưa convert.
- **Test Data:** đăng nhập `recruiter@polymind.local` → convert 1 lead.
- **Steps to Reproduce:**
  1. Đăng nhập bằng recruiter (không phải super admin seed đầu tiên).
  2. `/leads/{id}` → "Chuyển thành ứng viên" → xác nhận.
  3. Kiểm `candidates.created_by` của ứng viên vừa tạo.
- **Expected Result:** `created_by` = id của recruiter đang thao tác (actor).
- **Actual Result:** `created_by` = id của user đầu tiên `db.Users` trả về (không OrderBy → phụ thuộc thứ tự DB, thường là super admin seed) — KHÔNG phải actor.
- **UI Evidence:** —
- **API Evidence:** —
- **Database Evidence:** `candidates.created_by` không khớp người đăng nhập thực hiện convert.
- **Log Evidence:** —
- **Suspected Source Area:** `LeadDetail.razor:597` (`var adminId = await db.Users.Select(u => u.Id).FirstOrDefaultAsync();`) dùng cho `CreatedBy = adminId` tại `:618`.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Leads/LeadDetail.razor` (Convert)
  - `src/Polymind.Domain/Entities/Candidate.cs` (CreatedBy)
- **Dependencies:** không chặn module khác.
- **Regression Risk:** Thấp — thay `adminId` bằng actor thật đã có sẵn pattern trong cùng file (`await AuthStateProvider.GetRequiredUserIdAsync(db)`). Cần kiểm các nơi khác đọc `Candidate.CreatedBy` để không phá giả định.
- **Confidence Level:** Cao (source rõ ràng; các method khác cùng file dùng actor đúng, chỉ Convert dùng "user đầu tiên").
- **Status:** Fixed
- **Codex resolution:** User chốt `Candidate.CreatedBy` luôn là actor thực hiện convert. `LeadDetail.Convert` lấy actor qua `AuthenticationStateProvider` và truyền vào mapping thuần; không còn query user đầu tiên.
- **Gợi ý hướng sửa:** `CreatedBy = await AuthStateProvider.GetRequiredUserIdAsync(db);` (như `actorId` ở các method khác) thay cho `adminId`.

---

## Ghi chú không nâng thành bug (theo dõi)

- **R2 (Low) tìm kiếm/lọc client-side toàn bộ `/leads`:** nạp mọi lead chưa convert vào RAM → rủi ro hiệu năng khi scale. Đề xuất phân trang server (API đã có). Chờ chốt U2 — cải tiến, không phải defect chức năng.
- **R3 (Low) convert race:** không unique constraint `Candidate.LeadId` → 2 request đồng thời có thể tạo trùng. Xác suất thấp; đề xuất unique index `candidates(lead_id) WHERE lead_id IS NOT NULL` — theo dõi ở M05/M07.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M04_01 | Low | TC_M04_008 | BF-M04-05 | LeadDetail.Convert CreatedBy | LeadDetail.razor, Candidate.cs | 3 unit regression + TC_M04_007/008 runtime | **Verified Fixed** (Claude 2026-07-10) |
