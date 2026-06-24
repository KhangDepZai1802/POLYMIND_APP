using System.Text;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Display;

namespace Polymind.Web.Reporting;

/// <summary>Xuất báo cáo ra CSV (mở được bằng Excel). Không ghi file vào repo — stream trực tiếp về client.</summary>
public static class CsvExportEndpoints
{
    public static void MapCsvExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/export").RequireAuthorization("reports:read");

        group.MapGet("/finance-monthly.csv", async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var payments = await db.Payments.Where(p => p.Status == PaymentStatus.Paid)
                .Select(p => new { p.Amount, p.PaidDate, p.CreatedAt }).ToListAsync();
            var expenses = await db.Expenses.Select(e => new { e.Amount, e.ExpenseDate }).ToListAsync();

            var today = DateTime.UtcNow.Date;
            var first = new DateOnly(today.Year, today.Month, 1);
            var rows = new List<string[]>();
            for (int i = 11; i >= 0; i--)
            {
                var m = first.AddMonths(-i);
                var rev = payments.Where(p => Same((p.PaidDate ?? DateOnly.FromDateTime(p.CreatedAt.UtcDateTime)), m)).Sum(p => p.Amount);
                var exp = expenses.Where(e => Same(e.ExpenseDate, m)).Sum(e => e.Amount);
                rows.Add(new[] { m.ToString("MM/yyyy"), rev.ToString("0"), exp.ToString("0"), (rev - exp).ToString("0") });
            }
            return Csv("thu-chi-theo-thang", new[] { "Tháng", "Doanh thu", "Chi phí", "Lợi nhuận" }, rows);
        });

        group.MapGet("/commissions.csv", async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var agentNames = await db.Agents.ToDictionaryAsync(a => a.Id, a => a.Name);
            var commissions = await db.AgentCommissions
                .Select(c => new { c.AgentId, c.CommissionAmount, c.Status }).ToListAsync();
            var rows = commissions.GroupBy(c => c.AgentId)
                .Select(g => new
                {
                    Name = agentNames.GetValueOrDefault(g.Key, "—"),
                    Count = g.Count(),
                    Paid = g.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.CommissionAmount),
                    Total = g.Sum(c => c.CommissionAmount)
                })
                .OrderByDescending(r => r.Total)
                .Select(r => new[] { r.Name, r.Count.ToString(), r.Paid.ToString("0"), (r.Total - r.Paid).ToString("0"), r.Total.ToString("0") })
                .ToList();
            return Csv("hoa-hong-theo-dai-ly", new[] { "Đại lý", "Số mốc", "Đã chi", "Chờ/đã duyệt", "Tổng hoa hồng" }, rows);
        });

        group.MapGet("/overdue-payments.csv", async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var names = await db.Candidates.ToDictionaryAsync(c => c.Id, c => c.FullName);
            var open = await db.Payments
                .Where(p => p.Status != PaymentStatus.Paid && p.Status != PaymentStatus.Refunded)
                .Select(p => new { p.CandidateId, p.PaymentType, p.Amount, p.DueDate, p.Status }).ToListAsync();
            var rows = open
                .Where(p => p.Status == PaymentStatus.Overdue || (p.DueDate != null && p.DueDate < today))
                .OrderBy(p => p.DueDate)
                .Select(p => new[]
                {
                    names.GetValueOrDefault(p.CandidateId, "—"),
                    Labels.Vi(p.PaymentType),
                    p.Amount.ToString("0"),
                    p.DueDate?.ToString("dd/MM/yyyy") ?? "",
                    (p.DueDate is null ? 0 : today.DayNumber - p.DueDate.Value.DayNumber).ToString()
                })
                .ToList();
            return Csv("khoan-thu-qua-han", new[] { "Ứng viên", "Loại", "Số tiền", "Hạn thu", "Số ngày quá hạn" }, rows);
        });
    }

    private static bool Same(DateOnly d, DateOnly m) => d.Year == m.Year && d.Month == m.Month;

    private static IResult Csv(string fileBase, string[] header, IEnumerable<string[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(Escape)));
        foreach (var r in rows)
            sb.AppendLine(string.Join(",", r.Select(Escape)));

        // BOM UTF-8 để Excel nhận đúng dấu tiếng Việt.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fileName = $"{fileBase}-{DateTime.Now:yyyyMMdd}.csv";
        return Results.File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string Escape(string? value)
    {
        value ??= "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
