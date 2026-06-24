using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Đại lý / cộng tác viên giới thiệu ứng viên (Module 5 / mục 8.1).</summary>
public class Agent : BaseEntity
{
    public string Code { get; set; } = default!; // AG-XXXXXX
    public string Name { get; set; } = default!;
    public string? RepresentativeName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public string? ContractUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? UserId { get; set; } // tài khoản Portal đại lý

    public ICollection<AgentCommissionConfig> CommissionConfigs { get; set; } = new List<AgentCommissionConfig>();
    public ICollection<AgentCommission> Commissions { get; set; } = new List<AgentCommission>();
}
