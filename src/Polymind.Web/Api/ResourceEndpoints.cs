using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Security;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Api;

/// <summary>Endpoint đọc cho các tài nguyên liên quan (Ứng viên, Đơn hàng) — cùng mẫu phân trang/RBAC.</summary>
public static class ResourceEndpoints
{
    public static void MapCandidatesApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/candidates").WithTags("Candidates");

        group.MapGet("/", async (
            string? search, int? page, int? pageSize,
            ClaimsPrincipal principal,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var (p, size) = Paging(page, pageSize);
            await using var db = await dbFactory.CreateDbContextAsync();
            var scope = await ResolveCandidateScopeAsync(principal, db);
            var query = scope.Apply(db.Candidates.AsNoTracking());
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

        group.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal principal,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var scope = await ResolveCandidateScopeAsync(principal, db);
            var c = await scope.Apply(db.Candidates.AsNoTracking())
                .FirstOrDefaultAsync(x => x.Id == id);
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

    private static async Task<CandidateAccessScope> ResolveCandidateScopeAsync(
        ClaimsPrincipal principal,
        ApplicationDbContext db)
    {
        var hasStaffRole = CandidateFullAccessRoles.Any(principal.IsInRole);
        if (hasStaffRole)
            return CandidateAccessScope.All;

        var userId = principal.UserId();
        if (userId is null)
            return CandidateAccessScope.None;

        var isAgentOnly = principal.IsInRole(RoleNames.Agent);
        var isCollaboratorOnly = !isAgentOnly && principal.IsInRole(RoleNames.Collaborator);
        var isSelfScoped = !isAgentOnly && !isCollaboratorOnly
            && (principal.IsInRole(RoleNames.Parent) || principal.IsInRole(RoleNames.Student));

        if (isAgentOnly)
        {
            var agentId = await db.Agents.AsNoTracking()
                .Where(agent => agent.UserId == userId)
                .Select(agent => (Guid?)agent.Id)
                .FirstOrDefaultAsync();
            return agentId is Guid id ? CandidateAccessScope.ForAgent(id) : CandidateAccessScope.None;
        }

        if (isCollaboratorOnly)
        {
            var collaboratorId = await db.Collaborators.AsNoTracking()
                .Where(collaborator => collaborator.UserId == userId)
                .Select(collaborator => (Guid?)collaborator.Id)
                .FirstOrDefaultAsync();
            return collaboratorId is Guid id
                ? CandidateAccessScope.ForCollaborator(id)
                : CandidateAccessScope.None;
        }

        return isSelfScoped ? CandidateAccessScope.ForUser(userId.Value) : CandidateAccessScope.None;
    }

    private static readonly string[] CandidateFullAccessRoles =
    {
        RoleNames.SuperAdmin,
        RoleNames.Director,
        RoleNames.RecruitmentManager,
        RoleNames.Recruiter,
        RoleNames.Consultant,
        RoleNames.DocumentStaff,
        RoleNames.VisaStaff,
        RoleNames.Accountant,
    };
}
