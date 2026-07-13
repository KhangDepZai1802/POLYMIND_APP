# M05 — Candidate Management · Traceability

| Business Flow | Page / API | Role | State/Data | Test Case IDs | Automated Test | Coverage | Gap |
|---|---|---|---|---|---|---|---|
| BF-M05-01 List theo scope | `Candidates.razor` | staff/agent/collab/self | AgentScope | TC_001–004 | `CandidateAccessScope` (@M02) proxy | Scope logic ✔; UI redirect ✗ | Redirect/list UI cần harness |
| BF-M05-02 Detail IDOR | `CandidateDetail.Load` | tất cả | scope guard | TC_005–007, 027–028 | code review | IDOR web chặn ✔ (code) | Runtime UI + U1 passport spec |
| (REST scope) | `GET /api/candidates` | student/parent/collab | BUG_M02_02 | TC_008 | `CandidateAccessScope` (@M02) | Fixed+Verified code @M02 | Runtime HTTP JWT |
| BF-M05-03 Tạo | `CandidateDialog` / Convert | create roles | insert | TC_009–010, 029–030 | `LeadConversionRules` (@M04) convert | Convert attribution ✔ | Dialog create UI; race |
| BF-M05-04 Sửa | `CandidateDialog` edit | edit roles | update | TC_011–012 | code review | AuthZ ✔ (code 1394) | Update UI + lost-update |
| BF-M05-05 Xóa cascade | `DeleteCandidate` | super_admin/doc_staff | manual cascade | TC_013–015 | code review | AuthZ re-check ✔ | Cascade integrity runtime |
| BF-M05-06 RB-1 | `CollaboratorInfoDialog` | parent/student | `_hideSensitive` | TC_016–018 | code review | Đúng spec ✔ (code) | UI render pending |
| BF-M05-07 RB-2 TVV/CTV | `ChangeAssigneesAsync` | super_admin | password confirm | TC_019–021 | code review | AuthZ+password ✔ (code 1572) | Password UI runtime |
| BF-M05-08 RB-2 đơn hàng | `ChangeJobOrderAsync` | super_admin | reset workflow | TC_022–023 | code review | AuthZ ✔ (code 1606) | Reset side-effects (U2, M07) |
| BF-M05-09 Tài khoản cổng | `Student/ParentAccountDialog` | users:create | link + stamp | TC_024–026 | `CandidateAccountLinkRules` (@M03) | Stamp/cleanup ✔ (@M01/M03) | Create/link UI runtime |

## Tổng hợp coverage

- **Verified ở mức code (source review):** IDOR web detail (R2), RB-1 (R3), RB-2 authorization (R4), delete authorization, mask SĐT CTV, gắn/gỡ tài khoản (stamp+cleanup).
- **Verified ở mức unit (proxy, chạy pass):** data-scope (`CandidateAccessScope` 5 case @M02), account-link cleanup (@M03), convert attribution (@M04) — nằm trong `dotnet test` 29/29.
- **Blocked (no harness):** mọi luồng UI thực (tạo/sửa/xóa, RB-2 password), cascade integrity, redirect self-scope, XSS, mass-assignment, convert race.
- **Requirement Clarification ĐÃ CHỐT (user 2026-07-10):** U1 — CTV **được** xem passport/CCCD (không mask). U2 — reset workflow **KHÔNG** hoàn tiền/hoa hồng. Cả hai: hành vi hiện tại đúng, không bug.

## Rủi ro còn lại
- Không có integration/E2E harness → các khẳng định runtime chưa đo. Không tuyên bố 100%.
- `BusinessRoleAccess`/`AgentScope` ở `Polymind.Web` chưa unit-test được (backlog tách layer).
