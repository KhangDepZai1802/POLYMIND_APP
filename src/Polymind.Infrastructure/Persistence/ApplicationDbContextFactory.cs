using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Polymind.Infrastructure.Persistence;

/// <summary>
/// Factory dùng cho design-time (dotnet ef migrations) — cho phép tạo migration mà chỉ cần
/// build project Infrastructure (không phụ thuộc project Web đang chạy/khóa DLL).
/// Phải khớp cấu hình runtime ở <see cref="DependencyInjection"/>: Npgsql + snake_case.
/// Chuỗi kết nối chỉ là placeholder — lệnh `migrations add` không kết nối DB.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=polymind_designtime;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ApplicationDbContext(options);
    }
}
