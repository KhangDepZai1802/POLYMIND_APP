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
    public string? Benefits { get; set; } // Đãi ngộ của công ty tiếp nhận (ăn ở, bảo hiểm, tăng ca...)
    public string? Bonus { get; set; }     // Thưởng (thưởng ký HĐ, thưởng năm, thưởng chuyên cần...)
    public JobCategory Category { get; set; } = JobCategory.OverseasJob;
    public DateOnly? PostedDate { get; set; }           // ngày đăng job
    public DateOnly? ApplicationDeadline { get; set; }  // hạn ứng tuyển — sắp hết hạn (≤7 ngày) hiển thị đỏ
    public DateOnly? RecruitmentStartDate { get; set; }
    public DateOnly? ExpectedDepartureDate { get; set; }
    public JobOrderStatus Status { get; set; } = JobOrderStatus.Recruiting;
    public Guid CreatedBy { get; set; }
}
