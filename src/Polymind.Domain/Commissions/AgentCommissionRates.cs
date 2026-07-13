namespace Polymind.Domain.Commissions;

/// <summary>
/// Tỉ lệ hoa hồng chuẩn theo góp ý Vietgroup 07/2026:
/// - Đại lý hưởng TỔNG 5% số tiền mỗi ứng viên phải đóng (cho phép 3-5%),
///   chia theo mốc đóng tiền: 1% đặt cọc / 1.5% trúng tuyển / 2.5% xuất cảnh.
/// - Cộng tác viên (CTV) hưởng 30-40% phần hoa hồng của đại lý (mặc định 35%).
/// Dùng chung cho seeder và bước chuẩn hóa dữ liệu cũ.
/// </summary>
public static class AgentCommissionRates
{
    public const decimal Deposit = 1m;
    public const decimal Selected = 1.5m;
    public const decimal Departure = 2.5m;

    /// <summary>Tổng % hoa hồng đại lý trên số tiền ứng viên đóng.</summary>
    public const decimal Total = Deposit + Selected + Departure; // 5%

    /// <summary>Phần chia mặc định cho CTV (trong khoảng cho phép 30-40%).</summary>
    public const decimal CollaboratorShareDefault = 35m;
    public const decimal CollaboratorShareMin = 30m;
    public const decimal CollaboratorShareMax = 40m;

    public static decimal NormalizeCollaboratorShare(decimal percentage)
        => Math.Clamp(percentage, CollaboratorShareMin, CollaboratorShareMax);

    public static decimal CollaboratorShareAmount(decimal commissionAmount, decimal percentage)
        => Math.Round(
            commissionAmount * NormalizeCollaboratorShare(percentage) / 100m,
            0,
            MidpointRounding.AwayFromZero);
}
