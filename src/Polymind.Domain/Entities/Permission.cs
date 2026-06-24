using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Quyền chi tiết, vd: leads:create, candidates:read.</summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = default!;   // leads:create
    public string Resource { get; set; } = default!; // leads
    public string Action { get; set; } = default!;   // create/read/update/delete/approve

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
