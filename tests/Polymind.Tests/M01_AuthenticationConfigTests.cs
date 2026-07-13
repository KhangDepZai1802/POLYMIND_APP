using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polymind.Infrastructure;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M01 — Authentication &amp; Session.
/// Kiểm chứng cấu hình bảo mật của Identity do <see cref="DependencyInjection.AddInfrastructure"/> đăng ký:
/// lockout, chính sách mật khẩu, unique email. Không kết nối DB (chỉ resolve IdentityOptions).
/// TC_M01_003, TC_M01_009, TC_M01_010.
/// </summary>
public class M01_AuthenticationConfigTests
{
    private static IdentityOptions BuildIdentityOptions()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Chuỗi kết nối giả — AddInfrastructure chỉ đọc chuỗi, không mở kết nối lúc đăng ký DI.
                ["ConnectionStrings:Default"] = "Host=localhost;Port=5432;Database=polymind_test;Username=u;Password=p",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(config);
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
    }

    [Fact] // TC_M01_003
    public void Lockout_is_5_attempts_15_minutes()
    {
        var options = BuildIdentityOptions();
        Assert.True(options.Lockout.AllowedForNewUsers);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
    }

    [Fact] // TC_M01_009
    public void Password_policy_requires_length8_digit_upper_lower()
    {
        var p = BuildIdentityOptions().Password;
        Assert.Equal(8, p.RequiredLength);
        Assert.True(p.RequireDigit);
        Assert.True(p.RequireUppercase);
        Assert.True(p.RequireLowercase);
        Assert.False(p.RequireNonAlphanumeric);
    }

    [Fact] // TC_M01_010
    public void User_requires_unique_email()
    {
        Assert.True(BuildIdentityOptions().User.RequireUniqueEmail);
    }

    [Fact] // Xác nhận không bắt confirm email (cho phép login ngay sau tạo)
    public void SignIn_does_not_require_confirmed_account()
    {
        Assert.False(BuildIdentityOptions().SignIn.RequireConfirmedAccount);
    }
}
