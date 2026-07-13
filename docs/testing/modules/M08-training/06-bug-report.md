# M08 — Training · 06 Bug Report

> QA: Claude · 2026-07-10. Chỉ ghi bug có bằng chứng. Không sửa business logic.

## Kết luận

**KHÔNG có confirmed bug** ở mức code cho M08 Training.

Các điểm rủi ro đã kiểm và **đúng ở source**:
- Authorization: page `[Authorize("training:read")]` + dialog re-check `training:update`/`training:create` server-side (defense-in-depth).
- IDOR/data-scope: list & detail lọc `AgentScope` fail-closed (self/agent/collaborator/staff) — self-scoped chỉ thấy ứng viên của mình; agent/collaborator giới hạn scope.
- Attribution: `CreatedBy = await AuthStateProvider.GetRequiredUserIdAsync(db)` — **KHÔNG** dính first-user anti-pattern (khác BUG_M04_01/BUG_M06_01).
- Validation: `Math.Clamp(progress,0,100)`; trim/null level+note.
- Audit: `AddAudit` cả 2 flow.
- An toàn hiển thị: deserialize `AttachmentsJson` bọc try/catch (JSON hỏng không crash).
- Timezone: `DateOnly` (không lệch); week-grouping Monday-based.

→ **QA Status = No Confirmed Bugs**, **Codex Status = Fixed**, **Verification Status = Verified (code)** cho CR-M08-1 (Claude phiên #8). Runtime (bUnit/DB/MinIO) pending harness.

## Observations (KHÔNG phải bug chặn — theo dõi)

| ID | Severity | Mô tả | Bằng chứng | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| OBS-M08-01 | Low | Không có rowversion/concurrency token trên `TrainingRecord`. 2 phiên cùng tạo record 1 mảng → phiên sau vi phạm unique `(candidate,track)` → `DbUpdateException` **chưa bắt** → lỗi thô cho user (thay vì thông báo thân thiện). Cập nhật đồng thời → last-write-wins im lặng. | `ApplicationDbContext.cs:170` unique index; `TrainingTrackDialog.SaveAsync` không try/catch DbUpdateException | Cùng lớp OBS-M07-01 (concurrency toàn hệ thống). Xử lý gộp ở M17/M20 hoặc khi thêm rowversion. | Theo dõi |
| OBS-M08-02 / CR-M08-1 | Info / Req | `recruiter`, `document_staff`, `visa_staff`, `accountant` cần xem đào tạo. | `DbSeeder.cs` role map; M08 regression | Đã thêm đúng `training:read`, không cấp quyền mutation. | **✅ Verified Fixed (code) — Claude phiên #8** |
| OBS-M08-03 | Info | `training:delete` được seed cho RM/Consultant/SuperAdmin (`Crud("training")`) nhưng KHÔNG có UI/endpoint dùng → quyền thừa; record & phiếu hiện **immutable** (không xóa/sửa từ UI). | `DbSeeder.cs:51,67`; không có delete trong razor | Chấp nhận (immutable hợp audit) hoặc bổ sung UI xóa nếu nghiệp vụ cần. | **Needs Requirement Clarification (U-M08-3)** — non-blocking |
| OBS-M08-04 | Info | `Training.razor.Load()` nạp **toàn bộ** `JobOrders` vào dictionary chỉ để tra `Category`. Inefficiency nhẹ; không lộ dữ liệu nhạy cảm (chỉ category theo job id). | `Training.razor:204-205` | Tối ưu: chỉ nạp category của các job liên quan. Non-blocking. | Theo dõi (perf → M21) |

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---|---|---|---|---|---|---|---|---|
| 1 | CR-M08-1 | Change | TC_M08_020 | BF-M08-01 | role seed | DbSeeder.cs | M08 role theory | **✅ Verified Fixed (code) — Claude phiên #8** |

> M08 8/8, suite 116/116, Web 0/0. **Đang chờ Claude xác minh độc lập.**
