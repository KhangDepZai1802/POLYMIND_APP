# M04 — Lead CRM · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic.
> **Ngày:** 2026-07-10 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit; runtime convert UI/DB pending harness).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M04_01 Low) | ✔ |
| `07-fix-report.md` | ✔ |
| `LeadConversionRules.cs` (mới) | ✔ |
| `LeadDetail.Convert` | ✔ |
| `M04_LeadConversionRulesTests.cs` (3 cases) | ✔ |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 29, Skipped: 0 (gồm 3 case M04)
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M04_01 — `Convert()` gán `CreatedBy` cho user đầu tiên thay vì actor

**Kết luận: Verified Fixed (code-level).** Runtime convert UI/DB attribution còn chờ harness.

### Bằng chứng đã kiểm

1. **Actor thật:** `LeadDetail.Convert` (dòng 598) lấy `actorId = await AuthStateProvider.GetRequiredUserIdAsync(db)` — cùng pattern với delete/assign/appointment/revert trong file. Không còn `db.Users.Select(u => u.Id).FirstOrDefaultAsync()` (grep `db.Users.Select` và `adminId` = 0 kết quả).
2. **Quy tắc thuần fail-fast:** `LeadConversionRules.CreateCandidate(lead, actorId, code)` set `CreatedBy = actorId`, ném `ArgumentException` nếu `actorId == Guid.Empty` hoặc code rỗng, `ArgumentNullException` nếu lead null.
3. **Mapping bảo toàn:** factory copy đầy đủ profile + `AgentId`/`CollaboratorId`/`ConsultantId (= lead.AssignedTo)` — test `Conversion_preserves_candidate_profile_and_assignment_fields` (15 field) Passed.
4. **Authorization giữ nguyên:** `Convert` vẫn kiểm `HasLeadUpdateAccess() && candidates:create` trước khi chạy (dòng 571); actor lấy server-side sau authorize → không giả mạo từ client.
5. **Anti-duplicate + state transition giữ nguyên:** nhánh `existingId is not null` → điều hướng hồ sơ cũ (dòng 590-596); `lead.Status = Converted` + `LeadActivity` StatusChange + một `SaveChangesAsync` (dòng 605-614). Không phá luồng.

### Không tìm thấy hành vi né bug
- Codex **không** sửa expected result để hợp thức hóa; 3 test kiểm đúng bản chất (attribution = actor).
- Tách logic thuần sang `Polymind.Domain/Leads` → unit-test được, không hard-code, không tắt validation/authorization.

### Residual risk (đo lường được, KHÔNG mở bug mới)
- **Convert race (R3, Low, pre-existing):** kiểm trùng dòng 590 là read-then-write (TOCTOU), không có unique index `candidates(lead_id)`. Hai request đồng thời vẫn có thể tạo 2 Candidate. **Không thuộc BUG_M04_01** (attribution) — đã ghi ở "Ghi chú không nâng thành bug", theo dõi M05/M07. Không phải regression của fix này.
- Runtime convert thật (UI → DB `created_by`) chờ harness → chưa đo, không tuyên bố 100%.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M04_01 | Low | **Verified Fixed** (code-level; runtime convert attribution pending harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed
- **Verification Status:** Verified (code-level) — runtime convert + race R3 (pre-existing) ghi rõ là chưa đo.
