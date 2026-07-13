# M04 — Lead CRM · Automation Report

## Framework & dependency
- **Test framework:** xUnit 2.9.2 (`tests/Polymind.Tests`), như M01–M03.

## Automated Test IDs → Test Case
- **Không có test tự động mới ở session này.** 23 test case M04 là Unit-blocked (4) / Integration-blocked (5) / Manual (còn lại).

## Lý do chưa tự động được
1. **`LeadCareRules` + `BusinessRoleAccess` là logic thuần LÝ TƯỞNG để unit test** (chỉ phụ thuộc enum Domain), nhưng đặt trong **`Polymind.Web/Display`**. Test project không tham chiếu `Polymind.Web` (build test sẽ rebuild Web → khóa DLL `MSB3021` khi dev server `:5177` chạy). → TC_M04_017–020 **blocked**.
2. **CRUD/convert/revert/xóa** cần Blazor component + DB + AuthorizationService → integration harness (chưa có).
3. **API `/api/leads`** cần WebApplicationFactory + JWT + DB test.

## Lệnh chạy (suite chung)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Hiện có: 11 pass (M01 4 + M02 6 + smoke 1). M04 chưa thêm test.
```

## Kết quả
- **Pass:** 0 (M04) · **Fail:** 0 · **Blocked:** 4 unit (Web ref) + 5 integration.
- **Environment issue:** dev server `:5177` khóa DLL `Polymind.Web`.

## Automation backlog (ưu tiên — dễ thắng)
1. **Tách `LeadCareRules` (và/hoặc `BusinessRoleAccess`) sang `Polymind.Domain` hoặc `Polymind.Application`** (chỉ phụ thuộc enum) → test project ref được ngay → tự động hóa TC_M04_017–020 (ThresholdHours, Appointment-anchor, trạng thái kết thúc, DurationLabel). Đây là quick win có giá trị regression cao (rule nhắc chăm sóc).
2. **Integration (WebApplicationFactory + DB test):** POST/PUT/DELETE `/api/leads` + 401/403; convert tạo candidate + assert `CreatedBy` (regression BUG_M04_01); convert chống trùng + race.
3. **bUnit** cho LeadDetail: kiểm nút theo quyền (CanEditLead/CanDeleteLead), khóa khi IsConverted.
