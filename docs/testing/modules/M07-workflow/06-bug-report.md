# M07 — Candidate Workflow · Bug Report

Chỉ ghi bug có bằng chứng source code.

---

## Kết luận: KHÔNG có bug xác nhận ở M07 (mức code)

State-machine 20 bước và phân quyền được đối chiếu source và **đúng**:

| Rủi ro trọng tâm | Kết quả | Bằng chứng |
|---|---|---|
| Vertical escalation (sai quyền chuyển bước) | **Chặn** | `CanAdvance` re-check ở cả 3 mutation (1723/1857/1911) |
| Nhảy/bỏ bước tùy ý | **Chặn** | `WorkflowSteps.Next` tuần tự; 7.5 chỉ vào qua fail B8 |
| Reselect gắn lại đơn cũ / hết hạn | **Chặn** | `ReassignJobOrder` new≠old + deadline (1924/1931) |
| Hoàn thành khi còn nợ vay | **Chặn** | `_hasOpenLoan` gate B20 (1777) |
| Attribution sai (first-user anti-pattern) | **Không dính** | dùng `GetRequiredUserIdAsync` (actor thật) mọi record |

→ `QA Status = No Confirmed Bugs`, `Codex Status = Not Required`. Runtime `Blocked (no harness)`.

---

## Observations (theo dõi — không nâng thành bug)

- **OBS-M07-01 (Med) — Concurrency/stale-state khi chuyển bước:** `candidate_job_orders` không có concurrency token (rowversion). Trong `AdvanceStep`, validation switch dùng `_cjo.CurrentStep` (**cached** từ Load) trong khi advance thực dùng `cjo.CurrentStep` (**fresh** từ DB). Nếu hai actor (hoặc double-click qua hai circuit) chuyển bước cùng lúc: có thể double-advance/skip bước hoặc validate theo bước cũ. `_busy` chỉ chặn re-entry trong MỘT circuit. **Chưa có bằng chứng data-corruption** (cần integration/concurrency test — TC_M07_020) → ghi observation, không file bug. Đề xuất: rowversion + re-validate step fresh trước khi advance. Theo dõi ở M17/M20.
- **OBS-M07-02 (Low) — Commission trigger phụ thuộc M09:** `AdvanceStep` gọi `CommissionEngine.EnsureAsync` mỗi lần; comment khẳng định idempotent. Verify thực tế ở **M09**.

## Requirement Clarification — ĐÃ CHỐT (user 2026-07-10)

- **U1 (= M05 U2) — ĐÃ CHỐT:** RB-2 `ChangeJobOrderAsync` (super_admin đổi đơn hàng) reset tiến trình 20 bước **KHÔNG hoàn/hủy khoản thu + hoa hồng đã phát sinh** (khớp WORKLOG). → Hành vi hiện tại **ĐÚNG, không phải bug.** Verify chéo M09/M10: xác nhận không có logic hoàn tiền/hủy hoa hồng vô tình khi reset.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Status |
|---:|---|---|---|---|---|---|
| — | — | — | — | — | Không có bug. OBS-M07-01 (concurrency) theo dõi ở M17/M20. | — |
