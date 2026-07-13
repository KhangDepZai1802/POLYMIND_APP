using Polymind.Domain.Enums;

namespace Polymind.Domain.Commissions;

/// <summary>Luật chuyển trạng thái hoa hồng; mọi caller phải kiểm tra lại trên dữ liệu DB hiện tại.</summary>
public static class AgentCommissionTransitions
{
    public static bool CanApprove(CommissionStatus current)
        => current == CommissionStatus.Pending;

    public static bool CanMarkPaid(CommissionStatus current)
        => current == CommissionStatus.Approved;
}
