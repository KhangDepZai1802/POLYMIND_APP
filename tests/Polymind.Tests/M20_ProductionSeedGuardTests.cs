using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M20 — Chốt cổng seed trước khi go-live: production TUYỆT ĐỐI không được tạo tài khoản mẫu
/// dùng chung mật khẩu `Admin@123`. Đây là rủi ro nghiêm trọng nhất khi mở web ra Internet thật.
/// LƯU Ý PHẠM VI: đây là test chốt HỢP ĐỒNG của nhánh seed, không phải bằng chứng runtime.
/// Kiểm chứng thật nằm ở checklist go-live: đăng nhập `admin@polymind.local / Admin@123` phải THẤT BẠI.
/// </summary>
public class M20_ProductionSeedGuardTests
{
    [Fact] // Production không seed bất kỳ tài khoản mẫu nào
    public void Production_does_not_seed_sample_users()
        => Assert.False(DbSeeder.ShouldSeedSampleUsers(isDevelopment: false));

    [Fact] // Development vẫn seed đủ tài khoản mẫu để test RBAC
    public void Development_still_seeds_sample_users()
        => Assert.True(DbSeeder.ShouldSeedSampleUsers(isDevelopment: true));

    [Fact] // Mật khẩu mặc định chỉ tồn tại cho môi trường dev — nếu đổi hằng số này phải rà lại toàn bộ tài liệu go-live
    public void Default_admin_credentials_are_the_known_dev_only_values()
    {
        Assert.Equal("admin@polymind.local", DbSeeder.DefaultAdminEmail);
        Assert.Equal("Admin@123", DbSeeder.DefaultAdminPassword);
    }
}
