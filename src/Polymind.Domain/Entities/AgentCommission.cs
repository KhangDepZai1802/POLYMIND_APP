using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Hoa hồng thực tế phát sinh theo từng mốc.</summary>
public class AgentCommission : BaseEntity
{
    public Guid AgentId { get; set; }
    public Agent Agent { get; set; } = default!;
    public Guid CandidateId { get; set; }
    public Guid JobOrderId { get; set; }
    public Guid? ConfigId { get; set; }
    public CommissionMilestone Milestone { get; set; }
    /// <summary>Giai đoạn đóng tiền đã kích hoạt lát hoa hồng này (góp ý Vietgroup: chia theo giai đoạn đóng tiền). Null = dữ liệu cũ theo mốc workflow.</summary>
    public PaymentStage? Stage { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    /// <summary>CTV trực tiếp tại thời điểm hoa hồng phát sinh. Null khi ứng viên không do CTV giới thiệu.</summary>
    public Guid? CollaboratorId { get; set; }
    /// <summary>Snapshot % CTV tại thời điểm phát sinh; không bị thay đổi khi cấu hình CTV đổi sau này.</summary>
    public decimal? CollaboratorSharePercentage { get; set; }
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public DateOnly? PaidDate { get; set; }
    public Guid? ReceiptId { get; set; }
    public Guid? ApprovedBy { get; set; }
}
