# M19 — Audit Log · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M19_<NN>`.

## Kết luận: **NO CONFIRMED BUGS**

Module Audit Log đúng ở các điểm cốt lõi (đối chiếu source):
- **Authorization xem nhật ký 2 lớp:** trang `/admin` gate `users:read`; tab Nhật ký gate `audit:read`. `audit:read` chỉ cấp **Director + super_admin** (seed). Role khác → alert, KHÔNG query DB. → không rò rỉ, không IDOR (không có REST audit; view admin-only toàn cục nên không cần data-scope).
- **Atomicity ghi:** `AddAudit` chỉ Add vào change-tracker; audit + thay đổi nghiệp vụ commit CHUNG một `SaveChanges` (cùng DbContext) → nguyên tử, không audit mồ côi. Đối chiếu `LeadDetail.Delete`, `AccountManagerPanel.AddAuditAsync`, `PaymentPostingService`.
- **Attribution actor thật:** call sites dùng `GetRequiredUserIdAsync`/`GetUserIdAsync` = người thao tác (sau các fix M04/M06/M12); `UserId=null` hiển thị "Hệ thống".
- **Bất biến:** không UI/endpoint/code nào sửa hoặc xóa `audit_logs` → append-only theo quy ước app.
- **Không FK cứng** trên `UserId`/`ResourceId` → audit sống sót khi xóa user/resource (đúng bản chất nhật ký).
- **Không log secret:** đổi mật khẩu chỉ log `{PasswordChanged=true}`.
- **Index** `(Resource,ResourceId)` + `(UserId,CreatedAt)` hỗ trợ filter/sort.

Không phát hiện: broken authorization, IDOR, mất lịch sử do transaction tách rời, ghi đè/xóa audit, log dữ liệu nhạy cảm, timezone sai.

---

## Observations (theo dõi — KHÔNG handoff trừ khi user chốt)

- **OBS-M19-01 — IpAddress/UserAgent không bao giờ được ghi (Low, requirement):** Entity `AuditLog` có 2 cột `IpAddress`/`UserAgent` (mục đích forensic) nhưng `AddAudit` không nhận/không set, không call site nào set → 2 cột **luôn NULL**. → **U-M19-1** (có cần lưu IP/UA?).
- **OBS-M19-02 — Login/Logout không ghi audit (Low, requirement):** XML doc entity ghi "mọi thao tác CRUD/**đăng nhập**", nhưng `Login.razor` (web) và `AuthEndpoints` (`/api/auth/login`) chỉ set `LastLoginAt`, KHÔNG `AddAudit`; không action `login`/`logout` ở bất kỳ đâu. → **U-M19-1** (audit sự kiện đăng nhập/đăng xuất?). *Lưu ý:* user từng chốt "KHÔNG thêm nhóm Tài khoản/Bảo mật" cho **notification** (RB-7) — nhưng đó là thông báo, không phải audit; cần chốt riêng.
- **OBS-M19-03 — Fallback first-user gây mis-attribution audit (Low-Med tùy quan điểm, hiếm trigger):** `GetRequiredUserIdAsync` khi actor null → `db.Users.Select(u=>u.Id).FirstAsync()` → audit có thể gán thao tác cho **user đầu DB** thay vì null/throw. Với nhật ký kiểm toán, ghi sai "ai" nguy hại hơn module khác. Thực tế: các trang ghi audit đều `[Authorize]` nên actor hầu như luôn có → hiếm khi chạm fallback → **không tái hiện được kịch bản khai thác hiện tại**. Khuyến nghị: cho audit dùng `GetUserIdAsync` (null → "Hệ thống") hoặc throw thay vì mượn first-user. (Cùng lớp observation `AuditLogHelpers:33` đã ghi ở sweep các phiên trước.)
- **OBS-M19-04 — Chỉ 200 dòng, không phân trang/khoảng ngày/export (Low, requirement):** `Take(200)` → log cũ hơn không xem được qua UI; không lọc theo ngày; không export. → **U-M19-2** (vận hành thật có cần?).
- **OBS-M19-05 — Không enforce immutability ở DB (Low):** append-only chỉ theo quy ước app; super_admin/DB trực tiếp vẫn sửa/xóa được `audit_logs`. Không có đường khai thác qua ứng dụng. Cân nhắc trigger DB/append-only khi siết bảo mật (giao M20).
- **OBS-M19-06 — Nhãn action chưa phủ hết (cosmetic, Low):** `AuditActionLabel` có key `create_receipt`/`reset_password` không bao giờ được emit (receipts ghi `create`; đổi mật khẩu ghi `change_password`) → hiển thị rơi về `HumanizeTechnicalName` (vẫn đọc được, chỉ kém "đẹp"). Không ảnh hưởng dữ liệu.

## Codex Handoff Queue

| Order | Bug ID | Severity | Status |
|---|---|---|---|
| — | — | — | **Không có bug cần Codex.** Observations chờ user chốt (U-M19-1/2) trước khi thành change request. |

> **Kết luận M19:** `QA=No Confirmed Bugs`, `Codex=Not Required`, `Verification=Verified (code-level)`. 3 unit contract (`M19_AuditLogTests`) pass; suite 101/101; Web build 0/0. Observations OBS-M19-01..06 (non-blocking); requirement mở U-M19-1 (login/logout + Ip/UA), U-M19-2 (paging/range/export). Runtime E2E write→view + actor-null path pending harness.
