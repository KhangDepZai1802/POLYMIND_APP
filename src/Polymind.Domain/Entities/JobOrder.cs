using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Đơn hàng tuyển dụng (Module 3 / mục 5).</summary>
public class JobOrder : BaseEntity
{
    public string Code { get; set; } = default!; // JO-YYYYMM-XXX
    public string Country { get; set; } = default!;
    public string? UnionName { get; set; }
    public string? CompanyName { get; set; }
    public string? Field { get; set; }
    public int Quantity { get; set; }
    public string? SalaryDescription { get; set; }
    public decimal? CostAmount { get; set; }
    public string? Requirements { get; set; }
    public DateOnly? RecruitmentStartDate { get; set; }
    public DateOnly? ExpectedDepartureDate { get; set; }
    public JobOrderStatus Status { get; set; } = JobOrderStatus.Recruiting;
    public Guid CreatedBy { get; set; }
}
