using Polymind.Domain.Enums;

namespace Polymind.Web.Display;

/// <summary>
/// Lịch đóng tiền 4 bước của ứng viên, tính theo chi phí đơn hàng (JobOrder.CostAmount).
/// Tỉ lệ: Đặt cọc 20% / Phí dịch vụ 30% / Phí trước xuất cảnh 30% / Tất toán 20%.
/// </summary>
public static class PaymentSchedule
{
    public static readonly (PaymentStage Stage, double Ratio)[] Stages =
    {
        (PaymentStage.Deposit,      0.20),
        (PaymentStage.ServiceFee,   0.30),
        (PaymentStage.PreDeparture, 0.30),
        (PaymentStage.Settlement,   0.20),
    };

    public static int Percent(PaymentStage stage) =>
        (int)Math.Round(Stages.First(s => s.Stage == stage).Ratio * 100);

    /// <summary>Chia tổng chi phí thành 4 phần theo tỉ lệ; bước cuối nhận phần dư để tổng khớp tuyệt đối.</summary>
    public static Dictionary<PaymentStage, decimal> Split(decimal total)
    {
        var result = new Dictionary<PaymentStage, decimal>();
        decimal running = 0;
        for (int i = 0; i < Stages.Length; i++)
        {
            decimal amount;
            if (i == Stages.Length - 1)
            {
                amount = total - running; // bước cuối lấy phần còn lại
            }
            else
            {
                amount = Math.Round(total * (decimal)Stages[i].Ratio, 0, MidpointRounding.AwayFromZero);
                running += amount;
            }
            result[Stages[i].Stage] = amount;
        }
        return result;
    }

    public static decimal AmountFor(decimal total, PaymentStage stage) =>
        Split(total).GetValueOrDefault(stage);
}
