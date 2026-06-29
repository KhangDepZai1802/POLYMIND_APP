using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Display;

public static class FinanceEligibility
{
    public static async Task<HashSet<Guid>> CandidateIdsAsync(ApplicationDbContext db)
    {
        var latest = await LatestAssignmentsAsync(db);

        return latest
            .Where(c => c.CurrentStep >= WorkflowStep.Deposit)
            .Select(c => c.CandidateId)
            .ToHashSet();
    }

    public static async Task<Dictionary<Guid, Guid>> CandidateJobOrderIdsAsync(ApplicationDbContext db, Guid? includeCandidateId = null)
    {
        var latest = await LatestAssignmentsAsync(db);

        return latest
            .Where(c => c.CurrentStep >= WorkflowStep.Deposit || c.CandidateId == includeCandidateId)
            .ToDictionary(c => c.CandidateId, c => c.JobOrderId);
    }

    private static async Task<List<LatestAssignment>> LatestAssignmentsAsync(ApplicationDbContext db)
    {
        var rows = await db.CandidateJobOrders
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new LatestAssignment(c.CandidateId, c.JobOrderId, c.CurrentStep, c.CreatedAt))
            .ToListAsync();

        return rows
            .GroupBy(c => c.CandidateId)
            .Select(g => g.First())
            .ToList();
    }

    private sealed record LatestAssignment(Guid CandidateId, Guid JobOrderId, WorkflowStep CurrentStep, DateTimeOffset CreatedAt);
}
