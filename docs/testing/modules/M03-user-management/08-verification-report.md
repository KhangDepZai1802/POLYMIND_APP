# M03 — User & Account Management · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic.
> **Ngày:** 2026-07-10 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit; runtime Identity/PostgreSQL delete pending harness).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M03_01 Medium) | ✔ |
| `07-fix-report.md` | ✔ |
| `CandidateAccountLinkRules.cs` (mới) | ✔ |
| `AccountManagerPanel.DeleteUserAsync` | ✔ |
| `M03_CandidateAccountLinkRulesTests.cs` (4 cases) | ✔ |
| Cross-ref hardening M01 (Parent/StudentAccountDialog.Unlink) | ✔ (đã verify ở M01) |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 29, Skipped: 0 (gồm 4 case M03)
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M03_01 — Xóa tài khoản Phụ huynh để lại `Candidate.ParentUserId` rác

**Kết luận: Verified Fixed (code-level).** Runtime Identity/PostgreSQL delete còn chờ harness.

### Bằng chứng đã kiểm

1. **Quy tắc thuần đối xứng:** `CandidateAccountLinkRules.UnlinkUser(candidate, userId)` xóa `OwnerUserId` và/hoặc `ParentUserId` nếu trùng `userId`, trả `changed`. Không còn bất đối xứng chỉ-Owner.
2. **Caller đã sửa:** `DeleteUserAsync` query `OwnerUserId == user.Id || ParentUserId == user.Id` (dòng 416-418), lặp `UnlinkUser` + cập nhật `UpdatedAt`, `SaveChangesAsync` khi có thay đổi, **rồi mới** `UserManager.DeleteAsync`. Đúng thứ tự: gỡ link trước khi xóa Identity.
3. **Giữ hồ sơ ứng viên** (đúng quyết định user): chỉ set link=null, không xóa Candidate. Không thêm FK/cascade/migration.
4. **Dữ liệu biên:** case cùng user ở cả hai field (`Same_user_in_both_links_is_fully_unlinked`) và case không liên quan (`Unrelated_candidate_links_are_preserved`) đều Passed → không dọn nhầm link của user khác.
5. Audit `delete/users` giữ nguyên sau khi xóa.

### Không tìm thấy hành vi né bug
- Codex **không** sửa expected result để hợp thức hóa; 4 test kiểm đúng bản chất (đối xứng owner/parent).
- Không hard-code, không tắt validation/authorization. Tách logic thuần sang `Polymind.Domain` giúp unit-test được — đúng đề xuất quick-win.

### Residual risk (đo lường được, KHÔNG mở bug mới)
- **Hai DbContext/transaction:** cleanup Candidate và `DeleteAsync` chạy ở hai context riêng. Nếu `DeleteAsync` fail SAU khi cleanup đã save → user còn tồn tại nhưng link đã gỡ (candidate mất liên kết dù user chưa xóa). Đây là **rủi ro cũ có sẵn** (thiết kế delete cũ), Codex không mở rộng — ghi nhận, không tính là regression của bug này. Đề xuất theo dõi ở M03 backlog / M17 Data Integrity.
- Runtime xóa thật trên PostgreSQL (tracking/save/delete + audit) chờ WebApplicationFactory + DB test → chưa đo, không tuyên bố 100%.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M03_01 | Medium | **Verified Fixed** (code-level; runtime DB delete pending harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed
- **Verification Status:** Verified (code-level) — runtime DB delete + residual two-transaction risk ghi rõ là chưa đo.
