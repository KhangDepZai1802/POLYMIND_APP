using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;

namespace Polymind.Infrastructure.Persistence;

/// <summary>Sinh dữ liệu mẫu để demo (chỉ chạy ở môi trường Development, idempotent).</summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminId = await db.Users.Select(u => u.Id).FirstOrDefaultAsync();
        var rnd = new Random(42);

        if (await db.Leads.AnyAsync()) // core đã seed → chỉ bù dữ liệu mở rộng (idempotent theo bảng)
        {
            await SeedExtrasAsync(db, rnd, adminId);
            return;
        }

        // ---- Đơn hàng tuyển dụng ----
        var jobOrders = new[]
        {
            NewJobOrder("Nhật Bản", "Nghiệp đoàn Kanto", "Toyota Kyushu", "Cơ khí chế tạo", 30, adminId),
            NewJobOrder("Nhật Bản", "Nghiệp đoàn Osaka", "Panasonic", "Điện tử", 20, adminId),
            NewJobOrder("Đài Loan", "Formosa Group", "Formosa Plastics", "Nhựa - Hóa chất", 50, adminId),
            NewJobOrder("Hàn Quốc", "EPS Korea", "Hyundai Steel", "Luyện kim", 15, adminId),
            NewJobOrder("Đức", "ZAV Germany", "Bosch GmbH", "Điều dưỡng", 10, adminId),
        };
        db.JobOrders.AddRange(jobOrders);

        // ---- Đại lý ----
        var agents = new[]
        {
            NewAgent("AG-000001", "Đại lý Miền Bắc", "Nguyễn Văn An"),
            NewAgent("AG-000002", "CTV Nghệ An", "Trần Thị Bình"),
            NewAgent("AG-000003", "Đại lý Hải Phòng", "Lê Hoàng Cường"),
        };
        db.Agents.AddRange(agents);
        await db.SaveChangesAsync();

        // ---- Leads ----
        string[] firstNames = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ" };
        string[] midLast = { "Văn An", "Thị Bình", "Minh Châu", "Quốc Dũng", "Thị Hà", "Hữu Khang", "Thị Lan", "Đức Mạnh", "Thị Nga", "Tuấn Phong" };
        string[] provinces = { "Hà Nội", "Nghệ An", "Thanh Hóa", "Hải Dương", "Bắc Giang", "Nam Định", "Hà Tĩnh", "Quảng Bình" };
        var sources = Enum.GetValues<LeadSource>();
        var statuses = Enum.GetValues<LeadStatus>();

        var leads = new List<Lead>();
        for (int i = 0; i < 40; i++)
        {
            var name = $"{firstNames[rnd.Next(firstNames.Length)]} {midLast[rnd.Next(midLast.Length)]}";
            var status = statuses[rnd.Next(statuses.Length)];
            var createdAt = DateTimeOffset.UtcNow.AddDays(-rnd.Next(0, 45)).AddHours(-rnd.Next(0, 24));
            leads.Add(new Lead
            {
                Code = $"LD-{createdAt:yyyyMMdd}-{1000 + i}",
                FullName = name,
                Phone = $"09{rnd.Next(10000000, 99999999)}",
                Email = $"lead{i}@example.com",
                Province = provinces[rnd.Next(provinces.Length)],
                TargetCountry = jobOrders[rnd.Next(jobOrders.Length)].Country,
                Source = sources[rnd.Next(sources.Length)],
                Status = status,
                AssignedTo = adminId,
                AgentId = rnd.Next(3) == 0 ? agents[rnd.Next(agents.Length)].Id : null,
                Gender = rnd.Next(2) == 0 ? Gender.Male : Gender.Female,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            });
        }
        db.Leads.AddRange(leads);
        await db.SaveChangesAsync();

        // ---- Ứng viên: đảm bảo 12 hồ sơ để demo (đánh dấu 12 lead đầu là Converted) ----
        var convertedLeads = leads.Take(12).ToList();
        foreach (var l in convertedLeads) l.Status = LeadStatus.Converted;
        var steps = Enum.GetValues<WorkflowStep>();
        int c = 0;
        foreach (var lead in convertedLeads)
        {
            var createdAt = lead.CreatedAt.AddDays(2);
            var candidate = new Candidate
            {
                Code = $"UV-{createdAt:yyyyMMdd}-{2000 + c}",
                LeadId = lead.Id,
                FullName = lead.FullName,
                Phone = lead.Phone,
                Province = lead.Province,
                Gender = lead.Gender,
                CccdNumber = $"0{rnd.Next(10000000, 99999999)}{rnd.Next(100, 999)}",
                AgentId = lead.AgentId,
                CreatedBy = adminId,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            };

            var jo = jobOrders[rnd.Next(jobOrders.Length)];
            var reached = steps[rnd.Next(3, steps.Length)]; // đã đi tới một bước ngẫu nhiên
            var cjo = new CandidateJobOrder
            {
                Candidate = candidate,
                JobOrderId = jo.Id,
                CurrentStep = reached,
                Status = CandidateJobOrderStatus.Active,
                AssignedTo = adminId,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            };

            foreach (var step in steps)
            {
                if ((int)step > (int)reached) break;
                cjo.Steps.Add(new WorkflowStepRecord
                {
                    Step = step,
                    Status = step == reached ? WorkflowStepStatus.InProgress : WorkflowStepStatus.Completed,
                    AssignedTo = adminId,
                    StartedAt = createdAt.AddDays((int)step),
                    CompletedAt = step == reached ? null : createdAt.AddDays((int)step + 1),
                    CreatedBy = adminId,
                });
            }
            candidate.JobOrders.Add(cjo);
            db.Candidates.Add(candidate);

            // vài khoản thu mẫu
            if ((int)reached >= (int)WorkflowStep.Deposit)
            {
                db.Payments.Add(new Payment
                {
                    Code = $"PT-{createdAt:yyyyMMdd}-{3000 + c}",
                    CandidateId = candidate.Id,
                    JobOrderId = jo.Id,
                    PaymentType = PaymentType.Deposit,
                    Amount = 20_000_000,
                    Status = PaymentStatus.Paid,
                    PaidDate = DateOnly.FromDateTime(createdAt.AddDays(5).DateTime),
                    CreatedBy = adminId,
                });
            }
            c++;
        }
        await db.SaveChangesAsync();

        await SeedExtrasAsync(db, rnd, adminId);
    }

    /// <summary>Bù dữ liệu demo cho Visa / Vé máy bay / Cấu hình &amp; phát sinh hoa hồng.
    /// Idempotent theo từng bảng nên an toàn chạy lại trên DB đã seed.</summary>
    private static async Task SeedExtrasAsync(ApplicationDbContext db, Random rnd, Guid adminId)
    {
        var cjos = await db.CandidateJobOrders
            .Select(x => new { x.CandidateId, x.JobOrderId, x.CurrentStep })
            .ToListAsync();
        if (cjos.Count == 0) return;

        var jobOrders = await db.JobOrders.ToDictionaryAsync(j => j.Id);
        var candAgent = await db.Candidates.ToDictionaryAsync(c => c.Id, c => c.AgentId);

        // ---- Portal đại lý: gắn tài khoản agent@ vào 1 đại lý (để demo data-scope) ----
        var agentUser = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "AGENT@POLYMIND.LOCAL");
        if (agentUser is not null && !await db.Agents.AnyAsync(a => a.UserId == agentUser.Id))
        {
            var firstAgent = await db.Agents.OrderBy(a => a.Code).FirstOrDefaultAsync(a => a.UserId == null);
            if (firstAgent is not null)
            {
                firstAgent.UserId = agentUser.Id;
                firstAgent.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        // ---- Visa: ứng viên đã tới bước Nộp hồ sơ Visa trở đi ----
        if (!await db.Visas.AnyAsync())
        {
            foreach (var x in cjos.Where(x => (int)x.CurrentStep >= (int)WorkflowStep.VisaSubmit))
            {
                var jo = jobOrders[x.JobOrderId];
                var status = x.CurrentStep switch
                {
                    WorkflowStep.VisaSubmit => VisaStatus.Submitted,
                    >= WorkflowStep.VisaApproved => VisaStatus.Approved,
                    _ => VisaStatus.Submitted
                };
                var submitted = DateTimeOffset.UtcNow.AddDays(-rnd.Next(20, 60));
                db.Visas.Add(new Visa
                {
                    CandidateId = x.CandidateId,
                    JobOrderId = x.JobOrderId,
                    Country = jo.Country,
                    VisaType = jo.Country == "Nhật Bản" ? "Tokutei Ginou" : "Lao động",
                    Status = status,
                    SubmittedDate = DateOnly.FromDateTime(submitted.UtcDateTime),
                    InterviewDate = DateOnly.FromDateTime(submitted.AddDays(10).UtcDateTime),
                    ResultDate = status == VisaStatus.Approved ? DateOnly.FromDateTime(submitted.AddDays(20).UtcDateTime) : null,
                    HandledBy = adminId,
                });
            }
        }

        // ---- Vé máy bay: ứng viên đã tới bước Đặt vé máy bay trở đi ----
        if (!await db.Flights.AnyAsync())
        {
            string[] airlines = { "Vietnam Airlines", "Vietjet Air", "Japan Airlines", "Korean Air", "China Airlines" };
            foreach (var x in cjos.Where(x => (int)x.CurrentStep >= (int)WorkflowStep.BookFlight))
            {
                var jo = jobOrders[x.JobOrderId];
                var dep = DateTimeOffset.UtcNow.AddDays(rnd.Next(5, 40));
                db.Flights.Add(new Flight
                {
                    CandidateId = x.CandidateId,
                    JobOrderId = x.JobOrderId,
                    Airline = airlines[rnd.Next(airlines.Length)],
                    TicketCode = $"VN{rnd.Next(100, 999)}-{rnd.Next(1000, 9999)}",
                    DepartureDate = DateOnly.FromDateTime(dep.UtcDateTime),
                    DepartureTime = new TimeOnly(rnd.Next(6, 22), rnd.Next(0, 60)),
                    DepartureAirport = "Nội Bài (HAN)",
                    DestinationCountry = jo.Country,
                    DestinationAirport = AirportOf(jo.Country),
                    ActualDepartureAt = x.CurrentStep >= WorkflowStep.Departure ? dep : null,
                    AssignedTo = adminId,
                });
            }
        }

        // ---- Cấu hình hoa hồng cho mỗi đại lý: 20% đặt cọc / 30% trúng tuyển / 50% xuất cảnh ----
        if (!await db.AgentCommissionConfigs.AnyAsync())
        {
            var agentIds = await db.Agents.Select(a => a.Id).ToListAsync();
            foreach (var aid in agentIds)
            {
                db.AgentCommissionConfigs.Add(new AgentCommissionConfig { AgentId = aid, Milestone = CommissionMilestone.Deposit, Percentage = 20 });
                db.AgentCommissionConfigs.Add(new AgentCommissionConfig { AgentId = aid, Milestone = CommissionMilestone.Selected, Percentage = 30 });
                db.AgentCommissionConfigs.Add(new AgentCommissionConfig { AgentId = aid, Milestone = CommissionMilestone.Departure, Percentage = 50 });
            }
        }

        // ---- Hoa hồng phát sinh theo mốc cho ứng viên có đại lý ----
        if (!await db.AgentCommissions.AnyAsync())
        {
            var milestones = new (CommissionMilestone Milestone, WorkflowStep Step, decimal Percent)[]
            {
                (CommissionMilestone.Deposit, WorkflowStep.Deposit, 20m),
                (CommissionMilestone.Selected, WorkflowStep.Selected, 30m),
                (CommissionMilestone.Departure, WorkflowStep.Departure, 50m),
            };
            foreach (var x in cjos)
            {
                var agentId = candAgent.GetValueOrDefault(x.CandidateId);
                if (agentId is null) continue;
                var jo = jobOrders[x.JobOrderId];
                var baseAmount = jo.CostAmount ?? 120_000_000m;
                foreach (var m in milestones)
                {
                    if ((int)x.CurrentStep < (int)m.Step) continue;
                    db.AgentCommissions.Add(new AgentCommission
                    {
                        AgentId = agentId.Value,
                        CandidateId = x.CandidateId,
                        JobOrderId = x.JobOrderId,
                        Milestone = m.Milestone,
                        BaseAmount = baseAmount,
                        CommissionAmount = baseAmount * m.Percent / 100m,
                        Status = x.CurrentStep >= WorkflowStep.Departure ? CommissionStatus.Approved : CommissionStatus.Pending,
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static string AirportOf(string country) => country switch
    {
        "Nhật Bản" => "Narita (NRT)",
        "Hàn Quốc" => "Incheon (ICN)",
        "Đài Loan" => "Đào Viên (TPE)",
        "Đức" => "Frankfurt (FRA)",
        _ => "—"
    };

    private static JobOrder NewJobOrder(string country, string union, string company, string field, int qty, Guid createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        return new JobOrder
        {
            Code = $"JO-{now:yyyyMM}-{Random.Shared.Next(100, 999)}",
            Country = country,
            UnionName = union,
            CompanyName = company,
            Field = field,
            Quantity = qty,
            SalaryDescription = "30-40 triệu/tháng",
            CostAmount = 120_000_000,
            Status = JobOrderStatus.Recruiting,
            RecruitmentStartDate = DateOnly.FromDateTime(now.DateTime),
            ExpectedDepartureDate = DateOnly.FromDateTime(now.AddMonths(4).DateTime),
            CreatedBy = createdBy,
        };
    }

    private static Agent NewAgent(string code, string name, string rep) => new()
    {
        Code = code,
        Name = name,
        RepresentativeName = rep,
        Phone = $"09{Random.Shared.Next(10000000, 99999999)}",
        Email = $"{code.ToLower()}@agent.local",
        IsActive = true,
    };
}
