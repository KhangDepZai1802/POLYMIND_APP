using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Infrastructure.Persistence;

/// <summary>Sinh dữ liệu mẫu để demo (chỉ chạy ở môi trường Development, idempotent).</summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DemoDataSeeder");

        var adminId = await db.Users.Select(u => u.Id).FirstOrDefaultAsync();
        var rnd = new Random(42);

        if (await db.Leads.AnyAsync()) // core đã seed → chỉ bù dữ liệu mở rộng (idempotent theo bảng)
        {
            await SeedExtrasAsync(db, userManager, logger, rnd, adminId);
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
        // Bước phụ 7.5 (ReselectJobOrder) chỉ vào được qua luồng rớt B8 — không seed ngẫu nhiên.
        var steps = Enum.GetValues<WorkflowStep>()
            .Where(s => s != WorkflowStep.ReselectJobOrder)
            .ToArray();
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

        await SeedExtrasAsync(db, userManager, logger, rnd, adminId);
    }

    /// <summary>Bù dữ liệu demo cho Visa / Vé máy bay / Cấu hình &amp; phát sinh hoa hồng.
    /// Idempotent theo từng bảng nên an toàn chạy lại trên DB đã seed.</summary>
    private static async Task SeedExtrasAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        Random rnd,
        Guid adminId)
    {
        var cjos = await db.CandidateJobOrders
            .Select(x => new { x.CandidateId, x.JobOrderId, x.CurrentStep })
            .ToListAsync();
        if (cjos.Count == 0)
        {
            await EnsurePartnerAccountsAsync(db, userManager, logger);
            return;
        }

        // ---- Bổ sung đại lý để bảng thi đua phong phú (idempotent theo Code) ----
        var extraAgents = new[]
        {
            NewAgent("AG-000004", "Đại lý Miền Trung", "Phan Thị Hồng"),
            NewAgent("AG-000005", "Đại lý Tây Nguyên", "Ngô Văn Tài"),
        };
        foreach (var a in extraAgents)
        {
            if (!await db.Agents.AnyAsync(x => x.Code == a.Code))
                db.Agents.Add(a);
        }
        await db.SaveChangesAsync();

        // ---- Cộng tác viên (CTV): mỗi đại lý có vài CTV ----
        if (!await db.Collaborators.AnyAsync())
        {
            var allAgents = await db.Agents.OrderBy(a => a.Code).ToListAsync();
            string[] ctvFirst = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Dương", "Phan" };
            string[] ctvMidLast = { "Thị Mai", "Văn Toàn", "Thị Thu", "Quốc Bảo", "Thị Hương", "Hữu Lộc", "Thị Yến", "Đức Anh", "Thị Linh", "Tuấn Kiệt", "Thị Diệu", "Minh Quân" };
            int n = 1;
            foreach (var agent in allAgents)
            {
                int count = rnd.Next(2, 5); // 2-4 CTV/đại lý
                for (int i = 0; i < count; i++)
                {
                    db.Collaborators.Add(new Collaborator
                    {
                        Code = $"CTV-{n:0000}",
                        FullName = $"{ctvFirst[rnd.Next(ctvFirst.Length)]} {ctvMidLast[rnd.Next(ctvMidLast.Length)]}",
                        Phone = $"09{rnd.Next(10000000, 99999999)}",
                        Email = $"ctv{n}@polymind.local",
                        Address = "Việt Nam",
                        CommissionSharePercentage = new[] { 45m, 50m, 55m, 60m }[rnd.Next(4)],
                        AgentId = agent.Id,
                        IsActive = rnd.Next(6) != 0, // ~83% đang hoạt động
                    });
                    n++;
                }
            }
            await db.SaveChangesAsync();
        }

        // ---- Gán mỗi ứng viên vào 1 đại lý + 1 CTV của đại lý đó (ai chưa có thì backfill) ----
        await EnsurePartnerAccountsAsync(db, userManager, logger);

        var agentsForFill = await db.Agents.OrderBy(a => a.Code).ToListAsync();
        var ctvByAgent = (await db.Collaborators.ToListAsync())
            .GroupBy(c => c.AgentId)
            .ToDictionary(g => g.Key, g => g.ToList());
        if (agentsForFill.Count > 0)
        {
            var candidates = await db.Candidates.ToListAsync();
            var fillChanged = false;
            foreach (var cand in candidates)
            {
                if (cand.CollaboratorId is not null) continue; // đã gắn CTV
                var agent = cand.AgentId is Guid aid && agentsForFill.Any(a => a.Id == aid)
                    ? agentsForFill.First(a => a.Id == aid)
                    : agentsForFill[rnd.Next(agentsForFill.Count)];
                cand.AgentId = agent.Id;
                if (ctvByAgent.TryGetValue(agent.Id, out var ctvs) && ctvs.Count > 0)
                    cand.CollaboratorId = ctvs[rnd.Next(ctvs.Count)].Id;
                cand.UpdatedAt = DateTimeOffset.UtcNow;
                fillChanged = true;
            }
            if (fillChanged) await db.SaveChangesAsync();
        }

        // ---- Tư vấn viên (role consultant): gắn mỗi ứng viên với 1 TVV (1 TVV : nhiều ứng viên) ----
        var consultantRoleId = await db.Roles.Where(r => r.Name == RoleNames.Consultant).Select(r => r.Id).FirstOrDefaultAsync();
        var consultantIds = consultantRoleId == default
            ? new List<Guid>()
            : await db.UserRoles.Where(ur => ur.RoleId == consultantRoleId).Select(ur => ur.UserId).ToListAsync();
        if (consultantIds.Count > 0)
        {
            var needConsultant = await db.Candidates.Where(c => c.ConsultantId == null).ToListAsync();
            foreach (var cand in needConsultant)
            {
                cand.ConsultantId = consultantIds[rnd.Next(consultantIds.Count)];
                cand.UpdatedAt = DateTimeOffset.UtcNow;
            }
            if (needConsultant.Count > 0) await db.SaveChangesAsync();
        }

        // ---- Lead có nguồn "Giới thiệu" từ CTV (hiển thị "CTV-<tên>" ở cột Nguồn) ----
        // Idempotent: chỉ seed nếu chưa có lead nào gắn CollaboratorId.
        if (!await db.Leads.AnyAsync(l => l.CollaboratorId != null))
        {
            var ctvList = await db.Collaborators.Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
            if (ctvList.Count > 0 && consultantIds.Count > 0)
            {
                string[] rFirst = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương" };
                string[] rMidLast = { "Văn Thành", "Thị Hoa", "Minh Tuấn", "Quốc Việt", "Thị Trang", "Hữu Phước", "Thị Kiều", "Đức Huy", "Thị Ngân", "Tuấn Anh", "Thị Hằng", "Bá Lộc" };
                string[] rProvinces = { "Nghệ An", "Hà Tĩnh", "Thanh Hóa", "Quảng Bình", "Nam Định", "Bắc Giang", "Hải Dương", "Phú Thọ" };
                string[] rCountries = { "Nhật Bản", "Đài Loan", "Hàn Quốc", "Đức" };
                var leadStatuses = new[] { LeadStatus.New, LeadStatus.Contacted, LeadStatus.Interested, LeadStatus.Appointment, LeadStatus.Consulting };

                var referralLeads = new List<Lead>();
                for (int i = 0; i < 12; i++)
                {
                    var ctv = ctvList[rnd.Next(ctvList.Count)];
                    var createdAt = DateTimeOffset.UtcNow.AddDays(-rnd.Next(0, 20)).AddHours(-rnd.Next(0, 24));
                    referralLeads.Add(new Lead
                    {
                        Code = $"LD-{createdAt:yyyyMMdd}-{7000 + i}",
                        FullName = $"{rFirst[rnd.Next(rFirst.Length)]} {rMidLast[rnd.Next(rMidLast.Length)]}",
                        Phone = $"09{rnd.Next(10000000, 99999999)}",
                        Province = rProvinces[rnd.Next(rProvinces.Length)],
                        TargetCountry = rCountries[rnd.Next(rCountries.Length)],
                        Source = LeadSource.Referral,
                        CollaboratorId = ctv.Id,   // CTV giới thiệu → cột Nguồn hiện "CTV-<tên>"
                        AgentId = ctv.AgentId,
                        AssignedTo = consultantIds[rnd.Next(consultantIds.Count)], // tư vấn viên phụ trách
                        Status = leadStatuses[rnd.Next(leadStatuses.Length)],
                        Gender = rnd.Next(2) == 0 ? Gender.Male : Gender.Female,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt,
                    });
                }
                db.Leads.AddRange(referralLeads);
                foreach (var l in referralLeads)
                {
                    db.LeadActivities.Add(new LeadActivity
                    {
                        LeadId = l.Id,
                        ActivityType = LeadActivityType.Note,
                        Content = "Tạo lead mới (nguồn: CTV giới thiệu)",
                        NewStatus = l.Status,
                        CreatedAt = l.CreatedAt,
                        UpdatedAt = l.CreatedAt,
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        // ---- Backfill đãi ngộ/thưởng cho các đơn hàng cũ chưa có ----
        var jobsNeedingPerks = await db.JobOrders
            .Where(j => j.Benefits == null && j.Bonus == null)
            .ToListAsync();
        if (jobsNeedingPerks.Count > 0)
        {
            foreach (var j in jobsNeedingPerks)
            {
                j.Benefits = "• Bao ăn ở tại ký túc xá\n• Bảo hiểm đầy đủ theo luật nước sở tại\n• Tăng ca thêm thu nhập\n• 1-2 ngày nghỉ/tuần, nghỉ lễ theo công ty";
                j.Bonus = "Thưởng ký hợp đồng, thưởng chuyên cần hàng tháng, thưởng cuối năm (lương tháng 13).";
                j.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync();
        }

        // ---- Backfill ngày đăng/hạn ứng tuyển cho job cũ + job demo trong nước/du học (góp ý Vietgroup) ----
        var jobsNeedingDates = await db.JobOrders.Where(j => j.PostedDate == null).ToListAsync();
        if (jobsNeedingDates.Count > 0)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var i = 0;
            foreach (var j in jobsNeedingDates)
            {
                j.PostedDate = j.RecruitmentStartDate ?? DateOnly.FromDateTime(j.CreatedAt.UtcDateTime);
                // Xen kẽ vài job sắp hết hạn (≤7 ngày) để demo hiển thị ngày đỏ.
                j.ApplicationDeadline ??= today.AddDays(i++ % 3 == 0 ? rnd.Next(1, 7) : rnd.Next(14, 60));
                j.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync();
        }

        if (!await db.JobOrders.AnyAsync(j => j.Category != JobCategory.OverseasJob))
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var domestic = NewJobOrder("Việt Nam", "", "VinFast Hải Phòng", "Lắp ráp ô tô", 40, adminId);
            domestic.UnionName = null;
            domestic.Category = JobCategory.DomesticJob;
            domestic.SalaryDescription = "12-18 triệu/tháng";
            domestic.CostAmount = null;
            domestic.PostedDate = today.AddDays(-5);
            domestic.ApplicationDeadline = today.AddDays(25);

            var study = NewJobOrder("Nhật Bản", "", "Học viện Nhật ngữ Tokyo", "Du học sinh tiếng Nhật", 25, adminId);
            study.UnionName = null;
            study.Category = JobCategory.StudyAbroad;
            study.SalaryDescription = "Làm thêm 28h/tuần";
            study.CostAmount = 180_000_000;
            study.PostedDate = today.AddDays(-10);
            study.ApplicationDeadline = today.AddDays(5); // demo ngày đỏ sắp hết hạn

            db.JobOrders.AddRange(domestic, study);
            await db.SaveChangesAsync();
        }

        var jobOrders = await db.JobOrders.ToDictionaryAsync(j => j.Id);
        var candAgent = await db.Candidates.ToDictionaryAsync(c => c.Id, c => c.AgentId);

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

        // ---- Lịch đóng tiền 4 bước cho ứng viên đã xếp vào đơn hàng (20/30/30/20 chi phí đơn hàng) ----
        if (!await db.Payments.AnyAsync(p => p.Stage != null))
        {
            var stagePlan = new (PaymentStage Stage, double Ratio, PaymentType Type, WorkflowStep PaidFrom)[]
            {
                (PaymentStage.Deposit,      0.20, PaymentType.Deposit,     WorkflowStep.Deposit),     // đặt cọc
                (PaymentStage.ServiceFee,   0.30, PaymentType.ServiceFee,  WorkflowStep.Selected),    // phí dịch vụ
                (PaymentStage.PreDeparture, 0.30, PaymentType.TrainingFee, WorkflowStep.FullPayment), // phí trước xuất cảnh
                (PaymentStage.Settlement,   0.20, PaymentType.OtherIncome, WorkflowStep.Departure),   // tất toán
            };
            foreach (var x in cjos)
            {
                var jo = jobOrders[x.JobOrderId];
                var total = jo.CostAmount ?? 0m;
                if (total <= 0) continue;
                decimal running = 0;
                for (int i = 0; i < stagePlan.Length; i++)
                {
                    var sp = stagePlan[i];
                    var amount = i == stagePlan.Length - 1
                        ? total - running
                        : Math.Round(total * (decimal)sp.Ratio, 0, MidpointRounding.AwayFromZero);
                    if (i < stagePlan.Length - 1) running += amount;
                    var paid = (int)x.CurrentStep >= (int)sp.PaidFrom;
                    db.Payments.Add(new Payment
                    {
                        Code = $"PT-{DateTime.UtcNow:yyyyMMdd}-{rnd.Next(1000, 9999)}",
                        CandidateId = x.CandidateId,
                        JobOrderId = x.JobOrderId,
                        PaymentType = sp.Type,
                        Stage = sp.Stage,
                        Amount = amount,
                        Status = paid ? PaymentStatus.Paid : PaymentStatus.Pending,
                        PaidDate = paid ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
                        Notes = $"Bước {(int)sp.Stage} — lịch đóng tiền theo chi phí đơn hàng",
                        CreatedBy = adminId,
                        ApprovedBy = paid ? adminId : null,
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // ---- Hồ sơ hỗ trợ vay demo (xen kẽ Đang vay / Đã giải ngân; ~1/3 ứng viên chưa vay) ----
        if (!await db.Loans.AnyAsync())
        {
            var banks = new[] { "Vietcombank", "BIDV", "VietinBank", "Agribank", "Sacombank", "MB Bank" };
            var loanCands = cjos.Select(x => x.CandidateId).Distinct().ToList();
            var idx = 0;
            foreach (var cid in loanCands)
            {
                idx++;
                if (idx % 3 == 0) continue; // ~1/3 ứng viên chưa vay (không tạo hồ sơ)
                var disbursed = idx % 2 == 0;
                db.Loans.Add(new Loan
                {
                    Code = $"VAY-{DateTime.UtcNow:yyyyMMdd}-{2000 + idx}",
                    CandidateId = cid,
                    Status = disbursed ? LoanStatus.Disbursed : LoanStatus.Borrowing,
                    Amount = (decimal)rnd.Next(80, 220) * 1_000_000m,
                    TermMonths = new[] { 12, 24, 36, 48 }[rnd.Next(4)],
                    BankName = banks[rnd.Next(banks.Length)],
                    InterestRate = (decimal)(rnd.Next(60, 120) / 10.0),
                    DisbursedDate = disbursed ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-rnd.Next(10, 120))) : null,
                    Note = disbursed ? "Đã giải ngân, đang trả góp" : "Đang hoàn thiện hồ sơ vay",
                    CreatedBy = adminId,
                });
            }
            await db.SaveChangesAsync();
        }

        // ---- Vài tin nhắn nội bộ demo (để hộp thư không trống) ----
        if (!await db.Messages.AnyAsync())
        {
            var admin = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "ADMIN@POLYMIND.LOCAL");
            var director = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "DIRECTOR@POLYMIND.LOCAL");
            var recruiter = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "RECRUITER@POLYMIND.LOCAL");
            if (admin is not null && director is not null)
            {
                db.Messages.Add(new Message { SenderId = admin.Id, RecipientId = director.Id, Body = "Chào sếp, em gửi báo cáo tuyển dụng tuần này nhé.", IsRead = true, ReadAt = DateTimeOffset.UtcNow.AddHours(-20), CreatedAt = DateTimeOffset.UtcNow.AddHours(-22), UpdatedAt = DateTimeOffset.UtcNow.AddHours(-22) });
                db.Messages.Add(new Message { SenderId = director.Id, RecipientId = admin.Id, Body = "Ok em, số liệu ổn. Đẩy mạnh đơn Đức nhé.", IsRead = false, CreatedAt = DateTimeOffset.UtcNow.AddHours(-19), UpdatedAt = DateTimeOffset.UtcNow.AddHours(-19) });
            }
            if (recruiter is not null && admin is not null)
            {
                db.Messages.Add(new Message { SenderId = recruiter.Id, RecipientId = admin.Id, Body = "Anh ơi, ứng viên đơn Nhật cần duyệt hồ sơ gấp ạ.", IsRead = false, CreatedAt = DateTimeOffset.UtcNow.AddHours(-3), UpdatedAt = DateTimeOffset.UtcNow.AddHours(-3) });
            }
            await db.SaveChangesAsync();
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsurePartnerAccountsAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var now = DateTimeOffset.UtcNow;
        var changed = false;
        var agents = await db.Agents.OrderBy(a => a.Code).ToListAsync();
        var collaborators = await db.Collaborators.OrderBy(c => c.Code).ToListAsync();
        var genericAgent = await userManager.FindByEmailAsync("agent@polymind.local");

        for (var i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            var user = agent.UserId is Guid userId
                ? await db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                : null;

            if (user is null)
            {
                var canUseGenericAgent = i == 0
                    && genericAgent is not null
                    && !await db.Agents.AnyAsync(a => a.UserId == genericAgent.Id && a.Id != agent.Id);
                user = canUseGenericAgent
                    ? genericAgent!
                    : await EnsurePartnerUserAsync(
                        userManager,
                        logger,
                        PartnerEmail("agent", agent.Code),
                        string.IsNullOrWhiteSpace(agent.RepresentativeName) ? agent.Name : agent.RepresentativeName!,
                        agent.Phone,
                        RoleNames.Agent);

                agent.UserId = user.Id;
                agent.UpdatedAt = now;
                changed = true;
            }

            await EnsurePartnerUserReadyAsync(userManager, logger, user, RoleNames.Agent, agent.Name, agent.Phone);
        }

        foreach (var collaborator in collaborators)
        {
            if (collaborator.CommissionSharePercentage <= 0)
            {
                collaborator.CommissionSharePercentage = 50m;
                collaborator.UpdatedAt = now;
                changed = true;
            }

            var user = collaborator.UserId is Guid userId
                ? await db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                : null;

            if (user is null)
            {
                user = await EnsurePartnerUserAsync(
                    userManager,
                    logger,
                    PartnerEmail("ctv", collaborator.Code),
                    collaborator.FullName,
                    collaborator.Phone,
                    RoleNames.Collaborator);

                collaborator.UserId = user.Id;
                collaborator.UpdatedAt = now;
                changed = true;
            }

            await EnsurePartnerUserReadyAsync(userManager, logger, user, RoleNames.Collaborator, collaborator.FullName, collaborator.Phone);
        }

        if (changed)
            await db.SaveChangesAsync();

        await RemoveGeneratedPartnerRoleExtrasAsync(
            db,
            userManager,
            RoleNames.Agent,
            agents.Where(a => a.UserId is not null).Select(a => a.UserId!.Value).ToHashSet(),
            email => email.Equals("agent@polymind.local", StringComparison.OrdinalIgnoreCase)
                || email.StartsWith("agent-", StringComparison.OrdinalIgnoreCase));
        await RemoveGeneratedPartnerRoleExtrasAsync(
            db,
            userManager,
            RoleNames.Collaborator,
            collaborators.Where(c => c.UserId is not null).Select(c => c.UserId!.Value).ToHashSet(),
            email => email.StartsWith("ctv-", StringComparison.OrdinalIgnoreCase));

        logger.LogInformation(
            "Da dong bo tai khoan doi tac: {AgentCount} tai khoan dai ly, {CollaboratorCount} tai khoan CTV.",
            agents.Count,
            collaborators.Count);
    }

    private static async Task<ApplicationUser> EnsurePartnerUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string fullName,
        string? phone,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                PhoneNumber = phone,
                IsActive = true,
            };

            var create = await userManager.CreateAsync(user, DbSeeder.DefaultAdminPassword);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors.Select(e => e.Description));
                logger.LogError("Tao tai khoan doi tac {Email} that bai: {Errors}", email, errors);
                throw new InvalidOperationException($"Tao tai khoan doi tac {email} that bai: {errors}");
            }
        }

        await EnsurePartnerUserReadyAsync(userManager, logger, user, role, fullName, phone);
        return user;
    }

    private static async Task EnsurePartnerUserReadyAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        ApplicationUser user,
        string role,
        string fullName,
        string? phone)
    {
        var changed = false;
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            changed = true;
        }
        if (!user.IsActive)
        {
            user.IsActive = true;
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            user.FullName = fullName;
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(user.PhoneNumber) && !string.IsNullOrWhiteSpace(phone))
        {
            user.PhoneNumber = phone;
            changed = true;
        }

        if (changed)
        {
            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                logger.LogWarning(
                    "Cap nhat tai khoan doi tac {Email} chua thanh cong: {Errors}",
                    user.Email,
                    string.Join("; ", update.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }

    private static async Task RemoveGeneratedPartnerRoleExtrasAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        string role,
        HashSet<Guid> linkedUserIds,
        Func<string, bool> isGeneratedEmail)
    {
        var staffRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
        var generatedUsers = await db.Users
            .Where(u => u.Email != null)
            .ToListAsync();

        foreach (var user in generatedUsers.Where(u => !linkedUserIds.Contains(u.Id) && isGeneratedEmail(u.Email!)))
        {
            if (await userManager.IsInRoleAsync(user, role))
                await userManager.RemoveFromRoleAsync(user, role);
        }

        var unlinkedUsers = generatedUsers.Where(u => !linkedUserIds.Contains(u.Id)).ToList();
        foreach (var user in unlinkedUsers)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(role) || roles.Any(staffRoles.Contains)) continue;
            await userManager.RemoveFromRoleAsync(user, role);
        }
    }

    private static string PartnerEmail(string prefix, string code)
        => $"{prefix}-{NormalizeCode(code)}@polymind.local";

    private static string NormalizeCode(string code)
    {
        var chars = code
            .Trim()
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray();
        return chars.Length == 0 ? Guid.NewGuid().ToString("N")[..8] : new string(chars);
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
            Benefits = "• Bao ăn ở tại ký túc xá\n• Bảo hiểm đầy đủ theo luật nước sở tại\n• Tăng ca thêm thu nhập\n• 1-2 ngày nghỉ/tuần, nghỉ lễ theo công ty",
            Bonus = "Thưởng ký hợp đồng, thưởng chuyên cần hàng tháng, thưởng cuối năm (lương tháng 13).",
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
