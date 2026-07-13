# Verification Report — M14 Messaging / Chat

- **Module:** M14 — Messaging / Chat
- **Verifier:** Claude (Independent Verification) — 2026-07-11 phiên #8
- **Fix under review:** CR-M14-1 (U-M14-1) — `07-fix-report.md`
- **Verification Status:** **Verified (code-level)**

## Scope

CR-M14-1: giới hạn staff/Agent/CTV chỉ nhắn Phụ huynh/Học viên **thuộc ứng viên mình phụ trách**; áp đối xứng để portal reply đúng người liên quan; enforcement phải ở server (không chỉ ẩn UI).

## Evidence Reviewed

- `src/Polymind.Domain/Messaging/CandidateMessagingRelationship.cs`:
  - `MessagingCandidateScope.ForResponsibleUser` gom candidate theo `ConsultantId`, `CJO.AssignedTo`, `WorkflowStepRecord.AssignedTo`, `Visa.HandledBy`, `Flight.AssignedTo`.
  - `CandidateMessagingRelationship.AllowedRecipientsFor(userId)`: portal↔portal + portal↔responsible; **fail-closed** (user không thuộc portal/responsible → tập rỗng); tự loại `userId`.
- `src/Polymind.Web/Identity/MessagingPolicy.cs`: thêm `IsPortalUser(roles)` = chứa Parent/Student; `CanMessage` role policy giữ nguyên.
- `src/Polymind.Web/Components/Pages/Messages/Messages.razor`:
  - `BuildRelationshipRecipientsAsync` (`:258-357`) resolve scope theo actor: self→`OwnedCandidateId`; Agent→`AgentId`; CTV→`CollaboratorId` (null→`Where(_=>false)`); staff→`ForResponsibleUser`. Dựng participants (responsible + consultant + agent user + collaborator user), union `AllowedRecipientsFor(_meId)`.
  - `LoadContacts` (`:225-233`): self-scoped chỉ hiện user trong graph; non-self bỏ portal user không thuộc graph (`:231`) + áp `CanMessage` (`:232`).
  - `Send` (`:441-459`): **re-query** `BuildRelationshipRecipientsAsync(db)` + recipient roles từ DB TRƯỚC upload/insert; self-scoped chặn ngoài graph; non-self chặn portal ngoài graph HOẶC role policy fail → Snackbar warning + return.

## Bug-by-bug Verdict

| Item | Verdict | Bằng chứng |
|---|---|---|
| CR-M14-1 | **Verified Fixed (code-level)** | Scope quan hệ áp cho MỌI actor (không chỉ self-scoped như trước); `Send` re-check server-side từ DB; đối xứng hai chiều; fail-closed khi thiếu mapping. |

## Tests / Regression

- `M14_MessagingRulesTests` (**7/7**): relationship, symmetric reply, fail-closed, PostgreSQL SQL translation (EXISTS/WHERE).
- Full suite: **Passed 122, Failed 0, Skipped 0**.
- Web build: **0 Warning, 0 Error**.
- Không sửa test/expected để né lỗi; guard IDOR thread / recall / upload cũ không bị làm yếu; partner→staff và staff↔staff role policy giữ nguyên (đúng phạm vi CR).

## Residual / Not Measured

- E2E Blazor/PostgreSQL/MinIO đa người dùng chưa chạy (chưa có integration harness) — graph assembly runtime chưa đo trực tiếp; translation đã kiểm.
- `Send` chưa re-check recipient `IsActive` (OBS-M14-03, ngoài CR).
- Staff chỉ "phụ trách" qua explicit assignment; candidate chưa gán staff → không hiện portal (fail-closed, đúng thiết kế).

## Conclusion

CR-M14-1 **Verified Fixed (code-level)**. → `QA=No Confirmed Bugs`, `Codex=Fixed`, `Verification=Verified (code)`.
