# M12 — Visa & Flight / Exit · Bug Report

> Chỉ ghi bug có bằng chứng source. Status ban đầu: `Ready for Codex`.

---

## BUG_M12_01 — `VisaDialog.Save` gán `Visa.HandledBy` cho user đầu tiên DB → **misroute nhắc việc visa**

- **Bug ID:** BUG_M12_01
- **Module ID:** M12
- **Title:** Khi tạo hồ sơ visa, `HandledBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()` (user ĐẦU TIÊN, không OrderBy) thay vì actor. **Không chỉ sai truy vết:** `NotificationService` gửi nhắc việc visa (phỏng vấn/kết quả) tới `HandledBy` → nhắc **sai người** (thường là super admin seed) thay vì VisaStaff thật xử lý hồ sơ.
- **Severity:** **Medium** (notification sai người — vi phạm RB-6 "đúng người"; không chỉ cosmetic attribution).
- **Priority:** P2
- **Business Flow ID:** BF-M12-01, BF-M12-05
- **Test Case ID:** TC_M12_002, TC_M12_019
- **Automated Test ID:** `M12_VisaFlightRulesTests.New_visa_is_attributed_to_the_authenticated_actor`
- **Environment:** mọi môi trường
- **Role:** super_admin/VisaStaff (có `visas:create`) — nạn nhân: VisaStaff thật + người bị nhắc oan (user đầu DB).
- **Preconditions:** đăng nhập VisaStaff (không phải user seed đầu); tạo hồ sơ visa có InterviewDate/ResultDate sắp tới.
- **Steps to Reproduce:**
  1. Đăng nhập VisaStaff.
  2. `/visa` → Thêm hồ sơ visa → chọn CJO, đặt InterviewDate trong horizon → Lưu.
  3. Kiểm `visas.handled_by`; chạy NotificationJob → xem recipient nhắc "Phỏng vấn visa".
- **Expected Result:** `handled_by` = id VisaStaff đang thao tác; nhắc việc gửi VisaStaff đó.
- **Actual Result:** `handled_by` = id user đầu `db.Users` (không OrderBy, thường super admin seed); nhắc việc gửi sai người.
- **UI Evidence:** —
- **API Evidence:** —
- **Database Evidence:** `visas.handled_by` ≠ người đăng nhập tạo visa.
- **Log Evidence:** —
- **Suspected Source Area:** `src/Polymind.Web/Components/Pages/Visas/VisaDialog.razor:136` (`new Visa { HandledBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync() }`). `AuthStateProvider` đã inject (dòng 3) nhưng bị bỏ qua. Routing hệ quả: `src/Polymind.Web/Notifications/NotificationService.cs:291` (`recipients = v.HandledBy is not null ? [v.HandledBy] : owners`).
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Visas/VisaDialog.razor` (Save — create path)
  - `src/Polymind.Web/Notifications/NotificationService.cs` (visa reminder recipients)
  - `src/Polymind.Web/Auditing/AuditLogHelpers.cs` (`GetRequiredUserIdAsync` — pattern chuẩn)
- **Dependencies:** ảnh hưởng M13 Notifications (visa reminder recipient).
- **Regression Risk:** Thấp — thay bằng `HandledBy = await AuthStateProvider.GetRequiredUserIdAsync(db)` (đúng pattern BUG_M04_01/M06_01 đã fix). Cùng lớp anti-pattern "first-user attribution".
- **Confidence Level:** Cao (source rõ; routing xác nhận ở NotificationService:291).
- **Status:** **Verified Fixed (code-level)** — Claude 2026-07-11 (`08-verification-report.md`); runtime E2E pending harness.
- **Gợi ý hướng sửa (không bắt buộc):** `HandledBy = await AuthStateProvider.GetRequiredUserIdAsync(db);`. Cân nhắc tách factory `Domain/Visas/VisaCreationRules.Create(actorId, …)` để có regression test không cần Blazor harness (như M06).

---

## BUG_M12_02 — `FlightDialog.Save` gán `Flight.AssignedTo` cho user đầu tiên DB (attribution)

- **Bug ID:** BUG_M12_02
- **Module ID:** M12
- **Title:** Khi tạo vé máy bay, `AssignedTo = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()` thay vì actor. Sai truy vết người phụ trách vé. **Không** dùng cho routing notification (departure reminder dùng `CandidateOwnersOr`) → chỉ cosmetic.
- **Severity:** **Low** (attribution; không sai notification/phân quyền).
- **Priority:** P3
- **Business Flow ID:** BF-M12-03
- **Test Case ID:** TC_M12_008
- **Automated Test ID:** `M12_VisaFlightRulesTests.New_flight_is_attributed_to_the_authenticated_actor`
- **Environment:** mọi môi trường
- **Role:** super_admin/VisaStaff (`flights:create`).
- **Preconditions:** đăng nhập VisaStaff (không phải user seed đầu); tạo vé.
- **Steps to Reproduce:**
  1. Đăng nhập VisaStaff.
  2. `/visa` tab Vé → Thêm vé → chọn CJO, nhập → Lưu.
  3. Kiểm `flights.assigned_to`.
- **Expected Result:** `assigned_to` = id actor đang tạo vé.
- **Actual Result:** `assigned_to` = id user đầu `db.Users`.
- **Database Evidence:** `flights.assigned_to` ≠ người tạo.
- **Suspected Source Area:** `src/Polymind.Web/Components/Pages/Visas/FlightDialog.razor:128` (`new Flight { AssignedTo = await db.Users.Select(u => u.Id).FirstOrDefaultAsync() }`). `AuthStateProvider` inject sẵn (dòng 3).
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Visas/FlightDialog.razor` (Save — create path)
  - `src/Polymind.Web/Auditing/AuditLogHelpers.cs`
- **Dependencies:** không chặn module khác.
- **Regression Risk:** Thấp — thay bằng `AssignedTo = await AuthStateProvider.GetRequiredUserIdAsync(db)`.
- **Confidence Level:** Cao.
- **Status:** **Verified Fixed (code-level)** — Claude 2026-07-11 (`08-verification-report.md`).
- **Gợi ý hướng sửa:** `AssignedTo = await AuthStateProvider.GetRequiredUserIdAsync(db);` (đề xuất sửa CHUNG cùng BUG_M12_01 vì cùng anti-pattern, đã sweep-note từ M06).

---

## Observations (theo dõi — không handoff Codex trừ khi user chốt)

- **OBS-M12-01 — VisaDialog/FlightDialog không ghi audit** (Low): `Save` create/update không gọi `db.AddAudit`. Các module khác đều audit mutation. Thiếu lịch sử thay đổi visa/flight. **Req U-M12-2.**
- **OBS-M12-02 — VisaStatus không có state-machine + no rowversion** (Low): Status set tự do (nhảy cóc NotSubmitted→Approved); không guard concurrency.
- **OBS-M12-03 — Không có đường runtime set `Flight.ActualDepartureAt`** (Low-Med, req): field chỉ set ở `DemoDataSeeder:421`; FlightDialog không có input; NotificationService/CsvExportEndpoints chỉ ĐỌC. Hệ quả: **report xuất cảnh thực tế luôn rỗng** với dữ liệu thật; không xác nhận được "đã bay". **Req U-M12-1** (xác nhận xuất cảnh ở FlightDialog hay workflow bước Departure?).
- **OBS-M12-04 — Không unique (CandidateId, JobOrderId) cho visa/flight** (Low): có thể tạo trùng hồ sơ visa/vé cho cùng cặp ứng viên–đơn hàng.

---

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M12_01 | Medium | TC_M12_002,019 | BF-M12-01/05 | VisaDialog.Save HandledBy first-user → misroute visa reminder | VisaDialog.razor, NotificationService.cs, AuditLogHelpers.cs | attribution regression + Claude verify visa reminder routing | **Fixed — chờ Claude xác minh** |
| 2 | BUG_M12_02 | Low | TC_M12_008 | BF-M12-03 | FlightDialog.Save AssignedTo first-user | FlightDialog.razor, AuditLogHelpers.cs | attribution regression | **Fixed — chờ Claude xác minh** |

> **Ghi chú:** BUG_M12_01/02 khép lại sweep-note "first-user attribution" từ M06 (`VisaDialog:136`+`FlightDialog:128`). Nên sửa CHUNG. Sau sửa: toàn `src` chỉ còn `AuditLogHelpers:33` (fallback, obs) + `DemoDataSeeder:23` (seed) dùng first-user — không phải defect.
