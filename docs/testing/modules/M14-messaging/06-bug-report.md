# M14 — Messaging / Chat · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M14_<NN>`.

## Kết luận: **No Confirmed Bugs** (Verified code-level)

Các thuộc tính bảo mật/nghiệp vụ lõi đúng ở source:

- **IDOR đọc hội thoại — đóng:** `LoadThread` chỉ nạp tin `me↔other` (me là sender/recipient); `SelectContact` chỉ nhận id từ danh bạ đã render server-side.
- **Broken authz gửi — đóng:** `Send` **re-check server** cả hai nhánh (self-scoped `allowed.Contains` / `MessagingPolicy.CanMessage`), không tin UI.
- **Recall — đóng:** chỉ xóa tin `SenderId==me`.
- **File upload — đóng:** validate size ≤ MaxUploadBytes + extension whitelist server-side; `SanitizeFileName`.
- **XSS — đóng:** Text/FileName render qua Blazor auto-encode; ảnh/audio từ presigned URL.
- **Self-scoped quan hệ — đúng:** `BuildAllowedRecipientsAsync` resolve đúng FK (`Collaborator.UserId`, `Candidate.ConsultantId`=user, `OwnerUserId`/`ParentUserId`); parent/student không thấy super admin.
- **Permission — đúng:** `Messaging()` cấp `messages:read`/`messages:create`, khớp policy trang.

---

## Observations (theo dõi — không handoff Codex trừ khi user chốt)

- **OBS-M14-01 / CR-M14-1 — RESOLVED by Codex, chờ Claude:** self-scoping trước đây chỉ áp phía phụ huynh/học viên. Phía nhân sự (kể cả **CTV/Đại lý**) dùng `MessagingPolicy`:
  - (a) **Nhân sự/CTV/Đại lý có thể nhắn BẤT KỲ phụ huynh/học viên** toàn hệ thống (`CanMessage(staff, parent)` → nhánh "nội bộ" trả true), **không** giới hạn theo ứng viên mình phụ trách. → có thể lộ danh bạ phụ huynh/học viên cho CTV/Đại lý không liên quan.
  - (b) **Đại lý/CTV khởi tạo được** tin tới kế toán/visa/hồ sơ (recipient "nội bộ" → true), nhưng chiều ngược bị chặn (`CanMessage(accountant, agent)` → false) → **thread một chiều**.
  - **Đã sửa theo U-M14-1:** nếu một đầu là parent/student, bắt buộc cùng candidate relationship; áp danh bạ + Send re-check DB. Partner→staff giữ nguyên như quyết định mặc định.
- **OBS-M14-02 — Recall xóa cứng, không audit** (Low): `RecallMessage` `db.Messages.Remove` + không `AddAudit`; attachment MinIO không dọn theo → orphan object. Các mutation khác đều audit.
- **OBS-M14-03 — Send không re-check `IsActive` người nhận** (Low): danh bạ lọc `IsActive` nhưng `Send` re-check quyền theo role/allowed, không kiểm recipient còn active → circuit cũ có thể gửi cho user vừa bị khóa.
- **OBS-M14-04 — Không phân trang** (Low, perf): `LoadThread` nạp toàn bộ tin hội thoại; `LoadContacts` nạp toàn bộ message liên quan me → nặng với lịch sử dài.
- **OBS-M14-05 — Message không FK Users + presigned URL không kiểm ownership object** (Low): `SenderId`/`RecipientId` là Guid trần (no FK) → orphan message khi xóa user. `GetDownloadUrlAsync(objectKey)` cấp URL cho object bất kỳ — **an toàn trong M14** (objectKey lấy từ thread của mình) nhưng cần chú ý khi QA M18 Documents.

---

## Codex Handoff Queue

| Order | Bug ID | Severity | Status |
|---:|---|---|---|
| 1 | **CR-M14-1** | Change | **✅ Verified Fixed (code) — Claude phiên #8:** relationship đối xứng fail-closed, danh bạ + Send re-check DB server-side; M14 7/7, suite 122/122, Web 0/0. Xem `08-verification-report.md`. |

### CR-M14-1 — Giới hạn danh bạ/khởi tạo tin nhắn theo quan hệ ứng viên
- **Nguồn:** OBS-M14-01 (đã user chốt U-M14-1).
- **Hiện trạng:** `MessagingPolicy.CanMessage:33-34` cho nhánh "nội bộ" trả true → staff/CTV/đại lý nhắn được BẤT KỲ phụ huynh/học viên toàn hệ thống; `Messages.razor LoadContacts:226-230` liệt kê toàn bộ.
- **Hướng cho Codex:** khi người gửi là staff/CTV/đại lý và người nhận là parent/student → chỉ cho phép nếu parent/student đó thuộc **ứng viên mà người gửi phụ trách** (consultant/recruiter/document/... theo assignment; CTV theo `CollaboratorId`; đại lý theo `AgentId`). Áp cả `LoadContacts` (ẩn ngoài phạm vi) lẫn `Send` (re-check server). Quyết định phụ: CTV/đại lý có được chủ động nhắn nhân sự nội bộ khác không (đang một chiều) — mặc định giữ nguyên trừ khi user nói khác.
- **Required Files:** `Web/Messaging/MessagingPolicy.cs`, `Components/Pages/Messages/Messages.razor` (`LoadContacts`, `Send`, `BuildAllowedRecipientsAsync`).

### Trạng thái sau sửa

- Staff candidate scope: `ConsultantId`, CJO/Workflow `AssignedTo`, Visa `HandledBy`, Flight `AssignedTo`.
- Agent scope: `Candidate.AgentId`; CTV: `Candidate.CollaboratorId` trực tiếp.
- Portal reply được Agent/CTV/staff phụ trách + tài khoản parent/student còn lại của cùng candidate.
- Thiếu mapping/không liên quan → rỗng (fail-closed).
- **Status:** ✅ **Verified Fixed (code-level) — Claude phiên #8 (2026-07-11).** Xem `08-verification-report.md`.

> **Ghi chú:** M14 → `QA Status = No Confirmed Bugs` (CR nghiệp vụ), `Codex Status = Fixed`, `Verification Status = Verified (code)` cho CR-M14-1 (Claude phiên #8). OBS-M14-02..05 vẫn là backlog.

---

## CR-M14-2 — Thu hẹp danh bạ Phụ huynh/Học viên: CHỈ CTV + TVV + người nhà

- **Nguồn:** user chốt 2026-07-13 (phiên #9) — báo trang tin nhắn của tài khoản Học viên/Phụ huynh đang sai phạm vi.
- **Loại:** Change (thu hẹp CR-M14-1) · **Severity:** Medium (lộ kênh liên lạc ngoài phạm vi cho tài khoản portal)
- **Business Flow ID:** BF-M14-01 · **Test Case ID:** TC_M14_043..047

### Quy tắc user chốt

| Tài khoản | Được nhắn với |
|---|---|
| **Học viên** | CTV (`Candidate.CollaboratorId`), TVV (`Candidate.ConsultantId`), **phụ huynh** của mình |
| **Phụ huynh** | CTV, TVV, **học viên** của mình |

Đối xứng: chiều ngược lại **chỉ CTV/TVV của đúng ứng viên đó** mới thấy/nhắn được Phụ huynh/Học viên.

### Hiện trạng trước sửa (root cause)

CR-M14-1 định nghĩa "người phụ trách" quá rộng. `Messages.BuildRelationshipRecipientsAsync` gom participants =
`ConsultantId` **+ `Agent.UserId` + `Collaborator.UserId` + `CandidateJobOrder.AssignedTo` + `WorkflowStepRecord.AssignedTo` + `Visa.HandledBy` + `Flight.AssignedTo`**.
`CandidateMessagingRelationship.AllowedRecipientsFor(portalUser)` trả `portalUsers ∪ responsibleUsers` → Học viên/Phụ huynh thấy **cả đại lý và toàn bộ nhân sự hồ sơ/visa/workflow**, vi phạm quy tắc trên.

### Files Changed

| File | Thay đổi |
|---|---|
| `src/Polymind.Domain/Messaging/CandidateMessagingRelationship.cs` | `MessagingCandidateScope.ForResponsibleUser` → **`ForConsultant`** (bỏ quét visa/flight/step). Thêm factory **`CandidateMessagingRelationship.ForCandidate(student, parent, consultant, collaborator)`** — khóa quan hệ đúng 2 đối tác. Đổi tên `_responsibleUsers` → `_counterparts`. |
| `src/Polymind.Web/Components/Pages/Messages/Messages.razor` | `BuildRelationshipRecipientsAsync`: participants chỉ còn `{ConsultantId, Collaborator.UserId}`; **đại lý (`IsAgentOnly`) trả rỗng fail-closed**; bỏ 4 query assignee (CJO/step/visa/flight) + query `db.Agents`. |
| `tests/Polymind.Tests/M14_MessagingRulesTests.cs` | +5 regression CR-M14-2 (student/parent scope, agent+visa staff loại trừ, TVV/CTV chỉ tới portal, no-collaborator giữ chặt); SQL translation đổi sang `ForConsultant`. |

### Ảnh hưởng đối xứng (có chủ đích)

- **Đại lý** không còn nhắn được Phụ huynh/Học viên (trước có, nếu ứng viên thuộc đại lý đó).
- **Nhân viên hồ sơ/visa + người được giao bước workflow** không còn nhắn được Phụ huynh/Học viên **trừ khi họ là TVV** của ứng viên.
- `Send` re-check server-side dùng chung `BuildRelationshipRecipientsAsync` → guard áp cả UI lẫn mutation (không chỉ ẩn danh bạ).
- Messaging staff↔staff và CTV/đại lý↔nhân sự (qua `MessagingPolicy.CanMessage`) **không đổi**.

### Tests Run

| Test | Type | Result |
|---|---|---|
| `M14_MessagingRulesTests` (10) | Unit/Domain + SQL translation | ✅ Pass |
| Full suite | Unit | ✅ **141/141** (Failed 0, Skipped 0) |
| `Polymind.Web` build | Compile | ✅ **0 Warning / 0 Error** |

### Verification Instructions

- Đăng nhập **Học viên** → `/messages`: danh bạ chỉ gồm CTV + TVV + phụ huynh. Không thấy đại lý/NV visa/NV hồ sơ.
- Đăng nhập **Phụ huynh** → `/messages`: chỉ CTV + TVV + học viên.
- Đăng nhập **đại lý** → không thấy phụ huynh/học viên trong danh bạ; `Send` tới họ bị chặn server-side.
- Ứng viên **chưa gắn CTV** → học viên chỉ còn TVV + phụ huynh (không nới rộng).
- Ứng viên chưa gắn portal account → cả hai chiều rỗng (fail-closed).

- **Status:** **Fixed — chờ xác minh độc lập.**
- **Runtime gap:** chưa chạy E2E Blazor/PostgreSQL đa tài khoản (chưa có harness) — verify hiện ở mức code + unit.

---

## CR-M14-3 — Ma trận phân bậc: chấm dứt "nhắn loạn xạ"

- **Nguồn:** user chốt 2026-07-13 (phiên #9).
- **Loại:** Change (bao trùm CR-M14-1/2) · **Severity:** **High** (mọi cặp nhân sự nội bộ đang nhắn được nhau vô tội vạ)
- **📖 LUẬT GỐC:** [`docs/messaging-tiers.md`](../../../messaging-tiers.md) — mô hình 5 bậc + ma trận đầy đủ 12×12. **Đọc file đó trước khi sửa luật.**

### Root cause

`MessagingPolicy.CanMessage` chỉ có vài luật rời rạc (chặn Giám đốc, chặn Đại lý/CTV) rồi kết thúc bằng **`return true`** — tức **mặc định MỞ** cho mọi cặp nhân sự nội bộ không khớp luật nào. Bậc 5 đã siết bằng quan hệ ứng viên (CR-M14-1/2), nhưng bậc 2–4 thả nổi hoàn toàn.

### Mô hình 5 bậc (user chốt)

| Bậc | Role |
|:---:|---|
| 1 | `super_admin` |
| 2 | `director` |
| 3 | `accountant`, `recruitment_manager`, `document_staff`, `visa_staff` |
| 4 | `consultant`, `recruiter`, `agent` |
| 5 | `parent`, `student`, `collaborator` |

**Bốn mệnh đề:** (1) Super Admin hai chiều với tất cả · (2) chênh bậc ≤ 1 thì được nhắn · (3) ba ngoại lệ chặn: TVV✗TVV, CTV✗CTV, **Đại lý ✗ toàn bộ bậc 4** (đối thủ + đối tác ngoài) · (4) tầng quan hệ ứng viên siết thêm lên trên ma trận.

### Files Changed

| File | Thay đổi |
|---|---|
| `src/Polymind.Domain/Messaging/MessagingTiers.cs` | **MỚI.** Ma trận thuần ở Domain → **unit-test được** (đóng blocker QA "ma trận role kiểm thủ công"). `TierByRole` + `CanMessage` fail-closed. |
| `src/Polymind.Web/Identity/MessagingPolicy.cs` | Xóa chuỗi if + **fallback `return true`**; ủy quyền cho `MessagingTiers.CanMessage`. Thêm `IsSuperAdmin`, `PrimaryRole`. |
| `Components/Pages/Messages/Messages.razor` | Gom luật vào `IsAllowedRecipient` (dùng chung danh bạ + `Send`). Bậc 5 nạp thêm Super Admin vào danh sách đóng. **Bộ lọc theo vai trò** ở ô tìm kiếm (hiện khi danh bạ có ≥2 role). |
| `tests/Polymind.Tests/M14_MessagingMatrixTests.cs` | **MỚI.** 56 case phủ ma trận + fail-closed + đa-role + **chống lệch tên role** giữa Domain và `RoleNames`. |
| `docs/messaging-tiers.md` | **MỚI.** Tài liệu gốc của luật. |

### Ảnh hưởng (siết quyền — có chủ đích)

Các cặp **mất** khả năng nhắn nhau: Giám đốc ✗ TVV/NVTD/Đại lý · Giám đốc ✗ bậc 5 · Kế toán/Hồ sơ/Visa ✗ bậc 5 · Đại lý ✗ TVV/NVTD/Đại lý khác · TVV ✗ TVV.
Các cặp **mới được** nhắn: Đại lý ↔ CTV của mình · bậc 5 ↔ Super Admin (trước không trả lời được SA).
**Không xóa dữ liệu** — tin nhắn cũ vẫn nằm trong DB, chỉ không hiện danh bạ và không gửi mới được.

### Tests Run

| Test | Result |
|---|---|
| `M14_MessagingMatrixTests` (56) | ✅ Pass |
| `M14_MessagingRulesTests` (10, tầng quan hệ) | ✅ Pass — giữ nguyên |
| Full suite | ✅ **208/208** (Failed 0, Skipped 0) |
| `Polymind.Web` build | ✅ **0 Warning / 0 Error** |

### Verification Instructions

Đối chiếu danh bạ `/messages` từng role với bảng ở `docs/messaging-tiers.md`:
- **Super Admin** → thấy tất cả + có dropdown "Lọc theo vai trò".
- **Giám đốc** → chỉ bậc 3 + SA (KHÔNG thấy TVV/NVTD/Đại lý/portal).
- **Kế toán** → Giám đốc + bậc 3 + TVV/NVTD/Đại lý + SA (KHÔNG thấy học viên/phụ huynh).
- **TVV** → bậc 3 + NVTD + học viên/phụ huynh mình phụ trách + SA (KHÔNG thấy TVV khác, KHÔNG thấy Đại lý).
- **Đại lý** → bậc 3 + CTV của mình + SA (KHÔNG thấy đại lý khác/TVV/NVTD).
- **CTV** → đại lý chủ quản + học viên/phụ huynh mình giới thiệu + SA (KHÔNG thấy CTV khác).
- **Gửi lậu:** `Send` re-check server-side cùng hàm `IsAllowedRecipient` → không thể bypass bằng cách giữ `_selectedId` cũ.

- **Status:** **Fixed — chờ xác minh độc lập.**
- **Runtime gap:** chưa chạy E2E đa tài khoản trên Blazor/PostgreSQL (chưa có harness).
