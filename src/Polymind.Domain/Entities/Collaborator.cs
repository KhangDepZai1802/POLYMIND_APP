using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Cộng tác viên (CTV) trực thuộc một đại lý; là người trực tiếp giới thiệu ứng viên.</summary>
public class Collaborator : BaseEntity
{
    public string Code { get; set; } = default!; // CTV-XXXX
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    public decimal CommissionSharePercentage { get; set; } = 50;
    public Guid AgentId { get; set; } // CTV thuộc đại lý nào
    public Agent Agent { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public Guid? UserId { get; set; } // tài khoản đăng nhập (nếu có)
}
