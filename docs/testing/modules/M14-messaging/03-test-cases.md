# M14 — Messaging / Chat · Test Cases

> Quy ước `TC_M14_<NNN>`. Layer: Unit (Message entity contract — chạy được); Manual/Integration (Web+DB/MinIO — pending harness). MessagingPolicy là pure static ở `Polymind.Web` → ma trận kiểm thủ công (blocker: test project không ref Web).

## MessagingPolicy — ma trận role (manual, source-verified)

| ID | Sender role | Recipient role | Expected `CanMessage` | Status |
|---|---|---|---|---|
| TC_M14_001 | bất kỳ | SuperAdmin | true (kênh hỗ trợ) | Pass (source ✅) |
| TC_M14_002 | SuperAdmin | Director | true | Pass (source ✅) |
| TC_M14_003 | Recruiter | Director | **false** (chỉ super) | Pass (source ✅) |
| TC_M14_004 | Accountant | Director | **false** | Pass (source ✅) |
| TC_M14_005 | Recruiter | Agent | true (recruitment role) | Pass (source ✅) |
| TC_M14_006 | RecruitmentManager | Collaborator | true | Pass (source ✅) |
| TC_M14_007 | Accountant | Agent | **false** (không phải recruitment) | Pass (source ✅) |
| TC_M14_008 | Agent | Agent | **false** | Pass (source ✅) |
| TC_M14_009 | Consultant | Recruiter | true (nội bộ) | Pass (source ✅) |
| TC_M14_010 | senderRoles rỗng | any | false | Pass (source ✅) |
| TC_M14_011 | any | recipientRoles rỗng | false | Pass (source ✅) |
| TC_M14_012 | Agent | Accountant | true (recipient nội bộ; U-M14-1 giữ hành vi partner→staff) | Pass (source ✅) |
| TC_M14_013 | Collaborator | Parent | Chỉ true khi Parent thuộc candidate có đúng `CollaboratorId`; ngoài quan hệ = false | **Pass (Codex relationship regression) — runtime pending** |

## Functional — danh bạ & hội thoại

| ID | Name | Flow | Expected | Layer | Status |
|---|---|---|---|---|---|
| TC_M14_014 | Danh bạ theo quyền | BF-M14-01 | Chỉ hiện người `CanMessage`; nếu một đầu là portal thì bắt buộc đúng quan hệ candidate; loại chính mình + user inactive | Unit/source + Manual | Pass unit/source; runtime Blocked |
| TC_M14_015 | Sắp xếp danh bạ | BF-M14-01 | unread desc → last desc → tên | Manual | Blocked |
| TC_M14_016 | Search theo tên | BF-M14-01 | Lọc `Name.Contains` (ci) | Manual | Blocked |
| TC_M14_017 | Xem hội thoại + mark read | BF-M14-02 | Nạp tin me↔other; tin gửi cho me → IsRead=true, ReadAt set; badge giảm | Manual | Blocked (source ✅) |
| TC_M14_018 | Không leak hội thoại người khác | BF-M14-02 | Chỉ tin me là participant | Manual | Blocked (source ✅) |
| TC_M14_019 | Attachment lỗi URL | BF-M14-02 | try/catch → ẩn ảnh, vẫn hiện tên file | Manual | Blocked (source ✅) |

## Gửi tin & re-check quyền

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_020 | Gửi text hợp lệ | Thêm Message IsRead=false; thread + danh bạ cập nhật | Manual | Blocked |
| TC_M14_021 | Gửi trống (no text/file) | Không gửi | Manual | Blocked (source ✅) |
| TC_M14_022 | Enter gửi, Shift+Enter xuống dòng | OnComposeKeyDown | Manual | Blocked |
| TC_M14_023 | Double-send | `_sending` guard chặn | Manual | Blocked (source ✅) |
| TC_M14_024 | Re-check server: gửi trái quyền | Bỏ qua UI, gọi Send tới recipient ngoài quyền → chặn + snackbar | Manual | Blocked (source ✅) |
| TC_M14_025 | Re-check relationship khi Send | Parent hoặc staff/partner gửi portal ngoài quan hệ → chặn bằng scope dựng lại từ DB | Unit/source + Manual | Pass unit/source; runtime Blocked |

## Thu hồi

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_026 | Thu hồi tin của mình | Xóa cứng; reload | Manual | Blocked (source ✅) |
| TC_M14_027 | Không thu hồi tin người khác | `SenderId==me` guard → không tìm thấy | Manual | Blocked (source ✅) |
| TC_M14_028 | Thu hồi không audit | OBS-M14-02: không ghi audit; attachment MinIO orphan | Manual | Blocked (obs) |

## Self-scoped quan hệ

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_029 | Parent allowed set | Agent + CTV + consultant/CJO/workflow/visa/flight assignee + con | Unit/source + Manual | Pass unit/source; runtime Blocked |
| TC_M14_030 | Student allowed set | Agent + CTV + consultant/CJO/workflow/visa/flight assignee + phụ huynh | Unit/source + Manual | Pass unit/source; runtime Blocked |
| TC_M14_031 | Self-scoped không thấy super admin | super không nằm allowed set | Manual | Blocked (source ✅) |
| TC_M14_032 | Self-scoped không ứng viên/quan hệ | allowed rỗng → không nhắn ai | Manual | Blocked (source ✅) |

## File upload / security

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_033 | Upload định dạng hợp lệ | pdf/ảnh/office/audio → ok | Manual | Blocked |
| TC_M14_034 | Upload extension lạ (.exe) | Chặn (`InvalidOperationException`) | Manual | Blocked (source ✅) |
| TC_M14_035 | Upload quá lớn | Chặn theo MaxUploadBytes | Manual | Blocked (source ✅) |
| TC_M14_036 | Body/filename XSS | render encode → không thực thi | Manual | Blocked (source ✅) |
| TC_M14_037 | Tin cũ không JSON | ParseMessageBody → hiển thị nguyên văn | Manual | Blocked (source ✅) |

## DB / concurrency

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_038 | Gửi cho user vừa bị khóa | OBS-M14-03: Send không re-check IsActive → vẫn gửi | Manual | Blocked (obs) |
| TC_M14_039 | Xóa user → orphan message | OBS-M14-05: no FK → message orphan | Manual | Blocked (obs) |
| TC_M14_040 | Hội thoại dài | OBS-M14-04: nạp toàn bộ (no paging) | Manual | Blocked (obs) |

## Contract (Unit — chạy được)

| ID | Name | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M14_041 | Message default | IsRead=false, ReadAt null | Unit | Pass |
| TC_M14_042 | Message body required (non-null) | Body = default! (khởi tạo qua Send) | Unit | Pass |
| TC_M14_043 | Staff phụ trách → portal candidate | Chỉ Parent/Student của candidate mình phụ trách | Unit | Pass |
| TC_M14_044 | User không liên quan fail-closed | Không có portal recipient | Unit | Pass |
| TC_M14_045 | Portal reply đối xứng | Parent/Student trả lời đúng Agent/CTV/staff phụ trách + nhau | Unit | Pass |
| TC_M14_046 | Scope staff dịch PostgreSQL | Query có WHERE/EXISTS, lọc server-side | Unit/EF translation | Pass |
