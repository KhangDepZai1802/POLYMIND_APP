using Microsoft.EntityFrameworkCore;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Api;

/// <summary>Endpoint đọc cho các tài nguyên liên quan (Ứng viên, Đơn hàng) — cùng mẫu phân trang/RBAC.</summary>
public static class ResourceEndpoints
{
    public static void MapCandidatesApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/candidates").WithTags("Candidates");

        group.MapGet("/", async (
            string? search, int? page, int? pageSize,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var (p, size) = Paging(page, pageSize);
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.Candidates.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c => c.FullName.Contains(s) || c.Code.Contains(s)
                    || (c.Phone != null && c.Phone.Contains(s)));
            }
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((p - 1) * size).Take(size)
                .Select(c => new CandidateDto(
                    c.Id, c.Code, c.FullName, c.Phone, c.Province,
                    c.Gender == null ? null : c.Gender.ToString(),
                    c.PassportNumber, c.CreatedAt))
                .ToListAsync();
            return Results.Ok(new PagedResult<CandidateDto>(items, p, size, total));
        })
        .RequireAuthorization(ApiAuth.Bearer("candidates:read"))
        .WithSummary("Danh sách ứng viên (phân trang, tìm kiếm).");

        group.MapGet("/{id:guid}", async (Guid id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var c = await db.Candidates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c is null
                ? Results.NotFound()
                : Results.Ok(new CandidateDto(c.Id, c.Code, c.FullName, c.Phone, c.Province,
                    c.Gender?.ToString(), c.PassportNumber, c.CreatedAt));
        })
        .RequireAuthorization(ApiAuth.Bearer("candidates:read"))
        .WithSummary("Chi tiết một ứng viên.");
    }

    public static void MapJobOrdersApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/job-orders").WithTags("JobOrders");

        group.MapGet("/", async (
            string? country, int? page, int? pageSize,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var (p, size) = Paging(page, pageSize);
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.JobOrders.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(country))
            {
                var c = country.Trim();
                query = query.Where(j => j.Country.Contains(c));
            }
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((p - 1) * size).Take(size)
                .Select(j => new JobOrderDto(
                    j.Id, j.Code, j.Country, j.CompanyName, j.Field,
                    j.Quantity, j.CostAmount, j.Status.ToString(), j.ExpectedDepartureDate))
                .ToListAsync();
            return Results.Ok(new PagedResult<JobOrderDto>(items, p, size, total));
        })
        .RequireAuthorization(ApiAuth.Bearer("job_orders:read"))
        .WithSummary("Danh sách đơn hàng tuyển dụng.");
    }

    private static (int Page, int Size) Paging(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var size = pageSize is null or < 1 ? 20 : Math.Min(pageSize.Value, 100);
        return (p, size);
    }
}
