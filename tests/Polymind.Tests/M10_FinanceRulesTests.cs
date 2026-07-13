using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Domain.Finance;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M10 — Finance. Pin hợp đồng Domain mà logic tài chính phụ thuộc.
/// LƯU Ý PHẠM VI: `PaymentSchedule.Split` (chia 20/30/30/20 + bù dư bước cuối), enforcement đóng
/// tuần tự, trigger hoa hồng… nằm trong `Polymind.Web` → KHÔNG unit-test được ở đây (không ref Web).
/// Xem 05-automation-report.md. Test dưới chốt thứ tự `PaymentStage` (enforcement dùng `(int)stage`
/// để ép đóng 1→4) và default entity.
/// TC_M10_007, TC_M10_009/010/011, TC_M10_030..033.
/// </summary>
public class M10_FinanceRulesTests
{
    [Fact] // TC_M10_030 — 4 bước đóng tiền đúng thứ tự 1→4 (enforcement so sánh (int)stage)
    public void PaymentStage_is_ordered_1_to_4_deposit_to_settlement()
    {
        Assert.Equal(1, (int)PaymentStage.Deposit);
        Assert.Equal(2, (int)PaymentStage.ServiceFee);
        Assert.Equal(3, (int)PaymentStage.PreDeparture);
        Assert.Equal(4, (int)PaymentStage.Settlement);
    }

    [Fact] // TC_M10_031 — khoản thu mới mặc định Pending (chưa ghi nhận)
    public void New_payment_defaults_to_pending()
    {
        var p = new Payment();

        Assert.Equal(PaymentStatus.Pending, p.Status);
    }

    [Fact] // TC_M10_032 — PaymentStatus có đủ trạng thái vòng đời khoản thu
    public void PaymentStatus_contains_lifecycle_states()
    {
        var all = Enum.GetValues<PaymentStatus>();

        Assert.Contains(PaymentStatus.Pending, all);
        Assert.Contains(PaymentStatus.Paid, all);
        Assert.Contains(PaymentStatus.Overdue, all);
        Assert.Contains(PaymentStatus.Refunded, all);
    }

    [Fact] // TC_M10_033 — phiếu thu/chi phân biệt Income vs Expense
    public void ReceiptType_distinguishes_income_and_expense()
    {
        Assert.Equal(new[] { ReceiptType.Income, ReceiptType.Expense }, Enum.GetValues<ReceiptType>());
    }

    [Fact] // BUG_M10_01 / TC_M10_007,009 — không được đóng bước 2 khi bước 1 chưa Paid
    public void Posting_stage_is_blocked_when_an_earlier_stage_is_unpaid()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Pending),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
        };

        Assert.True(PaymentPostingRules.HasUnpaidEarlierStage(PaymentStage.ServiceFee, siblings));
    }

    [Fact] // BUG_M10_01 / TC_M10_009..011 — được đóng khi mọi stage trước đã Paid
    public void Posting_stage_is_allowed_when_earlier_stages_are_paid()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Paid),
            (PaymentStage.Settlement, PaymentStatus.Pending),
        };

        Assert.False(PaymentPostingRules.HasUnpaidEarlierStage(PaymentStage.Settlement, siblings));
    }

    [Fact] // BUG_M10_01 — stage sau không ảnh hưởng việc đóng stage hiện tại
    public void Posting_stage_ignores_unpaid_later_stages()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
        };

        Assert.False(PaymentPostingRules.HasUnpaidEarlierStage(PaymentStage.Deposit, siblings));
    }

    // ===== CR-M10-2 — báo lỗi phải nêu ĐÍCH DANH bước còn thiếu (không nói chung chung "1 → 4") =====

    [Fact] // Duyệt bước 3 khi mới thu bước 1 → phải chỉ ra đúng bước 2 đang thiếu
    public void Unpaid_earlier_stages_names_the_exact_blocking_stage()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
            (PaymentStage.PreDeparture, PaymentStatus.Pending),
        };

        var blocking = PaymentPostingRules.UnpaidEarlierStages(PaymentStage.PreDeparture, siblings);

        Assert.Equal(new[] { PaymentStage.ServiceFee }, blocking);
    }

    [Fact] // Thiếu nhiều bước → liệt kê đủ, theo đúng thứ tự tăng dần
    public void Unpaid_earlier_stages_lists_all_gaps_in_order()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Pending),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
            (PaymentStage.PreDeparture, PaymentStatus.Pending),
            (PaymentStage.Settlement, PaymentStatus.Pending),
        };

        var blocking = PaymentPostingRules.UnpaidEarlierStages(PaymentStage.Settlement, siblings);

        Assert.Equal(
            new[] { PaymentStage.Deposit, PaymentStage.ServiceFee, PaymentStage.PreDeparture },
            blocking);
    }

    [Fact] // Đủ điều kiện → danh sách rỗng (không có gì để báo)
    public void Unpaid_earlier_stages_empty_when_sequence_is_satisfied()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Pending),
        };

        Assert.Empty(PaymentPostingRules.UnpaidEarlierStages(PaymentStage.PreDeparture, siblings));
    }

    [Fact] // Bước 1 không bao giờ bị chặn
    public void Deposit_is_never_blocked()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Pending),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
        };

        Assert.Empty(PaymentPostingRules.UnpaidEarlierStages(PaymentStage.Deposit, siblings));
    }

    [Fact] // Lịch có đúng 4 bước — hằng số dùng trong nhãn "Bước n/4"
    public void Schedule_has_four_stages()
        => Assert.Equal(4, PaymentPostingRules.TotalStages);

    // ===== CR-M10-3 — TÁCH "ứng viên đã nộp" (Submitted) khỏi "kế toán đã duyệt" (Paid) =====
    // Tick bên Tiến độ đóng tiền KHÔNG được tự động duyệt hộ kế toán bên Khoản thu.

    [Fact] // Submitted KHÔNG phải Paid → vẫn chặn duyệt bước sau
    public void Submitted_stage_does_not_satisfy_the_approval_gate()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Submitted), // ứng viên đã nộp, kế toán CHƯA duyệt
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
        };

        var blocking = PaymentPostingRules.UnpaidEarlierStages(PaymentStage.ServiceFee, siblings);

        Assert.Equal(new[] { PaymentStage.Deposit }, blocking);
    }

    [Fact] // …nhưng ứng viên vẫn được phép NỘP bước sau khi bước trước mới chỉ nộp (chưa duyệt)
    public void Submitted_stage_satisfies_the_submit_gate()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Submitted),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
        };

        Assert.Empty(PaymentPostingRules.UnsubmittedEarlierStages(PaymentStage.ServiceFee, siblings));
    }

    [Fact] // Nộp vượt bước vẫn bị chặn — nêu đích danh bước chưa nộp
    public void Submit_gate_blocks_out_of_order_submission()
    {
        var siblings = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Pending),
            (PaymentStage.ServiceFee, PaymentStatus.Pending),
            (PaymentStage.PreDeparture, PaymentStatus.Pending),
        };

        var blocking = PaymentPostingRules.UnsubmittedEarlierStages(PaymentStage.PreDeparture, siblings);

        Assert.Equal(new[] { PaymentStage.Deposit, PaymentStage.ServiceFee }, blocking);
    }

    [Theory]
    [InlineData(PaymentStatus.Submitted, true)]
    [InlineData(PaymentStatus.Paid, true)]
    [InlineData(PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Partial, false)]
    [InlineData(PaymentStatus.Overdue, false)]
    [InlineData(PaymentStatus.Refunded, false)]
    public void IsSubmittedOrPaid_only_accepts_submitted_and_paid(PaymentStatus status, bool expected)
        => Assert.Equal(expected, PaymentPostingRules.IsSubmittedOrPaid(status));

    [Fact] // Submitted nối vào CUỐI enum — giá trị int cũ trong DB không được xê dịch
    public void Submitted_is_appended_without_shifting_existing_values()
    {
        Assert.Equal(0, (int)PaymentStatus.Pending);
        Assert.Equal(1, (int)PaymentStatus.Partial);
        Assert.Equal(2, (int)PaymentStatus.Paid);
        Assert.Equal(3, (int)PaymentStatus.Overdue);
        Assert.Equal(4, (int)PaymentStatus.Refunded);
        Assert.Equal(5, (int)PaymentStatus.Submitted);
    }

    // ===== CR-M10-3 — KHO LƯU TRỮ: không được biến thành đường miễn nợ trá hình =====

    [Fact] // Thu đủ 4/4 → lưu trữ được
    public void Schedule_can_be_archived_when_all_four_stages_are_paid()
    {
        var stages = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Paid),
            (PaymentStage.Settlement, PaymentStatus.Paid),
        };

        Assert.True(PaymentPostingRules.CanArchiveSchedule(stages));
    }

    [Fact] // Còn 1 bước chưa duyệt → KHÔNG lưu trữ được (dù ứng viên đã nộp tiền)
    public void Schedule_cannot_be_archived_while_a_stage_is_only_submitted()
    {
        var stages = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Paid),
            (PaymentStage.Settlement, PaymentStatus.Submitted),
        };

        Assert.False(PaymentPostingRules.CanArchiveSchedule(stages));
    }

    [Fact] // Thiếu hẳn bước 4 trong lịch → KHÔNG lưu trữ được
    public void Schedule_cannot_be_archived_when_a_stage_is_missing()
    {
        var stages = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Paid),
        };

        Assert.False(PaymentPostingRules.CanArchiveSchedule(stages));
    }

    [Fact] // Lịch rỗng → KHÔNG lưu trữ được (fail-closed)
    public void Empty_schedule_cannot_be_archived()
        => Assert.False(PaymentPostingRules.CanArchiveSchedule(
            Array.Empty<(PaymentStage, PaymentStatus)>()));

    [Fact] // Hoàn tiền KHÔNG được tính là đã thu → không lách để lưu trữ
    public void Refunded_stage_does_not_count_as_collected()
    {
        var stages = new[]
        {
            (PaymentStage.Deposit, PaymentStatus.Paid),
            (PaymentStage.ServiceFee, PaymentStatus.Paid),
            (PaymentStage.PreDeparture, PaymentStatus.Paid),
            (PaymentStage.Settlement, PaymentStatus.Refunded),
        };

        Assert.False(PaymentPostingRules.CanArchiveSchedule(stages));
    }
}
