# M12 — Visa & Flight / Exit · Verification Report (Independent — Claude)

> Xác minh độc lập bản sửa của Codex cho BUG_M12_01/02. Đọc source thật + diff + test, không chỉ dựa `07-fix-report.md`.
> **Ngày:** 2026-07-11 · **AI:** Claude (Independent Verification Engineer) · **Phiên:** #5

---

## 1. Phạm vi xác minh

- **Bugs nhận từ Codex:** BUG_M12_01 (Medium), BUG_M12_02 (Low) — cùng lớp anti-pattern "first-user attribution".
- **Fix report:** `07-fix-report.md` (Codex, Session 67).
- **Nguồn đã đọc:** `01-analysis.md` → `06-bug-report.md`, `07-fix-report.md`, git diff thật, source sau sửa (2 dialog + Domain factory + test + NotificationService + AuditLogHelpers).
- **Không dùng verdict Codex làm căn cứ tự động** — Codex để trạng thái `Fixed — chờ Claude xác minh`.

## 2. Bằng chứng diff thật (git diff HEAD)

```
VisaDialog.razor  (create branch)
- var v = new Visa { HandledBy = await db.Users.Select(u => u.Id).FirstOrDefaultAsync() };
+ var actorId = await AuthStateProvider.GetRequiredUserIdAsync(db);
+ var v = VisaFlightCreationRules.CreateVisa(actorId);

FlightDialog.razor (create branch)
- var f = new Flight { AssignedTo = await db.Users.Select(u => u.Id).FirstOrDefaultAsync() };
+ var actorId = await AuthStateProvider.GetRequiredUserIdAsync(db);
+ var f = VisaFlightCreationRules.CreateFlight(actorId);
```

- Mỗi dialog +1 dòng `@using Polymind.Domain.Visas`. Diff sạch, chỉ chạm create branch (net +3/-1 mỗi file). Không đụng edit branch, permission check, form mapping, notification.
- File mới (untracked): `src/Polymind.Domain/Visas/VisaFlightCreationRules.cs`, `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs`.

## 3. Kết quả xác minh từng bug

### BUG_M12_01 (Medium) — Visa `HandledBy` first-user → misroute visa reminder → **Verified Fixed (code-level)**

| Điểm kiểm | Kết quả |
|---|---|
| Nguồn attribution | `VisaDialog.Save` create branch nay resolve `actorId = AuthStateProvider.GetRequiredUserIdAsync(db)` (VisaDialog.razor:137) rồi `CreateVisa(actorId)` → `HandledBy = actorId`. Không còn `db.Users...FirstOrDefaultAsync()`. |
| Helper actor | `GetRequiredUserIdAsync` (AuditLogHelpers:25-34) lấy `ClaimTypes.NameIdentifier` của phiên đăng nhập; chỉ fallback user-đầu-DB khi **không có** claim (không xảy ra với phiên hợp lệ). Đúng pattern đã Verified ở M06. |
| Permission gate | Re-check `visas:create` (VisaDialog.razor:113-118) chạy **trước** khi resolve actor + mutation. Không nới quyền. |
| Edit path không ghi đè | `FormModel.ApplyTo` (VisaDialog.razor:186-198) chỉ set CandidateId/JobOrderId/Country/VisaType/Status/dates/RejectionReason/Notes — **KHÔNG** chạm `HandledBy`. Ownership visa cũ giữ nguyên khi sửa. |
| Routing hệ quả | `NotificationService:291-293` route `ReminderVisa` tới `HandledBy` nếu có, else `CandidateOwnersOr(VisaStaff, Director)`. Logic notification **không đổi**; nay nguồn `HandledBy` = actor thật → reminder tới đúng người. |
| Regression test | `New_visa_is_attributed_to_the_authenticated_actor` assert `visa.HandledBy == actorId` (thật, không làm yếu assertion). |

### BUG_M12_02 (Low) — Flight `AssignedTo` first-user (attribution) → **Verified Fixed (code-level)**

| Điểm kiểm | Kết quả |
|---|---|
| Nguồn attribution | `FlightDialog.Save` create branch resolve `actorId` rồi `CreateFlight(actorId)` → `AssignedTo = actorId` (FlightDialog.razor:129-130). Không còn first-user query. |
| Không ảnh hưởng routing | Departure reminder (`NotificationService:306-312`) dùng `CandidateOwnersOr(f.CandidateId, VisaStaff, Director)` — **KHÔNG** dùng `Flight.AssignedTo`. Fix chỉ sửa attribution (cosmetic như bug report mô tả), routing không đổi. |
| Edit path | `FormModel.ApplyTo` (FlightDialog.razor:178-190) không chạm `AssignedTo`. |
| Regression test | `New_flight_is_attributed_to_the_authenticated_actor` assert `flight.AssignedTo == actorId`. |

## 4. Kiểm tra Codex có né bug / hard-code / workaround không

- **Không sửa expected result / không làm yếu assertion:** 2 regression test kiểm đúng invariant (attribution = actor). 6 test contract còn lại (VisaStatus lifecycle, default NotSubmitted, HandledBy/AssignedTo/ActualDepartureAt nullable) không bị đổi để né.
- **Không tắt validation/authz:** permission re-check giữ nguyên ở cả 2 dialog.
- **Không hard-code / không đụng migration / không viết lại dữ liệu cũ.**
- **Domain factory** `VisaFlightCreationRules` chỉ khởi tạo attribution — đúng phạm vi, không giấu logic.

## 5. Regression & build

| Hạng mục | Lệnh | Kết quả |
|---|---|---|
| Shared suite | `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo` | **Passed 64, Failed 0, Skipped 0** |
| Web build | `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=C:\tmp\polymind-m12-verify\` | **0 Warning / 0 Error** |

## 6. Kết luận từng bug

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M12_01 | Medium | **Verified Fixed** (code-level) |
| BUG_M12_02 | Low | **Verified Fixed** (code-level) |

**Module M12:** `QA Status = Completed` · `Codex Status = Fixed` · `Verification Status = Verified (code-level)`.

## 7. Residual / chưa đo (ghi rõ — không tuyên bố 100%)

- **Runtime chưa đo (thiếu harness bUnit/Playwright + DB):** submit dialog bằng user VisaStaff không-seed-đầu để xác nhận `visas.handled_by`/`flights.assigned_to` trong Postgres; chạy `GenerateRemindersForAllUsersAsync` end-to-end để xác nhận `ReminderVisa` tới đúng `HandledBy`. Phân tích tĩnh + regression đủ cho verdict code-level; runtime E2E pending harness (blocker chung).
- **Observations M12 vẫn mở (không thuộc 2 bug này — là change-request/observation):**
  - OBS-M12-01 — visa/flight không ghi audit (**req U-M12-2**, user đã chốt → change request).
  - OBS-M12-02 — VisaStatus không state-machine + no rowversion.
  - OBS-M12-03 — không có đường runtime set `Flight.ActualDepartureAt` → report xuất cảnh thực tế rỗng (**req U-M12-1**, user đã chốt: thêm nút "Xác nhận đã bay" → change request).
  - OBS-M12-04 — không unique (CandidateId, JobOrderId) cho visa/flight.
  - Các mục trên đã ghi ở bảng change-request (MODULE_QA_BOARD) chờ Codex thực thi khi user ưu tiên; không chặn verdict 2 bug attribution.

## 8. Hành động tiếp theo

- **Claude:** M12 verified → QA tiếp **M13 Notifications** (dep M07/M10/M12 đủ). Verify visa reminder recipient đã đúng nguồn (HandledBy = actor) — dùng lại kết luận này khi QA reminder ở M13.
- **Codex:** không còn bug M12 trong queue. Change-request M12 (U-M12-1/2) chờ user ưu tiên xếp lịch.
