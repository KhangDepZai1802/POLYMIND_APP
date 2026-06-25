using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Auditing;

namespace Polymind.Web.Api;

/// <summary>REST CRUD cho Lead — minh họa đầy đủ đọc/ghi + phân trang + RBAC theo <c>leads:*</c>.</summary>
public static class LeadsEndpoints
{
    public static void MapLeadsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads");

        // GET danh sách (phân trang + tìm kiếm + lọc trạng thái).
        group.MapGet("/", async (
            string? search, LeadStatus? status, int? page, int? pageSize,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var (p, size) = Paging(page, pageSize);
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.Leads.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(l => l.FullName.Contains(s)
                    || (l.Phone != null && l.Phone.Contains(s))
                    || (l.Email != null && l.Email.Contains(s))
                    || l.Code.Contains(s));
            }
            if (status is not null)
                query = query.Where(l => l.Status == status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((p - 1) * size).Take(size)
                .Select(l => Map(l))
                .ToListAsync();
            return Results.Ok(new PagedResult<LeadDto>(items, p, size, total));
        })
        .RequireAuthorization(ApiAuth.Bearer("leads:read"))
        .WithSummary("Danh sách Lead (phân trang, tìm kiếm, lọc trạng thái).");

        // GET chi tiết.
        group.MapGet("/{id:guid}", async (Guid id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var lead = await db.Leads.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            return lead is null ? Results.NotFound() : Results.Ok(Map(lead));
        })
        .RequireAuthorization(ApiAuth.Bearer("leads:read"))
        .WithSummary("Chi tiết một Lead.");

        // POST tạo mới.
        group.MapPost("/", async (
            LeadCreateRequest request, ClaimsPrincipal principal,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { message = "Họ tên là bắt buộc." });

            await using var db = await dbFactory.CreateDbContextAsync();
            var lead = new Lead
            {
                Code = $"LD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                FullName = request.FullName.Trim(),
                Phone = request.Phone,
                Email = request.Email,
                Province = request.Province,
                Address = request.Address,
                Occupation = request.Occupation,
                TargetCountry = request.TargetCountry,
                Source = request.Source,
                Notes = request.Notes,
                Status = LeadStatus.New,
            };
            db.Leads.Add(lead);
            db.AddAudit(principal.UserId(), "create", "leads", lead.Id, null, new { lead.Code, lead.FullName });
            await db.SaveChangesAsync();
            return Results.Created($"/api/leads/{lead.Id}", Map(lead));
        })
        .RequireAuthorization(ApiAuth.Bearer("leads:create"))
        .WithSummary("Tạo Lead mới.");

        // PUT cập nhật.
        group.MapPut("/{id:guid}", async (
            Guid id, LeadUpdateRequest request, ClaimsPrincipal principal,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { message = "Họ tên là bắt buộc." });

            await using var db = await dbFactory.CreateDbContextAsync();
            var lead = await db.Leads.FirstOrDefaultAsync(l => l.Id == id);
            if (lead is null) return Results.NotFound();

            var before = new { lead.FullName, lead.Phone, lead.Status };
            lead.FullName = request.FullName.Trim();
            lead.Phone = request.Phone;
            lead.Email = request.Email;
            lead.Province = request.Province;
            lead.Address = request.Address;
            lead.Occupation = request.Occupation;
            lead.TargetCountry = request.TargetCountry;
            lead.Source = request.Source;
            lead.Status = request.Status;
            lead.Notes = request.Notes;
            lead.UpdatedAt = DateTimeOffset.UtcNow;
            db.AddAudit(principal.UserId(), "update", "leads", lead.Id, before, new { lead.FullName, lead.Phone, lead.Status });
            await db.SaveChangesAsync();
            return Results.Ok(Map(lead));
        })
        .RequireAuthorization(ApiAuth.Bearer("leads:update"))
        .WithSummary("Cập nhật Lead.");

        // DELETE.
        group.MapDelete("/{id:guid}", async (
            Guid id, ClaimsPrincipal principal,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var lead = await db.Leads.FirstOrDefaultAsync(l => l.Id == id);
            if (lead is null) return Results.NotFound();
            db.Leads.Remove(lead);
            db.AddAudit(principal.UserId(), "delete", "leads", id, new { lead.Code, lead.FullName }, null);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization(ApiAuth.Bearer("leads:delete"))
        .WithSummary("Xóa Lead.");
    }

    private static (int Page, int Size) Paging(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var size = pageSize is null or < 1 ? 20 : Math.Min(pageSize.Value, 100);
        return (p, size);
    }

    private static LeadDto Map(Lead l) => new(
        l.Id, l.Code, l.FullName, l.Phone, l.Email, l.Province,
        l.Source.ToString(), l.Status.ToString(), l.CreatedAt);
}
