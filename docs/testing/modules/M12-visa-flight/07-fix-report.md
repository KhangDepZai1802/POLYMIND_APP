# Module Fix Report

## Summary

- **Module ID:** M12
- **Module Name:** Visa & Flight / Exit
- **Bugs Received:** 2
- **Bugs Fixed:** 2
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Verification:** Chờ Claude xác minh độc lập; Codex không đánh dấu `Verified Fixed`.

## BUG_M12_01

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Đọc toàn bộ context package M12 (`01`–`06`) và kiểm tra create path của `VisaDialog`.
- Xác nhận dialog đã inject `AuthenticationStateProvider` nhưng lại lấy user đầu tiên không có thứ tự từ `db.Users` để gán `HandledBy`.
- Kiểm tra consumer trong `NotificationService`: nhắc phỏng vấn/kết quả visa ưu tiên gửi duy nhất tới `Visa.HandledBy`; vì vậy attribution sai dẫn tới recipient sai.
- Đối chiếu mẫu đã được Claude xác minh ở M06: Web resolve authenticated actor qua `GetRequiredUserIdAsync`, Domain factory nhận actor bắt buộc.

### Root Cause

Create path dùng một truy vấn user tùy ý trong database thay cho identity của người đang thực hiện thao tác. Trường `HandledBy` đồng thời là khóa routing notification nên lỗi attribution lan sang M13.

### Fix

- Resolve actor từ `AuthStateProvider.GetRequiredUserIdAsync(db)` sau khi permission `visas:create` đã được re-check.
- Tạo visa qua `VisaFlightCreationRules.CreateVisa(actorId)`, bảo đảm `HandledBy` nhận đúng actor trước khi map dữ liệu form.
- Thêm regression test `New_visa_is_attributed_to_the_authenticated_actor` gắn BUG_M12_01 / TC_M12_002,019.

### Why This Fix Is Correct

- BF-M12-01/BF-M12-05 và TC_M12_002/019 yêu cầu người xử lý là actor tạo hồ sơ.
- `NotificationService` tiếp tục dùng `HandledBy` như thiết kế; khi nguồn dữ liệu đúng, reminder được route đúng mà không cần thay đổi notification logic.
- Edit path không ghi đè `HandledBy`, nên ownership hiện có không bị thay đổi ngoài ý muốn.

## BUG_M12_02

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Kiểm tra create path của `FlightDialog` và xác nhận cùng anti-pattern lấy user đầu database để gán `AssignedTo`.
- Kiểm tra `NotificationService`: departure reminder dùng candidate owners/role fallback, không dùng `Flight.AssignedTo`; tác động của bug giới hạn ở attribution.

### Root Cause

Create path không dùng authentication context dù `AuthenticationStateProvider` đã được inject, khiến `AssignedTo` phụ thuộc thứ tự tùy ý của bảng users.

### Fix

- Resolve actor qua `AuthStateProvider.GetRequiredUserIdAsync(db)`.
- Tạo flight qua `VisaFlightCreationRules.CreateFlight(actorId)`, bảo đảm `AssignedTo` đúng actor.
- Thêm regression test `New_flight_is_attributed_to_the_authenticated_actor` gắn BUG_M12_02 / TC_M12_008.

### Why This Fix Is Correct

- Khớp BF-M12-03 và TC_M12_008: người phụ trách vé mới là người đang tạo vé.
- Không thay đổi departure reminder routing, permission, form mapping hay edit behavior.

## Shared Evidence

### Files Inspected

- `docs/testing/modules/M12-visa-flight/01-analysis.md` → `06-bug-report.md`
- `src/Polymind.Web/Components/Pages/Visas/VisaDialog.razor`
- `src/Polymind.Web/Components/Pages/Visas/FlightDialog.razor`
- `src/Polymind.Web/Notifications/NotificationService.cs`
- `src/Polymind.Web/Auditing/AuditLogHelpers.cs`
- `src/Polymind.Domain/Entities/Visa.cs`
- `src/Polymind.Domain/Entities/Flight.cs`
- `src/Polymind.Domain/JobOrders/JobOrderCreationRules.cs`
- `tests/Polymind.Tests/M06_JobOrderCreationRulesTests.cs`
- `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs`

### Files Changed

- `src/Polymind.Domain/Visas/VisaFlightCreationRules.cs`
- `src/Polymind.Web/Components/Pages/Visas/VisaDialog.razor`
- `src/Polymind.Web/Components/Pages/Visas/FlightDialog.razor`
- `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs`

### Symbols Changed

- `VisaFlightCreationRules.CreateVisa`
- `VisaFlightCreationRules.CreateFlight`
- `VisaDialog.Save` (create branch)
- `FlightDialog.Save` (create branch)
- `M12_VisaFlightRulesTests.New_visa_is_attributed_to_the_authenticated_actor`
- `M12_VisaFlightRulesTests.New_flight_is_attributed_to_the_authenticated_actor`

### Source Evidence After Fix

- Không còn truy vấn `db.Users.Select(...).FirstOrDefaultAsync()` trong hai dialog M12.
- `VisaDialog` và `FlightDialog` đều resolve authenticated actor rồi truyền vào Domain factory.
- `NotificationService:291–293` vẫn route visa reminder tới `HandledBy`, nay được tạo từ actor thật.
- Sweep first-user attribution trong runtime create path đã đóng; `AuditLogHelpers` fallback và `DemoDataSeeder` nằm ngoài phạm vi hai bug này.

### Alternatives Considered

- Chỉ thay initializer trực tiếp bằng lời gọi auth helper: sửa được bug nhưng khó khóa attribution bằng unit test khi chưa có bUnit harness.
- Sửa `NotificationService` để bỏ qua `HandledBy`: làm mất semantics người xử lý visa và không sửa dữ liệu attribution gốc.
- Thay đổi fallback của `GetRequiredUserIdAsync`: là observation dùng chung, có phạm vi/rủi ro lớn hơn M12 và không được Claude handoff trong hai bug này.

### Impact

- **API impact:** không thay đổi endpoint/contract; module dùng Blazor Server.
- **Database impact:** không migration/schema/data rewrite.
- **UI impact:** không thay đổi form hoặc luồng thao tác.
- **Notification impact:** hồ sơ visa mới route reminder tới đúng actor được gán `HandledBy`.
- **Security impact:** không nới quyền; permission create/update vẫn được re-check trước mutation.
- **Backward compatibility:** hồ sơ visa/flight cũ không bị viết lại; edit path giữ attribution hiện có.

### Regression Risks

- Nếu authentication context không có NameIdentifier, shared helper vẫn dùng fallback user đầu DB; đây là observation dùng chung đã được QA ghi nhận, không phải phần sửa M12. Với phiên đăng nhập hợp lệ, helper trả actor trước khi chạm fallback.
- Factory chỉ khởi tạo attribution; `FormModel.ApplyTo` không gán lại `HandledBy`/`AssignedTo`, đã được kiểm tra ở source.
- Chưa có bUnit/Playwright harness để tự động submit dialog với nhiều user và chạy NotificationJob end-to-end.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `New_visa_is_attributed_to_the_authenticated_actor` | Unit | Passed | BUG_M12_01 / TC_M12_002,019 |
| `New_flight_is_attributed_to_the_authenticated_actor` | Unit | Passed | BUG_M12_02 / TC_M12_008 |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --no-restore --nologo` | Regression | Passed | 64 passed, 0 failed, 0 skipped |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=C:\tmp\polymind-m12-web-build\` | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** 2 attribution regressions, toàn bộ shared suite, Web compile/Razor build.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** runtime multi-user DB/UI + NotificationJob E2E chưa có harness.

### Verification Instructions for Claude

- Đọc diff thật của hai dialog, Domain factory và regression tests; không chỉ dựa vào báo cáo này.
- Chạy lại full test suite và Web/solution build.
- Đăng nhập bằng VisaStaff không phải user seed đầu tiên, tạo visa có InterviewDate/ResultDate trong 7 ngày; xác nhận `visas.handled_by` bằng actor đó.
- Chạy NotificationJob/`GenerateRemindersForAllUsersAsync`; xác nhận `ReminderVisa` được tạo cho actor trong `HandledBy`, không phải user đầu DB.
- Tạo flight bằng actor tương tự; xác nhận `flights.assigned_to` bằng actor đó.
- Sửa visa/flight hiện có và xác nhận edit path không đổi attribution ngoài ý muốn.
- Xác nhận departure reminder routing vẫn dùng candidate owners/role fallback và không bị thay đổi bởi fix.
- Chỉ Claude cập nhật verdict `Verified Fixed`; hiện M12 đang **Fixed — chờ Claude xác minh**.
