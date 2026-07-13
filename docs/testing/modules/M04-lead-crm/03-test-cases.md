# M04 — Lead CRM · Test Cases

Quy ước ID: `TC_M04_<NNN>`. Automation Layer: **Unit** (LeadCareRules/BusinessRoleAccess — logic thuần nhưng nằm ở `Polymind.Web` → hiện **blocked** vì test không ref được Web khi dev server chạy), **Integration** (API/DB — chưa harness), **Manual** (UI).

| TC | Tên | BF | Nguồn | Loại | Prio | Sev nếu fail | Role | Preconditions | Test Data | Expected | Automation | Layer | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| TC_M04_001 | Tạo Lead hợp lệ | BF-01 | LeadDialog | Functional | P1 | High | recruiter | `/leads` | tên+SĐT | tạo OK, Code auto, Status New | Manual | Manual | Not Run |
| TC_M04_002 | Tạo Lead thiếu tên | BF-01 | LeadDialog/API:62 | Negative | P2 | Medium | recruiter | — | tên rỗng | chặn/400 | Manual/Integration | Manual | Not Run |
| TC_M04_003 | Đổi trạng thái + ghi lịch sử | BF-02 | LeadDetail:460 | Functional | P1 | High | recruiter | lead New | New→Contacted | status đổi + LeadActivity StatusChange | Manual | Manual | Not Run |
| TC_M04_004 | Không chọn "Đã chuyển" thủ công | BF-02 | LeadDetail:147 | Functional | P2 | Medium | recruiter | lead | dropdown | không có option Converted | Manual | Manual | Not Run |
| TC_M04_005 | Phân công TVV | BF-03 | :378 | Functional | P2 | Medium | recruiter | lead | chọn consultant | AssignedTo lưu + activity | Manual | Manual | Not Run |
| TC_M04_006 | Lịch hẹn quá khứ bị chặn | BF-04 | :436 | Boundary | P2 | Medium | recruiter | lead | ngày hôm qua | "không thể đặt... đã qua" | Manual | Manual | Not Run |
| TC_M04_007 | Convert tạo ứng viên | BF-05 | :567 | Functional | P1 | Critical | recruiter | lead chưa convert | Convert | tạo candidate + status Converted + điều hướng | Manual | Manual | Not Run |
| TC_M04_008 | **Convert gán CreatedBy sai** | BF-05 | :597,618 | Database | P2 | Low | recruiter | lead | Convert | **kỳ vọng CreatedBy=actor; thực tế=user đầu tiên** | Manual/Integration | Integration | **Fail → BUG_M04_01** |
| TC_M04_009 | Convert chống trùng | BF-05 | :589 | Functional | P1 | High | recruiter | lead đã có candidate | Convert lại | không tạo trùng, điều hướng candidate cũ | Manual | Manual | Not Run |
| TC_M04_010 | Convert race 2 request | BF-05 | :589 | Concurrency | P3 | Medium | recruiter | lead | 2 request đồng thời | **kỳ vọng 1 candidate; rủi ro 2** | Integration | Integration | Blocked |
| TC_M04_011 | Revert khi ứng viên chưa có dữ liệu | BF-06 | :520 | Functional | P2 | Medium | recruiter | candidate rỗng | Revert | xóa candidate + về Lead | Manual | Manual | Not Run |
| TC_M04_012 | Revert bị chặn khi có dữ liệu | BF-06 | :527 | Functional | P1 | High | recruiter | candidate có payment | Revert | chặn "đã phát sinh dữ liệu" | Manual | Manual | Not Run |
| TC_M04_013 | Xóa Lead dọn liên kết | BF-07 | :345-364 | Functional | P2 | Medium | RM | lead có candidate+activities | Xóa | candidate.lead_id=null, activities/notif xóa, audit | Manual | Manual | Not Run |
| TC_M04_014 | Xóa Lead — role không đủ | BF-07 | BusinessRoleAccess:15 | Security | P1 | High | consultant | lead | Xóa | chặn (CanDeleteLead false) | Manual | Manual | Not Run |
| TC_M04_015 | API GET phân trang/tìm kiếm | BF-01 | LeadsEndpoints:18 | Functional | P2 | Medium | staff (JWT) | token | `/api/leads?search=` | PagedResult | Integration | Integration | Blocked |
| TC_M04_016 | API thiếu leads:read → 403 | BF- | ApiAuth | Security | P1 | High | agent (JWT) | token (không leads:read) | `/api/leads` | 403 | Integration | Integration | Blocked |
| TC_M04_017 | LeadCareRules ThresholdHours đúng | BF-08 | LeadCareRules:16 | Unit | P1 | High | — | — | mỗi status | ngưỡng đúng (New=24…) | Unit | Unit | **Blocked (Web ref lock)** |
| TC_M04_018 | LeadCareRules Appointment tính từ giờ hẹn | BF-08 | :55 | Unit | P1 | High | — | — | Appointment + appt tương lai | không overdue tới qua giờ hẹn | Unit | Unit | **Blocked (Web ref lock)** |
| TC_M04_019 | LeadCareRules trạng thái kết thúc không nhắc | BF-08 | :25 | Unit | P2 | Medium | — | — | Converted/Cancelled | Threshold null → không overdue | Unit | Unit | **Blocked (Web ref lock)** |
| TC_M04_020 | DurationLabel giờ/ngày | BF-08 | :67 | Unit | P3 | Low | — | — | 5/48/72 | "5 giờ"/"2 ngày"/"3 ngày" | Unit | Unit | **Blocked (Web ref lock)** |
| TC_M04_021 | Chuông quá hạn hiện ở list | BF-08 | Leads:164 | UI | P2 | Medium | staff | lead quá hạn | mở /leads | chuông đỏ + tooltip | Manual | Manual | Not Run |
| TC_M04_022 | Tìm kiếm/lọc list | BF-01 | Leads:203 | Functional | P2 | Medium | staff | nhiều lead | gõ tên/lọc status/source | lọc đúng | Manual | Manual | Not Run |
| TC_M04_023 | Lead đã convert ẩn khỏi list chính | BF-05 | Leads:222 | Functional | P2 | Medium | staff | có lead converted | mở /leads | không hiện; ở /leads/converted | Manual | Manual | Not Run |

## Gap analysis
- **Unit (blocked):** `LeadCareRules` (TC_017–020) và `BusinessRoleAccess` là logic thuần LÝ TƯỞNG để unit test, nhưng nằm trong `Polymind.Web` → test project không ref được khi dev server `:5177` khóa DLL. **Đề xuất:** tách 2 lớp này (hoặc phần rule) sang `Polymind.Domain`/`Polymind.Application` để unit test → sẽ tự động hóa ngay được ~4-6 test.
- **Integration (blocked):** API `/api/leads` CRUD + 401/403, convert race, CreatedBy (BUG_M04_01) — cần harness.
- **Manual:** CRUD UI, convert/revert, xóa, phân công, lịch hẹn, tìm kiếm/lọc, chuông quá hạn.
- **Bug:** BUG_M04_01 (Low).
