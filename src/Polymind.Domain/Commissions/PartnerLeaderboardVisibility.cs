namespace Polymind.Domain.Commissions;

/// <summary>Ẩn dữ liệu doanh số của đại lý cạnh tranh khỏi partner; staff vẫn xem toàn bộ.</summary>
public static class PartnerLeaderboardVisibility
{
    public static bool CanSeeAgentData(bool isPartnerOnly, Guid? currentAgentId, Guid dataAgentId)
        => !isPartnerOnly || currentAgentId is Guid ownAgentId && ownAgentId == dataAgentId;
}
