# M05 — Candidate Management · Bug Report

Chỉ ghi bug có bằng chứng source code.

---

## Kết luận: KHÔNG có bug mới xác nhận ở M05 (mức code)

Toàn bộ rủi ro authorization/PII trọng tâm của M05 đã được đối chiếu source và **đúng**:

| Rủi ro | Kết quả | Bằng chứng |
|---|---|---|
| IDOR web detail-by-URL (agent/collab/parent/student xem hồ sơ ngoài scope) | **Chặn** | `CandidateDetail.Load` 1074-1082 → `_accessDenied` |
| RB-1 lộ commission/số ứng viên CTV cho parent/student | **Ẩn đúng 2 dòng** | `CollaboratorInfoDialog` 47-51 + `_hideSensitive` 78 |
| RB-2 non-super_admin đổi TVV/CTV/đơn hàng | **Chặn** (re-check + password) | `ChangeAssigneesAsync` 1572, `ChangeJobOrderAsync` 1606 |
| Xóa/sửa hồ sơ sai role | **Chặn** (re-check server-side) | `DeleteCandidate` 1409, `Save` 1394 |
| Mask SĐT ứng viên với CTV | **Áp** | `_maskPhone` 1083 + `MaskPhone` 2148 |

**IDOR REST `/api/candidates` (PII toàn bộ)** — rủi ro nghiêm trọng duy nhất — đã được file **BUG_M02_02 (High)** ở M02, Codex đã fix và Claude đã `Verified (code)`. M05 chỉ tham chiếu + cung cấp bằng chứng runtime (`M02/evidence-M02_02-runtime.md`). **Không nhân đôi bug.**

→ `QA Status = No Confirmed Bugs`. `Codex Status = Not Required` (không có handoff mới). Runtime coverage `Blocked (no harness)` — xem `05-automation-report.md`.

---

## Observations (không nâng thành bug — theo dõi)

- **OBS-M05-01 (Med) — Xóa ứng viên manual cascade:** `DeleteCandidate` xóa thủ công 9 nhóm bảng liên quan trong một DbContext. Đầy đủ ở thời điểm hiện tại, nhưng **không có FK cascade DB** → nếu thêm quan hệ mới tới `candidates` mà quên cập nhật hàm này → orphan. Đề xuất: unit/integration guard liệt kê mọi bảng con, hoặc FK cascade. Theo dõi ở **M17 Dashboard/Data Integrity** hoặc M20.
- **OBS-M05-02 (Low) — List client-side filter:** `Candidates.razor` nạp toàn bộ (theo scope) vào RAM rồi lọc/sort/paginate client → perf ở scale (giống M04 R2). API server-paging có sẵn. Cải tiến, không phải defect.
- **OBS-M05-03 (Low) — Convert race:** không unique `candidates(lead_id)` → 2 request đồng thời có thể tạo trùng (pre-existing, = M04 R3). Theo dõi M07 (unique index có điều kiện).
- **OBS-M05-04 (Low) — Lost update hồ sơ:** `CandidateDialog` edit không có concurrency token → 2 người sửa = last-write-wins. Theo dõi M17.

## Requirement Clarification — ĐÃ CHỐT (user 2026-07-10)

- **U1 — ĐÃ CHỐT: CTV ĐƯỢC xem passport/CCCD.** Collaborator xem chi tiết ứng viên mình giới thiệu **được phép thấy `passport_number`/`cccd_number`** (chỉ SĐT bị mask). → Hành vi hiện tại **ĐÚNG**, không cần mask giấy tờ. **KHÔNG phải bug.** RB-1 giữ nguyên (chỉ ẩn 2 dòng hoa hồng/số ứng viên). TC_M05_028 = **Pass (spec confirmed)**; R8 đóng.
- **U2 — ĐÃ CHỐT: đổi đơn hàng reset workflow KHÔNG hoàn tiền/hoa hồng.** → Hành vi hiện tại **ĐÚNG** (khớp WORKLOG). **KHÔNG phải bug.** Verify chéo M09/M10 rằng không có logic hoàn tiền vô tình.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Status |
|---:|---|---|---|---|---|---|
| — | — | — | — | — | Không có bug mới. IDOR API = BUG_M02_02 (đã Fixed+Verified @M02). | — |
