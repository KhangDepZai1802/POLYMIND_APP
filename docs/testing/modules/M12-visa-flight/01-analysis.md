# M12 — Visa & Flight / Exit · Analysis

> QA phiên #4 (Claude 2026-07-11). Đọc source thật; không sửa business logic. Dep: **M05 Candidate (Verified)**, **M07 Workflow (Verified)**.

## 1. Module Overview

- **Module ID:** M12
- **Module name:** Visa & Xuất cảnh (Visa & Flight)
- **Business purpose:** Theo dõi hồ sơ **visa** (loại, trạng thái, ngày nộp/phỏng vấn/kết quả, lý do từ chối) và **vé máy bay/xuất cảnh** (hãng, mã vé, giờ bay, sân bay, xuất cảnh thực tế) của ứng viên theo đơn hàng (CandidateJobOrder). Cấp dữ liệu cho nhắc việc (RB-6/M13) và báo cáo xuất cảnh (M16).
- **Actor/Role:** VisaStaff (full visas+flights), super_admin (all); Director + DocumentStaff (read); không role nào self/agent-scoped.
- **Dependencies:** M02 Authorization (`visas:*`, `flights:*`), M05 Candidate + M06/M07 (visa/flight gắn CandidateJobOrder), M13 Notifications (visa reminder theo `HandledBy`; departure reminder), M16 Reports (actual departure export).
- **Entry point:** `/visa` (2 tab: Hồ sơ Visa, Vé máy bay).
- **Exit point:** Visa Approved/Rejected; Flight có `ActualDepartureAt` (xuất cảnh thực tế) — **nhưng không có đường set runtime, xem OBS-M12-03**.

## 2. Source Code Map

| File | Loại | Symbol | Mục đích | Dependency |
|---|---|---|---|---|
| `Components/Pages/Visas/Visas.razor` | Page `/visa` | `OnInitializedAsync`, `Load`, `OpenCreate/EditVisa`, `OpenCreate/EditFlight`, `HandleResult` | Danh sách 2 tab; mở dialog tạo/sửa | DbFactory, AuthZ |
| `Components/Pages/Visas/VisaDialog.razor` | Dialog | `Save`, `OnCjoChanged`, `FormModel` | Tạo/sửa hồ sơ visa | DbFactory, AuthZ, AuthStateProvider |
| `Components/Pages/Visas/FlightDialog.razor` | Dialog | `Save`, `OnCjoChanged`, `FormModel` | Tạo/sửa vé máy bay | DbFactory, AuthZ, AuthStateProvider |
| `Domain/Entities/Visa.cs` | Entity | — | CandidateId/JobOrderId/VisaType/Country/SubmittedDate/InterviewDate/ResultDate/Status/RejectionReason/Notes/**HandledBy** | BaseEntity, VisaStatus |
| `Domain/Entities/Flight.cs` | Entity | — | CandidateId/JobOrderId/Airline/TicketCode/DepartureDate/Time/Airport/**ActualDepartureAt**/**AssignedTo**/Notes | BaseEntity |
| `Domain/Enums/Enums.cs` | Enum | `VisaStatus` | NotSubmitted/Preparing/Submitted/AdditionalRequired/Approved/Rejected | — |
| `Notifications/NotificationService.cs` | Service (cross) | visa block 281-299, flight block 301-313 | Visa reminder → **`HandledBy`**; departure reminder (không dùng AssignedTo) | — |
| `Reporting/CsvExportEndpoints.cs` | Export (cross) | 216, 245 | Report actual departures (`ActualDepartureAt != null`) | — |
| `Infrastructure/Persistence/DbSeeder.cs` | Seed | RolePermissionMap 74,79-80 | VisaStaff AllActions visas+flights; Director/Document read | — |
| `Infrastructure/Persistence/DemoDataSeeder.cs` | Seed demo | 421 | **Chỗ DUY NHẤT set `ActualDepartureAt`** (theo workflow step) | — |

**Không có REST endpoint** cho visas/flights — Blazor Server component. **Không có chức năng xóa** visa/flight.

## 3. UI Inventory

- **`/visa`:** MudTabs 2 tab. Tab Visa: DataGrid (ứng viên/quốc gia/loại/trạng thái chip/ngày nộp/edit) + pager; nút "Thêm hồ sơ visa" (AuthorizeView visas:create); mobile card; empty state. Tab Flight: DataGrid (ứng viên/hãng/mã vé/điểm đến/giờ bay/edit) + pager; nút "Thêm vé" (flights:create); mobile card; empty state.
- **VisaDialog:** select CandidateJobOrder (khóa khi edit) · VisaType · Status select · SubmittedDate/InterviewDate/ResultDate · RejectionReason (chỉ khi Rejected) · Notes.
- **FlightDialog:** select CandidateJobOrder (khóa khi edit) · Airline · TicketCode · DepartureDate/Time · sân bay đi/đến · Notes. **Không có input `ActualDepartureAt`.**

## 4. API Inventory

Không REST API. Handler Blazor Server:

| Thao tác | Handler | AuthZ | DB side effect | Audit |
|---|---|---|---|---|
| Tạo visa | `VisaDialog.Save` (create) | `visas:create` | insert `visas`; `HandledBy = first user` (⚠ BUG_M12_01) | **KHÔNG audit** (OBS-M12-01) |
| Sửa visa | `VisaDialog.Save` (edit) | `visas:update` | update `visas` | **KHÔNG audit** |
| Tạo vé | `FlightDialog.Save` (create) | `flights:create` | insert `flights`; `AssignedTo = first user` (⚠ BUG_M12_02) | **KHÔNG audit** |
| Sửa vé | `FlightDialog.Save` (edit) | `flights:update` | update `flights` | **KHÔNG audit** |

## 5. Database Impact

- **`visas`:** CandidateId, JobOrderId (không FK cascade khai báo rõ), VisaType, Country, Submitted/Interview/ResultDate, Status (VisaStatus), RejectionReason, Notes, **HandledBy** (Guid? → user handler; dùng cho notification). Không rowversion.
- **`flights`:** CandidateId, JobOrderId, Airline, TicketCode, Departure Date/Time, Airport đi/đến, DestinationCountry, **ActualDepartureAt** (DateTimeOffset? — xuất cảnh thực tế), **AssignedTo** (Guid?), Notes. Không rowversion.
- **State field:** `Visa.Status` set tự do qua dropdown (không state-machine). `Flight` không có status.

## 6. Roles & Permissions

| Action | Role (permission) | UI | API | Điều kiện | Source |
|---|---|---|---|---|---|
| Xem `/visa` | super_admin, VisaStaff, Director, DocumentStaff (visas:read) | `visas:read` | — | không scope (staff xem tất cả) | DbSeeder 41,74,79 |
| Tạo/sửa visa | super_admin, VisaStaff (visas:create/update) | AuthorizeView + Save re-check | — | — | DbSeeder 79 |
| Xem/tạo/sửa flight | super_admin, VisaStaff (flights:*); Director read | `flights:*` | — | — | DbSeeder 41,80 |
| Xóa visa/flight | — (không có chức năng) | — | — | — | — |

**Không có vai trò agent/self-scoped nào truy cập visa/flight** → **không có rủi ro IDOR/scope**; staff xem toàn bộ là đúng nghiệp vụ.

## 7. Risk Analysis

| Rủi ro | Đánh giá ở source | Kết luận |
|---|---|---|
| **Broken authorization** | Page `[Authorize(visas:read)]`; create AuthorizeView + Save re-check permission | ✅ Đúng |
| **IDOR / scope** | Không role scoped truy cập visa/flight | ✅ Không áp dụng |
| **First-user attribution** | `VisaDialog:136` `HandledBy` + `FlightDialog:128` `AssignedTo` = user đầu DB | ❌ **BUG_M12_01 / BUG_M12_02** |
| **Notification sai người** | Visa reminder (`NotificationService:291`) gửi tới `HandledBy` = first-user → **sai người nhận** | ❌ **BUG_M12_01 = Medium** (không chỉ cosmetic) |
| Flight AssignedTo dùng cho notification? | Departure reminder (312) dùng `CandidateOwnersOr(VisaStaff,Director)`, **không** dùng AssignedTo | → BUG_M12_02 chỉ cosmetic = **Low** |
| **Missing audit/history** | VisaDialog/FlightDialog Save không `AddAudit` | ⚠ **OBS-M12-01** (Low, thiếu lịch sử) |
| **Xuất cảnh thực tế không ghi được** | `ActualDepartureAt` chỉ set ở DemoDataSeeder; không đường runtime | ⚠ **OBS-M12-03** (report actual departure luôn rỗng cho dữ liệu thật) |
| **State transition visa** | Status set tự do (NotSubmitted→Approved nhảy cóc); không state-machine, không rowversion | ⚠ **OBS-M12-02** (Low) |
| **Duplicate visa/flight** | Không ràng buộc unique (Candidate,JobOrder); có thể tạo trùng | ⚠ **OBS-M12-04** (Low, concurrency) |
| **Timezone** | DateOnly cho ngày; ActualDepartureAt DateTimeOffset | ✅ Nhất quán |
| **Attribution edit** | HandledBy/AssignedTo chỉ set khi create, không đổi khi edit | Chấp nhận (creator=handler) |

## 8. Unknowns (Needs Requirement Clarification)

- **U-M12-1 (OBS-M12-03):** Xuất cảnh thực tế (`Flight.ActualDepartureAt`) được xác nhận ở đâu? Field tồn tại + có trong report + seed nhưng **không có UI runtime**. Có nên thêm nút "Xác nhận đã bay" ở FlightDialog/workflow bước Departure không? (Ảnh hưởng report M16 + reminder M13.)
- **U-M12-2 (OBS-M12-01):** Thay đổi visa/flight có cần ghi audit log không? (Các module khác đều audit; đây không.)
