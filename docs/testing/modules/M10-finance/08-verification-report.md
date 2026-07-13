# M10 — Finance (Payments & Expenses) · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic; chỉ đọc source, chạy test, đánh giá.
> **Ngày:** 2026-07-11 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit; PostgreSQL posting probe do Codex đo, Claude chưa dựng lại DB harness).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M10_01 Medium) | ✔ |
| `07-fix-report.md` | ✔ |
| `Domain/Finance/PaymentPostingRules.cs` (`HasUnpaidEarlierStage`) | ✔ |
| `Web/Finance/PaymentPostingService.cs` (`MarkPaidAsync`, `PaymentPostingResult`) | ✔ |
| `Finance.razor` `ApprovePayment`/`MarkStagePaid` | ✔ |
| `PaymentDialog.razor` `Save`/`ApplyTo` (create + edit transition Paid) | ✔ |
| `CommissionEngine.EnsureAsync` (M09 invariant tái dùng) | ✔ |
| Rà toàn `Web` `PaymentStatus.Paid` (mọi assignment runtime) | ✔ |
| `M10_FinanceRulesTests.cs` | ✔ |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 52, Skipped: 0
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M10_01 — 3 đường set Payment→Paid không đồng nhất (thiếu tuần tự + không trigger commission)

**Kết luận: Verified Fixed (code-level).** PostgreSQL posting probe (out-of-order blocked; 4 stage Paid → 3 commission; thu lẻ không commission; 5 audit) do Codex đo; Claude xác minh cấu trúc + 3 entry point ở source, chưa dựng lại DB harness.

### Bằng chứng đã kiểm

1. **Đường duy nhất set Paid ở runtime:** rà `PaymentStatus.Paid` toàn `Web` — assignment duy nhất là `PaymentPostingService.cs:46` (`payment.Status = PaymentStatus.Paid`). Mọi vị trí khác (`Reports`/`Notifications`/`Home`/`Finance` UI/`PaymentDialog.markingPaid`/`Agents`/`Portal`/`AiAssistant`) chỉ là so sánh/filter đọc, không gán. `DemoDataSeeder` là seed, không phải runtime action.
2. **Service ép tuần tự đúng contract:** `MarkPaidAsync` nếu `Stage` khác null → nạp sibling stages `AsNoTracking` → `PaymentPostingRules.HasUnpaidEarlierStage(stage, siblings)` (`(int)x.Stage < (int)current && Status != Paid`) → fail "Phải đóng các bước trước theo thứ tự 1 → 4." Unit test khóa thứ tự: `Posting_stage_is_blocked_when_an_earlier_stage_is_unpaid`, `Posting_stage_is_allowed_when_earlier_stages_are_paid`, `Posting_stage_ignores_unpaid_later_stages` — tất cả Passed.
3. **Actor/date/audit/save + commission đồng nhất:** service set `ApprovedBy=actorId`, `Status=Paid`, `PaidDate ??= today`, `UpdatedAt`, `AddAudit`, `SaveChangesAsync` (payment lưu trước) rồi `CommissionEngine.EnsureAsync` **chỉ khi `Stage` khác null**. Thu lẻ (`Stage=null`) Paid nhưng `NewCommissions=0`.
4. **3 entry point cùng gọi service:**
   - `Finance.ApprovePayment` (tab Khoản thu, dòng 507) → `MarkPaidAsync`, báo lỗi nếu fail, hiện commission count.
   - `Finance.MarkStagePaid` (tab Tiến độ, dòng 659) → `MarkPaidAsync`.
   - `PaymentDialog.Save`: tách `ApplyTo(..., applyStatus: !markingPaid)` khỏi transition; khi `markingPaid` (cả create lẫn edit) **re-check `payments:approve`** (dòng 145/177) rồi gọi `MarkPaidAsync`; fail ⇒ không lưu edit pending. Không marking-paid ⇒ chỉ audit + save như cũ.
5. **Idempotent transition:** service `if (payment.Status == Paid) return Success(0)` chặn double-post; `EnsureAsync` (M09 unique index) đảm bảo mỗi mốc 1 commission dù gọi từ nhiều path.
6. **U2 giữ nguyên:** không thêm logic Refund/hoàn tiền khi đổi JobOrder; Paid cũ không bị viết lại.

### Không tìm thấy hành vi né bug
- Không sửa expected result; unit test kiểm đúng thứ tự stage. Không tắt authorization — trái lại thêm re-check `payments:approve` cho dialog transition Paid.
- Không migration/schema change; contract/enum không đổi.

### Residual risk (đo lường được)
- Claude chưa dựng lại PostgreSQL posting probe; dựa vào evidence Codex + phân tích tĩnh 3 entry point.
- Boundary "payment lưu trước, commission sau": nếu lỗi KHÔNG thuộc unique idempotency xảy ra ở `EnsureAsync`, payment đã Paid mà lỗi nổi lên — cần điều tra thủ công (cùng boundary M09, đã ghi). Không rollback âm thầm.
- OBS-M10-01 (khoản chi chưa có luồng duyệt — req RB-7), OBS-M10-02..04 chưa đổi.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M10_01 | Medium | **Verified Fixed** (code-level; PostgreSQL posting probe của Codex, Claude chưa dựng lại DB harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed
- **Verification Status:** Verified (code-level) — runtime 3-entry-point posting chưa Claude tự đo, không tuyên bố 100%.
