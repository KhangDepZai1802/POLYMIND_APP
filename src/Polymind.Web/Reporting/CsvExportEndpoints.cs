using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Display;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Polymind.Web.Reporting;

/// <summary>
/// Xuất báo cáo (CSV/Excel/PDF) và in phiếu thu/chi PDF. Stream trực tiếp về client,
/// không ghi file vào repo. Gated <c>reports:read</c> / <c>receipts:read</c>.
/// </summary>
public static class CsvExportEndpoints
{
    private sealed record ReportTable(string Title, string FileBase, string[] Header, List<string[]> Rows);

    public static void MapCsvExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/export").RequireAuthorization("reports:read");
        Register(group, "finance-monthly", BuildFinanceMonthlyAsync);
        Register(group, "commissions", BuildCommissionsAsync);
        Register(group, "overdue-payments", BuildOverdueAsync);
        Register(group, "revenue-by-country", BuildRevenueByCountryAsync);
        Register(group, "revenue-by-job-order", BuildRevenueByJobOrderAsync);
        Register(group, "lead-by-province", BuildLeadByProvinceAsync);
        Register(group, "recruitment-funnel", BuildRecruitmentFunnelAsync);
        Register(group, "top-agents", BuildTopAgentsAsync);

        // In phiếu thu/chi PDF.
        app.MapGet("/receipts/{id:guid}.pdf", async (Guid id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var r = await db.Receipts.FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return Results.NotFound();
            var party = r.CandidateId is not null
                ? await db.Candidates.Where(c => c.Id == r.CandidateId).Select(c => c.FullName).FirstOrDefaultAsync()
                : r.AgentId is not null
                    ? await db.Agents.Where(a => a.Id == r.AgentId).Select(a => a.Name).FirstOrDefaultAsync()
                    : null;
            var bytes = ReceiptPdf(r.Code, r.ReceiptType, party, r.Amount, r.ReceiptDate, r.Description);
            return Results.File(bytes, "application/pdf", $"{r.Code}.pdf");
        }).RequireAuthorization("receipts:read");
    }

    private static void Register(RouteGroupBuilder group, string slug,
        Func<ApplicationDbContext, Task<ReportTable>> builder)
    {
        group.MapGet($"/{slug}.csv", async (IDbContextFactory<ApplicationDbContext> f) => Csv(await Build(f, builder)));
        group.MapGet($"/{slug}.xlsx", async (IDbContextFactory<ApplicationDbContext> f) => Xlsx(await Build(f, builder)));
        group.MapGet($"/{slug}.pdf", async (IDbContextFactory<ApplicationDbContext> f) => Pdf(await Build(f, builder)));
    }

    private static async Task<ReportTable> Build(IDbContextFactory<ApplicationDbContext> f,
        Func<ApplicationDbContext, Task<ReportTable>> builder)
    {
        await using var db = await f.CreateDbContextAsync();
        return await builder(db);
    }

    // ---------- Data builders ----------

    private static async Task<ReportTable> BuildFinanceMonthlyAsync(ApplicationDbContext db)
    {
        var payments = await db.Payments.Where(p => p.Status == PaymentStatus.Paid)
            .Select(p => new { p.Amount, p.PaidDate, p.CreatedAt }).ToListAsync();
        var expenses = await db.Expenses.Select(e => new { e.Amount, e.ExpenseDate }).ToListAsync();

        var today = DateTime.UtcNow.Date;
        var first = new DateOnly(today.Year, today.Month, 1);
        var rows = new List<string[]>();
        for (int i = 11; i >= 0; i--)
        {
            var m = first.AddMonths(-i);
            var rev = payments.Where(p => Same(p.PaidDate ?? DateOnly.FromDateTime(p.CreatedAt.UtcDateTime), m)).Sum(p => p.Amount);
            var exp = expenses.Where(e => Same(e.ExpenseDate, m)).Sum(e => e.Amount);
            rows.Add(new[] { m.ToString("MM/yyyy"), rev.ToString("N0"), exp.ToString("N0"), (rev - exp).ToString("N0") });
        }
        return new ReportTable("Doanh thu / Chi phí theo tháng", "thu-chi-theo-thang",
            new[] { "Tháng", "Doanh thu", "Chi phí", "Lợi nhuận" }, rows);
    }

    private static async Task<ReportTable> BuildCommissionsAsync(ApplicationDbContext db)
    {
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
            .Select(r => new[] { r.Name, r.Count.ToString(), r.Paid.ToString("N0"), (r.Total - r.Paid).ToString("N0"), r.Total.ToString("N0") })
            .ToList();
        return new ReportTable("Hoa hồng theo đại lý", "hoa-hong-theo-dai-ly",
            new[] { "Đại lý", "Số mốc", "Đã chi", "Chờ/đã duyệt", "Tổng hoa hồng" }, rows);
    }

    private static async Task<ReportTable> BuildOverdueAsync(ApplicationDbContext db)
    {
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
                p.Amount.ToString("N0"),
                p.DueDate?.ToString("dd/MM/yyyy") ?? "",
                (p.DueDate is null ? 0 : today.DayNumber - p.DueDate.Value.DayNumber).ToString()
            })
            .ToList();
        return new ReportTable("Khoản thu quá hạn", "khoan-thu-qua-han",
            new[] { "Ứng viên", "Loại", "Số tiền", "Hạn thu", "Số ngày quá hạn" }, rows);
    }

    private static async Task<ReportTable> BuildRevenueByCountryAsync(ApplicationDbContext db)
    {
        var payments = await db.Payments.Where(p => p.Status == PaymentStatus.Paid)
            .Select(p => new { p.CandidateId, p.JobOrderId, p.Amount }).ToListAsync();
        var jobs = await db.JobOrders.Select(j => new { j.Id, j.Country }).ToDictionaryAsync(j => j.Id);
        var rows = payments
            .Where(p => p.JobOrderId is not null && jobs.ContainsKey(p.JobOrderId.Value))
            .Select(p => new { Payment = p, Job = jobs[p.JobOrderId!.Value] })
            .GroupBy(x => x.Job.Country)
            .Select(g => new
            {
                Country = g.Key,
                Orders = g.Select(x => x.Job.Id).Distinct().Count(),
                Candidates = g.Select(x => x.Payment.CandidateId).Distinct().Count(),
                Revenue = g.Sum(x => x.Payment.Amount)
            })
            .OrderByDescending(x => x.Revenue)
            .Select(x => new[] { x.Country, x.Orders.ToString(), x.Candidates.ToString(), x.Revenue.ToString("N0") })
            .ToList();

        return new ReportTable("Doanh thu theo quốc gia", "doanh-thu-theo-quoc-gia",
            new[] { "Quốc gia", "Số đơn hàng", "Ứng viên đã thu", "Doanh thu" }, rows);
    }

    private static async Task<ReportTable> BuildRevenueByJobOrderAsync(ApplicationDbContext db)
    {
        var payments = await db.Payments.Where(p => p.Status == PaymentStatus.Paid)
            .Select(p => new { p.CandidateId, p.JobOrderId, p.Amount }).ToListAsync();
        var jobs = await db.JobOrders
            .Select(j => new { j.Id, j.Code, j.Country, j.CompanyName })
            .ToDictionaryAsync(j => j.Id);
        var rows = payments
            .Where(p => p.JobOrderId is not null && jobs.ContainsKey(p.JobOrderId.Value))
            .Select(p => new { Payment = p, Job = jobs[p.JobOrderId!.Value] })
            .GroupBy(x => x.Job.Id)
            .Select(g =>
            {
                var job = jobs[g.Key];
                var name = string.IsNullOrWhiteSpace(job.CompanyName)
                    ? job.Code
                    : $"{job.Code} — {job.CompanyName}";
                return new
                {
                    Name = name,
                    job.Country,
                    Candidates = g.Select(x => x.Payment.CandidateId).Distinct().Count(),
                    Revenue = g.Sum(x => x.Payment.Amount)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Select(x => new[] { x.Name, x.Country, x.Candidates.ToString(), x.Revenue.ToString("N0") })
            .ToList();

        return new ReportTable("Doanh thu theo đơn hàng", "doanh-thu-theo-don-hang",
            new[] { "Đơn hàng", "Quốc gia", "Ứng viên đã thu", "Doanh thu" }, rows);
    }

    private static async Task<ReportTable> BuildLeadByProvinceAsync(ApplicationDbContext db)
    {
        var rows = await db.Leads
            .GroupBy(l => string.IsNullOrWhiteSpace(l.Province) ? "Chưa rõ" : l.Province!.Trim())
            .Select(g => new { Province = g.Key, Total = g.Count(), Converted = g.Count(l => l.Status == LeadStatus.Converted) })
            .ToListAsync();
        var output = rows
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Province)
            .Select(x => new[]
            {
                x.Province,
                x.Total.ToString(),
                x.Converted.ToString(),
                (x.Total == 0 ? 0 : x.Converted * 100.0 / x.Total).ToString("0.#") + "%"
            })
            .ToList();

        return new ReportTable("Lead theo tỉnh/thành", "lead-theo-tinh",
            new[] { "Tỉnh/Thành", "Tổng Lead", "Đã chuyển ứng viên", "Tỷ lệ chuyển đổi" }, output);
    }

    private static async Task<ReportTable> BuildRecruitmentFunnelAsync(ApplicationDbContext db)
    {
        var candidateJobs = await db.CandidateJobOrders
            .Select(c => new { c.CandidateId, c.JobOrderId, c.CurrentStep })
            .ToListAsync();
        var visaApprovedPairs = await db.Visas.Where(v => v.Status == VisaStatus.Approved)
            .Select(v => new { v.CandidateId, v.JobOrderId }).ToListAsync();
        var visaApprovedKeys = visaApprovedPairs.Select(v => (v.CandidateId, v.JobOrderId)).ToHashSet();
        var actualDepartures = await db.Flights.Where(f => f.ActualDepartureAt != null)
            .Select(f => new { f.CandidateId, f.JobOrderId }).ToListAsync();
        var departedKeys = actualDepartures.Select(f => (f.CandidateId, f.JobOrderId)).ToHashSet();

        var total = candidateJobs.Count;
        var selected = candidateJobs.Count(c => c.CurrentStep >= WorkflowStep.Selected);
        var visaApproved = candidateJobs.Count(c =>
            c.CurrentStep >= WorkflowStep.VisaApproved || visaApprovedKeys.Contains((c.CandidateId, c.JobOrderId)));
        var departed = candidateJobs.Count(c =>
            c.CurrentStep >= WorkflowStep.Departure || departedKeys.Contains((c.CandidateId, c.JobOrderId)));

        var rows = new[]
        {
            Row("Đang trong quy trình", total, total),
            Row("Trúng tuyển", selected, total),
            Row("Đậu visa", visaApproved, total),
            Row("Xuất cảnh", departed, total)
        }.ToList();

        return new ReportTable("Phễu tuyển dụng", "pheu-tuyen-dung",
            new[] { "Mốc", "Số ứng viên", "Tỷ lệ trên tổng" }, rows);
    }

    private static async Task<ReportTable> BuildTopAgentsAsync(ApplicationDbContext db)
    {
        var agentNames = await db.Agents.ToDictionaryAsync(a => a.Id, a => a.Name);
        var candidateAgents = await db.Candidates.Where(c => c.AgentId != null)
            .Select(c => new { c.Id, AgentId = c.AgentId!.Value }).ToListAsync();
        var candidateJobs = await db.CandidateJobOrders.Select(c => new { c.CandidateId, c.JobOrderId, c.CurrentStep }).ToListAsync();
        var actualDepartures = await db.Flights.Where(f => f.ActualDepartureAt != null)
            .Select(f => new { f.CandidateId, f.JobOrderId }).ToListAsync();
        var departedKeys = actualDepartures.Select(f => (f.CandidateId, f.JobOrderId)).ToHashSet();
        var departedCandidateIds = candidateJobs
            .Where(c => c.CurrentStep >= WorkflowStep.Departure || departedKeys.Contains((c.CandidateId, c.JobOrderId)))
            .Select(c => c.CandidateId)
            .Distinct()
            .ToHashSet();
        var commissions = await db.AgentCommissions
            .Select(c => new { c.AgentId, c.CommissionAmount, c.Status }).ToListAsync();

        var rows = candidateAgents
            .GroupBy(c => c.AgentId)
            .Select(g =>
            {
                var candidates = g.Select(c => c.Id).Distinct().ToList();
                var departed = candidates.Count(departedCandidateIds.Contains);
                var totalCommission = commissions.Where(c => c.AgentId == g.Key).Sum(c => c.CommissionAmount);
                var paidCommission = commissions.Where(c => c.AgentId == g.Key && c.Status == CommissionStatus.Paid).Sum(c => c.CommissionAmount);
                return new
                {
                    AgentName = agentNames.GetValueOrDefault(g.Key, "—"),
                    CandidateCount = candidates.Count,
                    DepartedCount = departed,
                    DepartureRate = candidates.Count == 0 ? 0 : departed * 100.0 / candidates.Count,
                    TotalCommission = totalCommission,
                    PaidCommission = paidCommission
                };
            })
            .OrderByDescending(x => x.CandidateCount)
            .ThenByDescending(x => x.TotalCommission)
            .Select(x => new[]
            {
                x.AgentName,
                x.CandidateCount.ToString(),
                x.DepartedCount.ToString(),
                x.DepartureRate.ToString("0.#") + "%",
                x.TotalCommission.ToString("N0"),
                x.PaidCommission.ToString("N0")
            })
            .ToList();

        return new ReportTable("Top đại lý", "top-dai-ly",
            new[] { "Đại lý", "Ứng viên giới thiệu", "Đã xuất cảnh", "Tỷ lệ xuất cảnh", "Hoa hồng phát sinh", "Đã chi" }, rows);
    }

    // ---------- Formatters ----------

    private static IResult Csv(ReportTable t)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", t.Header.Select(EscapeCsv)));
        foreach (var r in t.Rows)
            sb.AppendLine(string.Join(",", r.Select(EscapeCsv)));
        // BOM UTF-8 để Excel nhận đúng dấu tiếng Việt.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return Results.File(bytes, "text/csv; charset=utf-8", $"{t.FileBase}-{DateTime.Now:yyyyMMdd}.csv");
    }

    private static IResult Xlsx(ReportTable t)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Báo cáo");
        ws.Cell(1, 1).Value = t.Title;
        ws.Range(1, 1, 1, t.Header.Length).Merge().Style.Font.SetBold().Font.FontSize = 13;

        for (int c = 0; c < t.Header.Length; c++)
        {
            var cell = ws.Cell(3, c + 1);
            cell.Value = t.Header[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a8a");
            cell.Style.Font.FontColor = XLColor.White;
        }
        for (int r = 0; r < t.Rows.Count; r++)
            for (int c = 0; c < t.Rows[r].Length; c++)
                ws.Cell(4 + r, c + 1).Value = t.Rows[r][c];

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Results.File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{t.FileBase}-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private static IResult Pdf(ReportTable t)
    {
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("POLYMIND — Quản lý Xuất khẩu Lao động").Bold().FontSize(13);
                    col.Item().Text(t.Title).FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        for (int i = 0; i < t.Header.Length; i++) c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        foreach (var head in t.Header)
                            h.Cell().Background(Colors.Blue.Darken2).Padding(5).Text(head).FontColor(Colors.White).Bold();
                    });
                    foreach (var row in t.Rows)
                        foreach (var cell in row)
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(cell);
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return Results.File(bytes, "application/pdf", $"{t.FileBase}-{DateTime.Now:yyyyMMdd}.pdf");
    }

    private static byte[] ReceiptPdf(string code, ReceiptType type, string? party, decimal amount,
        DateOnly date, string? description)
    {
        var title = type == ReceiptType.Income ? "PHIẾU THU" : "PHIẾU CHI";
        var partyLabel = type == ReceiptType.Income ? "Người nộp tiền" : "Người nhận tiền";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A5.Landscape());
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("CÔNG TY POLYMIND").Bold().FontSize(12);
                    col.Item().Text("Quản lý Xuất khẩu Lao động").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).AlignCenter().Text(title).Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text($"Số: {code}").FontSize(10);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text(t => { t.Span($"{partyLabel}: ").SemiBold(); t.Span(party ?? "—"); });
                    col.Item().Text(t => { t.Span("Số tiền: ").SemiBold(); t.Span($"{amount:N0} đ").Bold().FontColor(Colors.Blue.Darken2); });
                    col.Item().Text(t => { t.Span("Ngày: ").SemiBold(); t.Span(date.ToString("dd/MM/yyyy")); });
                    if (!string.IsNullOrWhiteSpace(description))
                        col.Item().Text(t => { t.Span("Diễn giải: ").SemiBold(); t.Span(description); });

                    col.Item().PaddingTop(28).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("Người lập phiếu").SemiBold();
                            c.Item().Text("(Ký, ghi rõ họ tên)").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text(partyLabel).SemiBold();
                            c.Item().Text("(Ký, ghi rõ họ tên)").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private static bool Same(DateOnly d, DateOnly m) => d.Year == m.Year && d.Month == m.Month;

    private static string[] Row(string label, int count, int total) =>
        new[] { label, count.ToString(), (total == 0 ? 0 : count * 100.0 / total).ToString("0.#") + "%" };

    private static string EscapeCsv(string? value)
    {
        value ??= "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
