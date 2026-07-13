# M06 — Job Orders · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic; chỉ đọc source, chạy test, đánh giá.
> **Ngày:** 2026-07-11 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit; runtime create-as-RM pending harness).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M06_01 Low) | ✔ |
| `07-fix-report.md` | ✔ |
| `JobOrderDialog.razor` `Save` (create + edit path) | ✔ |
| `Domain/JobOrders/JobOrderCreationRules.cs` (factory mới) | ✔ |
| `AuditLogHelpers.GetRequiredUserIdAsync` (helper attribution) | ✔ |
| `M06_JobOrderCreationRulesTests.cs` | ✔ |
| Rà toàn `src` `Users.Select(...Id).First*` (anti-pattern) | ✔ |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 52, Skipped: 0
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M06_01 — `JobOrderDialog.Save` gán `CreatedBy` cho user đầu tiên trong DB thay vì actor

**Kết luận: Verified Fixed (code-level).** Runtime create-as-RM (query `job_orders.created_by`) còn chờ bUnit/integration harness — không tuyên bố đã đo runtime.

### Bằng chứng đã kiểm

1. **Create path dùng actor thật:** `JobOrderDialog.Save` nhánh tạo mới (dòng 151-156) nay `var actorId = await AuthStateProvider.GetRequiredUserIdAsync(db);` rồi tạo qua `JobOrderCreationRules.Create(actorId, code)`. Không còn `db.Users.Select(u => u.Id).FirstOrDefaultAsync()`.
2. **Factory gán đúng:** `JobOrderCreationRules.Create(Guid actorId, string code)` set `CreatedBy = actorId`. Unit test `New_job_order_is_attributed_to_the_authenticated_actor` (Passed) khóa: tạo bằng `actorId` bất kỳ (Guid độc lập) → `CreatedBy == actorId` và `Code` đúng.
3. **Edit path không đụng `CreatedBy`:** nhánh `IsEdit` (dòng 138-148) chỉ `_m.ApplyTo(j)` + `j.UpdatedAt`; `ApplyTo` không map `CreatedBy` (chỉ Category/Country/…/Status) → attribution gốc giữ nguyên khi sửa.
4. **Authorization vẫn trước insert:** permission `job_orders:create`/`update` + `BusinessRoleAccess.CanEditJobOrder` re-check ở dòng 125-132, chạy TRƯỚC khi resolve actor và mutate DB. Fix không làm yếu quyền.
5. **Sweep anti-pattern:** `Users.Select(...Id).First*` toàn `src` còn 2 vị trí — `VisaDialog.razor:136` + `FlightDialog.razor:128` (**M12**, đã ghi bug khi QA tới) và fallback `AuditLogHelpers.cs:33` (obs, dùng SAU actor) + `DemoDataSeeder.cs:23` (seed). Không còn instance nào trong M06.

### Không tìm thấy hành vi né bug
- Không sửa expected result; test kiểm đúng bản chất (CreatedBy = actor truyền vào).
- Không hard-code, không tắt validation/authorization. Contract API/schema/form không đổi.

### Residual risk (đo lường được)
- Runtime chưa chứng minh `job_orders.created_by = RM.id` bằng đăng nhập RM thật (cần bUnit/WebApplicationFactory). Logic đủ căn cứ ở source + unit.
- Nếu principal thiếu `NameIdentifier`, `GetRequiredUserIdAsync` fallback về user đầu DB (obs shared, chờ M19/M20) — không thuộc phạm vi bug này.
- M12 `VisaDialog`/`FlightDialog` cùng anti-pattern vẫn hở, sẽ verify chính thức khi QA M12.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M06_01 | Low | **Verified Fixed** (code-level; runtime create-as-RM pending harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed
- **Verification Status:** Verified (code-level) — runtime DB attribution chưa đo, không tuyên bố 100%.
