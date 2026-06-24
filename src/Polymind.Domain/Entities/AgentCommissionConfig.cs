using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Cấu hình % hoa hồng theo mốc (mục 8.2/8.3).</summary>
public class AgentCommissionConfig : BaseEntity
{
    public Guid AgentId { get; set; }
    public Agent Agent { get; set; } = default!;
    public Guid? JobOrderId { get; set; } // null = áp dụng mọi đơn
    public string? Country { get; set; }  // null = áp dụng mọi quốc gia
    public CommissionMilestone Milestone { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FixedAmount { get; set; }
}
