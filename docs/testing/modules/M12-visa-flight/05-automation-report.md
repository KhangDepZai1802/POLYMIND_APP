# M12 — Visa & Flight / Exit · Automation Report

## Framework & môi trường

- **Framework:** xUnit (`tests/Polymind.Tests/Polymind.Tests.csproj`, net10.0).
- **Reference:** `Polymind.Domain` (+ Infrastructure/Application). **KHÔNG** ref `Polymind.Web` → không unit-test được logic razor/service.
- **Môi trường:** Local. Không production, không secret.

## Test structure (M12)

- `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs` — 5 unit contract:
  - `VisaStatus_contains_full_lifecycle` (TC_M12_026)
  - `New_visa_defaults_to_not_submitted` (TC_M12_027)
  - `New_visa_has_no_handler_by_default` (TC_M12_028)
  - `New_flight_has_no_actual_departure_by_default` (TC_M12_029)
  - `New_flight_has_no_assignee_by_default` (TC_M12_030)

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 62, Skipped: 0  (57 trước + 5 M12)
```

| Loại | Số | Ghi chú |
|---|---|---|
| Pass | 62 | Toàn suite (5 M12 mới) |
| Fail | 0 | Bug M12 là **Application Defect** — không viết test fail để "chứng minh" ở tầng unit (logic ở Web); ghi ở 06-bug-report |
| Skipped | 0 | — |
| Blocked | — | Runtime create/edit + notification routing (cần harness) |

## Phân loại lỗi

- **Application Defect:** **BUG_M12_01** (VisaDialog HandledBy first-user → misroute visa reminder, Medium), **BUG_M12_02** (FlightDialog AssignedTo first-user, Low). → Codex.
- **Test Code Defect:** 0.
- **Environment Defect:** thiếu harness integration (WebApplicationFactory + DB) + bUnit → runtime tạo/sửa/reminder Blocked.
- **Requirement Ambiguity:** OBS-M12-03 (xác nhận xuất cảnh ở đâu? — U-M12-1), OBS-M12-01 (visa/flight có cần audit? — U-M12-2).

## Vì sao bug M12 chưa có test tự động fail

`HandledBy`/`AssignedTo` được gán trong `VisaDialog.razor`/`FlightDialog.razor` (Web) và routing reminder ở `NotificationService.cs` (Web). Test project không ref Web. Không viết test giả để "pass" — bug được chứng minh bằng đọc source (VisaDialog:136, FlightDialog:128, NotificationService:291). **Khuyến nghị Codex khi fix:** thay `db.Users.Select(u=>u.Id).FirstOrDefaultAsync()` bằng `AuthStateProvider.GetRequiredUserIdAsync(db)` (AuthStateProvider đã inject sẵn ở cả 2 dialog), và cân nhắc tách một helper attribution để có regression test không cần Blazor harness (như M06 `JobOrderCreationRules`).

## Automation backlog

| Hạng mục | Layer | Điều kiện |
|---|---|---|
| Attribution HandledBy/AssignedTo = actor | Unit | Tách factory (giống M06) hoặc integration harness |
| Visa reminder routing theo HandledBy | Integration | DB + NotificationService harness |
| ActualDepartureAt set + report | Integration | Cần đường runtime (OBS-M12-03) trước |
| Role/permission matrix visa/flight | Integration | Auth harness |
| Audit visa/flight (nếu user chốt cần) | Integration | Sau khi thêm audit |
