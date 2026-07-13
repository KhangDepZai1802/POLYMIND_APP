using Polymind.Domain.Entities;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M19 — Audit Log. Pin hợp đồng entity mà `AuditLogHelpers.AddAudit` + trang nhật ký (`Admin.razor`) dựa vào.
/// Logic ghi/đọc (AddAudit, LoadAuditAsync filter/label) nằm trong Polymind.Web → cần integration/UI harness,
/// không unit-test được từ test project (không ref Web). Các test dưới chốt default/nullable contract của entity.
/// TC_M19_030, TC_M19_031, TC_M19_032 (Ip/UserAgent không tự set → OBS-M19-01).
/// </summary>
public class M19_AuditLogTests
{
    [Fact] // TC_M19_030 — audit mới: mọi field tùy chọn = null (gồm Ip/UserAgent không tự set)
    public void New_audit_log_defaults_optional_fields_to_null()
    {
        var log = new AuditLog();

        Assert.Null(log.UserId);
        Assert.Null(log.ResourceId);
        Assert.Null(log.OldValue);
        Assert.Null(log.NewValue);
        // OBS-M19-01: entity có 2 cột forensic nhưng không đường nào set → luôn null.
        Assert.Null(log.IpAddress);
        Assert.Null(log.UserAgent);
    }

    [Fact] // TC_M19_031 — Id + CreatedAt tự sinh (view OrderByDescending(CreatedAt) dựa vào)
    public void New_audit_log_generates_id_and_created_at()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var log = new AuditLog();

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.True(log.CreatedAt >= before, "CreatedAt phải được gán mặc định UtcNow");
    }

    [Fact] // TC_M19_032 — Action/Resource là dữ liệu bắt buộc do caller cung cấp; nhận đủ giá trị
    public void Audit_log_stores_action_resource_and_json_values()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var log = new AuditLog
        {
            UserId = userId,
            Action = "update_role",
            Resource = "users",
            ResourceId = resourceId,
            OldValue = "{\"Roles\":\"recruiter\"}",
            NewValue = "{\"Role\":\"director\"}",
        };

        Assert.Equal(userId, log.UserId);
        Assert.Equal("update_role", log.Action);
        Assert.Equal("users", log.Resource);
        Assert.Equal(resourceId, log.ResourceId);
        Assert.Contains("director", log.NewValue);
    }
}
