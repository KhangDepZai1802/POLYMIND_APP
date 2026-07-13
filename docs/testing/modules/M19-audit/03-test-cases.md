# M19 — Audit Log · Test Cases

> Quy ước `TC_M19_<NNN>`. Nguồn: `01-analysis.md`, `02-business-flows.md`. Nhiều TC là **manual/static** (logic ghi audit + trang nằm ở `Polymind.Web`; test project không ref Web → chỉ contract test entity ở `Polymind.Domain`).

## Nhóm A — Authorization (xem nhật ký)

| TC | Tên | Role | Steps | Expected | Layer | Status |
|---|---|---|---|---|---|---|
| TC_M19_001 | Director xem được nhật ký | Director | Vào `/admin` → tab Nhật ký | Tab hiện, load 200 dòng mới nhất | Manual/E2E | Static-Pass (seed `audit:read`) |
| TC_M19_002 | super_admin xem được | SuperAdmin | như trên | Tab hiện, load dữ liệu | Manual/E2E | Static-Pass (all perms) |
| TC_M19_003 | Accountant KHÔNG xem được nhật ký | Accountant | Vào `/admin`? | `/admin` chặn (không `users:read`) → không tới tab | Manual/E2E | Static-Pass (seed không cấp) |
| TC_M19_004 | RM KHÔNG xem được nhật ký | RecruitmentManager | Vào `/admin` | Không `users:read` → chặn trang | Manual/E2E | Static-Pass |
| TC_M19_005 | Truy cập tab audit khi thiếu `audit:read` | (giả định role có `users:read` nhưng không `audit:read`) | mở tab | `AuthorizeView` → alert "Bạn không có quyền xem nhật ký thao tác."; KHÔNG query DB | Static | Pass (source `Admin.razor:176-178`) |
| TC_M19_006 | Không có REST API audit → không IDOR REST | agent/parent | thử gọi endpoint | Không tồn tại endpoint audit | Static | Pass (không map API) |

## Nhóm B — Ghi audit (write path) & atomicity

| TC | Tên | Steps | Expected DB | Layer | Status |
|---|---|---|---|---|---|
| TC_M19_010 | Create ghi audit action=create, old=null | Tạo lead/candidate | +1 audit `action=create`, `old_value=null`, `new_value=snapshot` | Static/E2E | Static-Pass (`LeadDialog:239`) |
| TC_M19_011 | Update ghi old+new | Sửa lead | audit `update` với cả old & new | Static/E2E | Static-Pass (`LeadDialog:225`) |
| TC_M19_012 | Delete ghi old, new=null, sống sót sau khi xóa entity | Xóa lead | audit `delete` vẫn còn dù `leads` row bị xóa (không FK) | Static/E2E | Static-Pass (`LeadDetail:356-364`, ResourceId không FK) |
| TC_M19_013 | Audit + thay đổi nghiệp vụ nguyên tử (cùng SaveChanges) | Sửa payment | Nếu SaveChanges fail → KHÔNG có audit mồ côi | Static | Pass (`PaymentPostingService:49` cùng save) |
| TC_M19_014 | Approve/mark_paid hoa hồng ghi audit | Duyệt/chi hoa hồng | audit `approve`/`mark_paid` resource=agent_commissions | Static/E2E | Static-Pass (`AgentDetail:385,425`) |
| TC_M19_015 | Đổi vai trò ghi audit update_role | Đổi role user | audit `update_role` old=roles cũ, new=role mới | Static/E2E | Static-Pass (`AccountManagerPanel:365`) |
| TC_M19_016 | Khóa/mở tài khoản ghi lock/unlock | Toggle IsActive | audit `lock`/`unlock` | Static/E2E | Static-Pass (`AccountManagerPanel:386`) |
| TC_M19_017 | Actor được ghi đúng người thao tác (không first-user) | Thao tác bởi user X | `user_id = X` | Static/E2E | Static-Pass (các module dùng `GetRequiredUserIdAsync` = actor thật sau fix M04/M06/M12) |
| TC_M19_018 | Đổi mật khẩu KHÔNG log mật khẩu thật | Đổi password | audit chỉ `{PasswordChanged=true}`, không có plaintext | Static | Pass (`ChangePasswordDialog:84`) |

## Nhóm C — Hiển thị & filter

| TC | Tên | Steps | Expected | Layer | Status |
|---|---|---|---|---|---|
| TC_M19_020 | Sort mới nhất trước | mở tab | `OrderByDescending(CreatedAt)` | Static | Pass (`Admin.razor:327`) |
| TC_M19_021 | Giới hạn 200 dòng | >200 log | chỉ 200 mới nhất | Static | Pass (`Take(200)`) — OBS-M19-04 log cũ không xem được |
| TC_M19_022 | Filter theo khu vực (VN → canonical) | gõ "ứng viên" | map "candidates", lọc Resource | Static | Pass (`NormalizeAuditFilter`) |
| TC_M19_023 | Filter theo thao tác | gõ "duyệt" | map "approve", lọc Action | Static | Pass |
| TC_M19_024 | UserId null hiển thị "Hệ thống" | log hệ thống | cột người = "Hệ thống" | Static | Pass (`Admin.razor:331`) |
| TC_M19_025 | UserId đã xóa hiển thị "—" | user bị xóa | cột người = "—" | Static | Pass (GetValueOrDefault) |
| TC_M19_026 | Timezone hiển thị local | log UTC | `.LocalDateTime` dd/MM/yyyy HH:mm | Static | Pass |
| TC_M19_027 | Mã kỹ thuật rút gọn 8 ký tự | có ResourceId | hiện 8 hex đầu | Static | Pass (`ShortTechnicalId`) |
| TC_M19_028 | Action không có nhãn → humanize | action lạ (`change_password`) | "Change Password" (fallback) | Static | Pass — OBS-M19-06 label chưa phủ hết |

## Nhóm D — Entity contract (automated)

| TC | Tên | Expected | Layer | Status |
|---|---|---|---|---|
| TC_M19_030 | AuditLog mới: UserId/ResourceId/Old/New/Ip/UserAgent = null | mặc định null (Ip/UA KHÔNG tự set → OBS-M19-01) | Unit | **Automated** (`M19_AuditLogTests`) |
| TC_M19_031 | AuditLog có Id + CreatedAt tự sinh | Id ≠ empty; CreatedAt > default | Unit | **Automated** |
| TC_M19_032 | AddAudit KHÔNG có tham số Ip/UserAgent (chữ ký) | helper không set 2 field → luôn null | Static | Pass (`AuditLogHelpers:36-54`) — OBS-M19-01 |

## Nhóm E — Negative / requirement gaps (observations)

| TC | Tên | Expected (theo entity doc) | Actual | Kết luận |
|---|---|---|---|---|
| TC_M19_040 | Login được ghi audit | entity doc: "CRUD/đăng nhập" | Login.razor/AuthEndpoints KHÔNG AddAudit | **OBS-M19-02 / U-M19-1** (gap requirement, chờ user chốt) |
| TC_M19_041 | Logout được ghi audit | (suy từ doc) | không ghi | **OBS-M19-02 / U-M19-1** |
| TC_M19_042 | IpAddress/UserAgent lưu cho forensic | entity có 2 cột | luôn null | **OBS-M19-01 / U-M19-1** |
| TC_M19_043 | Actor null KHÔNG bị gán first-user | audit nên null/throw | `GetRequiredUserIdAsync` fallback first-user | **OBS-M19-03** (integrity risk, hiếm trigger vì trang [Authorize]) |
| TC_M19_044 | Audit bất biến (không sửa/xóa qua app) | append-only | đúng ở tầng app; không enforce DB | **OBS-M19-05** (Low) |

## Tổng hợp
- **Automated:** TC_M19_030/031 (+ contract 032 static).
- **Static-Pass:** phần lớn (authz, atomicity, hiển thị) đối chiếu source.
- **Observation/requirement:** TC_M19_040-044 → OBS-M19-01..06, U-M19-1/2.
- **Blocked (pending harness):** E2E write→view thật, filter runtime, actor-null runtime → cần WebApplicationFactory + DB test.
