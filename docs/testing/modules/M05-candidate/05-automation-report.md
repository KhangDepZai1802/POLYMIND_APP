# M05 — Candidate Management · Automation Report

## Framework & môi trường
- **Test project:** `tests/Polymind.Tests` (xUnit, net10.0). Ref: `Polymind.Domain`, `Polymind.Infrastructure`. **KHÔNG** ref `Polymind.Web`.
- **Lệnh:** `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo`
- **Kết quả:** `Passed 29, Failed 0, Skipped 0` (đã chạy phiên này).
- **Web compile:** `dotnet build src/Polymind.Web/Polymind.Web.csproj` → 0 warning / 0 error.

## Automated tests liên quan M05 (proxy — logic thuần)

| Automated Test | Test Case phủ | Layer | Kết quả |
|---|---|---|---|
| `M02_CandidateAccessScopeTests` (5) | TC_M05_002/003/008 (data-scope + REST IDOR) | Domain unit | Pass |
| `M03_CandidateAccountLinkRulesTests` (4) | TC_M05_026 (cleanup link khi xóa user) | Domain unit | Pass |
| `M04_LeadConversionRulesTests` (3) | TC_M05_009 (convert attribution) | Domain unit | Pass |

> M05 KHÔNG thêm test mới: logic scope/link/convert đã tách sang `Polymind.Domain` và phủ ở M02/M03/M04; viết lại sẽ trùng lặp. Các thao tác còn lại (RB-1/RB-2/CRUD UI) nằm trong `.razor` (Web) → cần harness bUnit/Playwright chưa có.

## Verified bằng source review (không có test tự động, nhưng có bằng chứng dòng code)
- **IDOR web detail** — `CandidateDetail.Load` dòng 1074-1082 (TC_M05_005–007).
- **RB-1** — `CollaboratorInfoDialog` dòng 47-51 + `_hideSensitive` dòng 78 (TC_M05_016–018).
- **RB-2 authorization** — `ChangeAssigneesAsync` dòng 1572, `ChangeJobOrderAsync` dòng 1606 (TC_M05_020/023).
- **Delete authorization** — `DeleteCandidate` dòng 1409 (TC_M05_014).
- **Edit authorization** — `_canEditCandidateProfile` + re-check dòng 1394 (TC_M05_012).
- **Mask SĐT CTV** — `_maskPhone` dòng 1083 + `MaskPhone` dòng 2148 (TC_M05_027).

## Pass / Fail / Blocked
- **Pass (unit proxy):** 12 (5+4+3) — nằm trong 29/29.
- **Pass (code-level review, chưa runtime):** TC_005–007, 010, 012, 014, 016–018, 020, 023, 025, 027.
- **Blocked (no harness):** TC_001, 004, 009, 011, 013, 015, 019, 021, 022, 024, 029, 030 (UI/integration).
- **Pass (spec confirmed):** TC_028 — U1 đã chốt: CTV được xem passport/CCCD (không mask) → hành vi đúng.

## Automation backlog (đề xuất)
1. Dựng harness integration (WebApplicationFactory + DB test `polymind_test`/Testcontainers) → phủ runtime IDOR REST (BUG_M02_02), cascade delete, RB-2 password.
2. Tách `BusinessRoleAccess` + `AgentScope` từ `Polymind.Web` sang `Polymind.Domain`/`Application` → unit-test ma trận role trực tiếp (TC_012/014/020/023).
3. bUnit cho `CollaboratorInfoDialog` (RB-1) và card RB-2 (render theo scope/role).
