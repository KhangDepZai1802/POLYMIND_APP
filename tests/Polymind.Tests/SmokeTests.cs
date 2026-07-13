using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>Smoke test: kiểm tra test host chạy được và tham chiếu project thật đã nạp.</summary>
public class SmokeTests
{
    [Fact]
    public void PermissionRegistry_is_referenced_and_loads()
    {
        Assert.NotEmpty(PermissionRegistry.Resources);
        Assert.NotEmpty(PermissionRegistry.Actions);
    }
}
