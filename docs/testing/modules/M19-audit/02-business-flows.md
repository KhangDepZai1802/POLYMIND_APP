# M19 — Audit Log · Business Flows

## BF-M19-01 — Ghi audit khi thực hiện thao tác nghiệp vụ (write path)

- **Business Flow ID:** BF-M19-01
- **Flow name:** Ghi nhật ký kèm thao tác nghiệp vụ
- **Actor:** staff/partner đang thực hiện thao tác (create/update/delete/approve/...)
- **Role:** bất kỳ role có quyền thao tác tương ứng
- **Preconditions:** actor đã đăng nhập; thao tác nghiệp vụ hợp lệ
- **Initial state:** entity nghiệp vụ ở trạng thái trước thao tác
- **Input:** dữ liệu thao tác (VD sửa lead, duyệt hoa hồng)
- **Main flow:**
  1. Luồng nghiệp vụ áp thay đổi lên entity.
  2. Gọi `db.AddAudit(actorId, action, resource, resourceId, oldValue?, newValue?)` → thêm `AuditLog` vào change-tracker.
  3. `await db.SaveChangesAsync()` (CHUNG) → commit đồng thời thay đổi nghiệp vụ + bản ghi audit.
- **Alternate flow:** oldValue/newValue có thể null (create → old null; delete → new null).
- **Error flow:** nếu `SaveChanges` fail → cả thay đổi nghiệp vụ lẫn audit rollback (nguyên tử) → không có audit "mồ côi".
- **Validation:** không (audit là bản ghi phụ trợ).
- **Authorization:** không cần quyền riêng để ghi (side-effect của thao tác đã được authorize).
- **Database changes:** +1 hàng `audit_logs`.
- **Notification:** không.
- **Audit/history:** chính nó.
- **Final state:** entity đã đổi + audit đã lưu.
- **Page/API:** mọi trang có `AddAudit`.
- **Source reference:** `AuditLogHelpers.AddAudit`, ví dụ `LeadDetail.razor:356-364`, `AccountManagerPanel.razor:439-444`, `PaymentPostingService.cs:49`.
- **Risk:** [R3] mis-attribution (actor null → first-user); [R4] atomicity (đã đúng — cùng SaveChanges).
- **Unknown requirement:** —

### State machine
Audit là bản ghi **bất biến, không trạng thái** — không có state transition. Không sửa/không xóa sau khi ghi (append-only theo quy ước app).

| Current State | Action | Allowed Role | Condition | Next State | DB Change | Notification | History |
|---|---|---|---|---|---|---|---|
| (không tồn tại) | Insert audit | actor thao tác | kèm SaveChanges nghiệp vụ | (persisted, immutable) | +1 row | — | — |
| (persisted) | Update/Delete | ❌ không có đường app | — | (không đổi) | — | — | — |

## BF-M19-02 — Xem nhật ký thao tác (read path, admin)

- **Business Flow ID:** BF-M19-02
- **Flow name:** Xem & lọc nhật ký
- **Actor:** Giám đốc / super_admin
- **Role:** có `audit:read`
- **Preconditions:** đăng nhập; có `users:read` (vào `/admin`) + `audit:read` (tab)
- **Initial state:** —
- **Input:** filter resource/action (tùy chọn)
- **Main flow:**
  1. Vào `/admin` (gate `users:read`) → tab "Nhật ký thao tác" (gate `audit:read`).
  2. `LoadAuditAsync`: query `AuditLogs`, áp filter (Contains resource/action, có normalize VN→canonical), `OrderByDescending(CreatedAt).Take(200)`.
  3. Resolve `UserId → FullName` (null → "Hệ thống"; không tìm thấy → "—").
  4. Render bảng với nhãn VN, chip màu, mã kỹ thuật rút gọn.
- **Alternate flow:** không filter → 200 bản ghi mới nhất.
- **Error flow:** không có bản ghi khớp → bảng rỗng.
- **Validation:** filter là text tự do; normalize map một số từ khóa VN.
- **Authorization:** 2 lớp — page `[Authorize users:read]` + tab `AuthorizeView audit:read`. Role thiếu quyền → alert, KHÔNG load dữ liệu.
- **Database changes:** không (chỉ đọc).
- **Notification:** không.
- **Audit/history:** (không tự ghi việc xem — xem OBS-M19-02 nếu cần audit cả hành vi đọc).
- **Final state:** danh sách hiển thị.
- **Page/API:** `/admin` tab nhật ký; không REST.
- **Source reference:** `Admin.razor:127-180,316-337`.
- **Risk:** [R6] không IDOR (admin-only, global view); [R7] Take(200) giới hạn.
- **Unknown requirement:** U-M19-2 (phân trang/khoảng ngày/export).

## Kiểm tra các bẫy (checklist PHẦN D)

| Bẫy | Kết quả M19 |
|---|---|
| Trạng thái không thể đi tới | N/A (audit không có state) |
| Thao tác trái quyền (xem) | Chặn: `audit:read` chỉ Director+super_admin |
| Sửa/xóa dữ liệu đã ghi | Không có đường app sửa/xóa audit ✅ |
| Hai người ghi cùng lúc | Chỉ insert độc lập, không xung đột ✅ |
| Notification sai người | N/A |
| Lịch sử thiếu | [R4] audit + nghiệp vụ cùng SaveChanges → không mất; NHƯNG login/logout không ghi ([R2] OBS-M19-02) |
| Refresh/double-click tạo trùng | Có thể tạo 2 audit nếu thao tác nghiệp vụ chạy 2 lần — nhưng đó là hệ quả của guard ở module nghiệp vụ (đã kiểm ở M09/M10), không phải lỗi audit |
| Ghi đè dữ liệu | Audit không update → không ghi đè ✅ |
| Timezone | `CreatedAt` lưu UTC (DateTimeOffset), hiển thị `.LocalDateTime` ✅ |
