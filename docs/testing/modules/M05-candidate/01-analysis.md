# M05 — Candidate Management · Phân tích

> **Trạng thái:** Analysis hoàn chỉnh cho phạm vi authorization / data-scope / RB-1 / RB-2 / CRUD chính. Đọc kèm `02-business-flows.md`, `03-test-cases.md`.
> Đã đọc source: `Candidates.razor`, `CandidateDetail.razor` (2175 dòng — các luồng chính), `CollaboratorInfoDialog.razor`, `BusinessRoleAccess.cs`, `AgentScope`, `ResourceEndpoints` (đối chiếu ở M02), entity Candidate/CandidateDocument/CandidateJobOrder.
> Phạm vi CHƯA phủ (chuyển module chuyên trách): tài liệu versioned chi tiết → **M18**; workflow 20 bước → **M07**; tài chính/hoa hồng → **M10/M09**; visa/vé → **M12**.

## 1. Module Overview

- **Module ID:** M05
- **Module name:** Candidate Management (Quản lý ứng viên)
- **Business purpose:** Hồ sơ ứng viên xuyên suốt quy trình XKLĐ: thông tin cá nhân/hộ chiếu, đại lý/CTV/TVV, gắn đơn hàng + workflow, tài chính tóm tắt, và liên kết tài khoản cổng. 1 ứng viên = tối đa 3 tài khoản: CTV/TVV (giới thiệu), Học viên (`OwnerUserId`), Phụ huynh (`ParentUserId`).
- **Actor:** staff (super_admin/RM/recruiter/consultant/doc_staff/visa_staff/accountant theo quyền) + đối tác (agent/collaborator — scoped) + cổng cá nhân (parent/student — 1 hồ sơ).
- **Dependencies:** M02 (`candidates:*` + AgentScope), M04 (Convert tạo hồ sơ). Tài liệu→M18, workflow→M07, tài chính→M10.
- **Entry point:** `/candidates` (list), `/candidates/{id}` (detail), REST `GET /api/candidates` (+/{id}). Cổng cá nhân: parent/student redirect thẳng vào hồ sơ của mình.
- **Exit point:** hồ sơ xóa (manual cascade), hoặc chuyển sang workflow/tài chính (module khác).

## 2. Source Code Map

| # | File | Vai trò | Ghi chú QA |
|---|---|---|---|
| 1 | `Candidates.razor` (356) | List + tìm/lọc + data-scope | AgentScope áp đúng; lọc/sort client-side (perf obs) |
| 2 | `CandidateDetail.razor` (2175) | Chi tiết + mọi thao tác | IDOR-by-URL chặn ở `Load`; RB-2; delete cascade |
| 3 | `CandidateDialog.razor` (447) | Form tạo/sửa hồ sơ | gate `_canEditCandidateProfile` |
| 4 | `CollaboratorInfoDialog.razor` (Shared) | Modal thông tin CTV | **RB-1** `_hideSensitive` |
| 5 | `StudentAccountDialog` / `ParentAccountDialog` (164) | Gắn/gỡ tài khoản cổng | verify ở M01/M03 (stamp + cleanup) |
| 6 | `ResourceEndpoints.cs` | REST `/api/candidates` | **BUG_M02_02** (đã fix + verify ở M02) |
| 7 | `BusinessRoleAccess.cs` (Web/Display) | Chặn role sửa/xóa | ma trận ở §6 |
| 8 | `AgentScope.cs` (Web/Identity) | Resolve phạm vi | agent/collaborator/self/parent/student |
| 9 | `Candidate.cs` / `CandidateDocument.cs` / `CandidateJobOrder.cs` (Domain) | Entity | FK mềm (chỉ index) |

## 3. Data-scope (trọng tâm rủi ro)

### 3.1 Web list — ĐÚNG
`Candidates.razor.Load` áp `AgentScope`: agent→`AgentId`, collaborator→`CollaboratorId`, self(parent/student)→`Id == OwnedCandidateId` (null→rỗng); parent/student redirect thẳng vào hồ sơ mình.

### 3.2 Web detail `/candidates/{id}` — ĐÚNG (giải U1)
`CandidateDetail.Load` (dòng 1074-1082): out-of-scope (`IsAgentOnly && AgentId≠scope` | `IsCollaboratorOnly && CollaboratorId≠scope` | `IsSelfScoped && OwnedCandidateId≠Id`) → `_accessDenied=true; _candidate=null`. **Parent/student KHÔNG xem được hồ sơ khác qua URL.**

### 3.3 REST API — SAI → BUG_M02_02 (đã fix + verify ở M02)
`GET /api/candidates` trước fix chỉ gate `candidates:read`, không áp scope → student/parent/collaborator demo thấy toàn bộ 18 hồ sơ + `passportNumber` (bằng chứng runtime `M02/evidence-M02_02-runtime.md`). Codex đã áp `CandidateAccessScope` fail-closed; M02 `08-verification-report.md` = Verified (code). M05 chỉ tham chiếu.

## 4. Database Impact
- `candidates`: PII (`cccd_number`, `passport_number`, bank…); FK mềm `agent_id`/`collaborator_id`/`consultant_id`/`owner_user_id`/`parent_user_id`/`lead_id` (chỉ index, không FK cứng → liên quan BUG_M03_01/BUG_M04_01).
- Xóa ứng viên: **manual cascade một DbContext** (candidate_job_orders, workflow_step_records, candidate_documents, document_versions, loans, payments, receipts, visas, agent_commissions). Không FK cascade → rủi ro sót bảng khi thêm quan hệ mới (obs, không phải bug hiện tại).

## 5. Masking / hiển thị nhạy cảm
- `_maskPhone = scope.IsCollaboratorOnly` → CTV không xem SĐT ứng viên (Phone, EmergencyContactPhone, GuardianPhone qua `MaskPhone`).
- **RB-1** (`CollaboratorInfoDialog._hideSensitive = IsParent || IsStudent`): ẩn đúng 2 dòng "Ứng viên đã giới thiệu" + "Tỷ lệ hoa hồng CTV %"; các dòng liên lạc (tên/đại lý/SĐT/email/địa chỉ/trạng thái) vẫn hiện. **Đúng spec WORKLOG:35.**

## 6. Roles & Permissions

| Action | Permission | Role gate (BusinessRoleAccess) | Extra | Source |
|---|---|---|---|---|
| Xem list/detail | `candidates:read` | — (+ AgentScope) | scope | Candidates/CandidateDetail |
| Tạo hồ sơ | `candidates:create` | — | — | Candidates (+Convert M04) |
| Sửa hồ sơ | `candidates:update` | `CanEditCandidateProfile` = super_admin/RM/recruiter/consultant/doc_staff | — | `_canEditCandidateProfile` |
| Xóa hồ sơ | `candidates:delete` | `CanDeleteCandidate` = super_admin/doc_staff | confirm | `DeleteCandidate` (re-check dòng 1409) |
| Đổi TVV/CTV | — | **super_admin only** | **ConfirmPassword** | RB-2 `ChangeAssigneesAsync` (dòng 1572) |
| Đổi đơn hàng | — | **super_admin only** | **ConfirmPassword** + reset workflow | RB-2 `ChangeJobOrderAsync` (dòng 1606) |
| Gắn/gỡ tài khoản cổng | `users:create` | — | stamp/cleanup (M01/M03) | Open{Student,Parent}Account |

**Kết luận authorization:** mọi thao tác mutating re-check server-side (không chỉ ẩn nút UI). Blazor Server → handler chỉ chạy nếu component render nút; parent/student bị `_accessDenied` trước khi render. IDOR bề mặt duy nhất là REST API (BUG_M02_02, đã xử lý).

## 7. Risk Analysis

| # | Risk | Mức | Trạng thái |
|---|---|---|---|
| R1 | IDOR REST `/api/candidates` (PII toàn bộ) | High | **BUG_M02_02 — Fixed + Verified (M02)** |
| R2 | IDOR web detail-by-URL | High | **Chặn** (AgentScope ở `Load`) — không bug |
| R3 | RB-1 lộ commission/referred-count cho parent/student | Medium | **Đúng** (`_hideSensitive`) — không bug |
| R4 | RB-2 non-super_admin đổi TVV/CTV/đơn hàng | High | **Chặn** (`_isSuperAdmin` + password, re-check) — không bug |
| R5 | Xóa ứng viên sót bảng liên quan (orphan) | Medium | Obs — manual cascade đầy đủ hiện tại; rủi ro bảo trì |
| R6 | List client-side filter (perf ở scale) | Low | Obs — như M04 R2 |
| R7 | Convert race tạo trùng Candidate (no unique `lead_id`) | Low | Obs — pre-existing (M04 R3), theo dõi M07 |
| R8 | CTV xem `passport_number` của ứng viên giới thiệu | Low | **ĐÓNG (user chốt 2026-07-10): CTV ĐƯỢC xem passport/CCCD → hành vi đúng, không bug** |

## 8. Requirement Clarification — ĐÃ CHỐT (user 2026-07-10)
- **U1 — ĐÃ CHỐT:** CTV (collaborator scope) **ĐƯỢC** xem `passport_number`/`cccd_number` của ứng viên mình giới thiệu (chỉ SĐT bị mask). → Hành vi hiện tại đúng, **không cần mask giấy tờ, không phải bug.** RB-1 giữ nguyên.
- **U2 — ĐÃ CHỐT:** Reset workflow khi đổi đơn hàng (RB-2) **KHÔNG** hoàn tiền/hoa hồng đã phát sinh (khớp WORKLOG). → Hành vi đúng, không bug. Verify chéo M09/M10 rằng không có hoàn tiền vô tình.
