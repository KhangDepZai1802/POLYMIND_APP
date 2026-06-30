using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Polymind.Domain.Entities;
using Polymind.Infrastructure.Identity;

namespace Polymind.Infrastructure.Persistence;

/// <summary>
/// DbContext chính: gộp ASP.NET Core Identity (users/roles) với toàn bộ entity nghiệp vụ.
/// Bảng dùng snake_case (cấu hình UseSnakeCaseNamingConvention ở tầng DI).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<JobOrder> JobOrders => Set<JobOrder>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<CandidateJobOrder> CandidateJobOrders => Set<CandidateJobOrder>();
    public DbSet<WorkflowStepRecord> WorkflowStepRecords => Set<WorkflowStepRecord>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AgentCommissionConfig> AgentCommissionConfigs => Set<AgentCommissionConfig>();
    public DbSet<AgentCommission> AgentCommissions => Set<AgentCommission>();
    public DbSet<Visa> Visas => Set<Visa>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Bảng Identity đặt tên gọn (snake_case sẽ tự áp dụng).
        b.Entity<ApplicationUser>().ToTable("users");
        b.Entity<ApplicationRole>().ToTable("roles");

        b.Entity<Permission>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<RolePermission>(e =>
        {
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.HasOne(x => x.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Lead>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Phone);
            e.HasIndex(x => x.Cccd);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.AssignedTo);
            e.HasIndex(x => x.Source);
            e.HasIndex(x => x.CollaboratorId);
            e.HasIndex(x => x.CreatedAt);
        });

        b.Entity<JobOrder>().HasIndex(x => x.Code).IsUnique();

        b.Entity<Candidate>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Phone);
            e.HasIndex(x => x.CccdNumber);
            e.HasIndex(x => x.PassportNumber);
            e.HasIndex(x => x.ConsultantId);
        });

        b.Entity<Payment>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.CandidateId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Amount).HasPrecision(15, 2);
        });

        b.Entity<Loan>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.CandidateId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Amount).HasPrecision(15, 2);
            e.Property(x => x.InterestRate).HasPrecision(5, 2);
        });

        b.Entity<Receipt>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.PaymentId);
            e.HasIndex(x => x.ExpenseId);
            e.Property(x => x.Amount).HasPrecision(15, 2);
        });

        b.Entity<Expense>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Amount).HasPrecision(15, 2);
        });

        b.Entity<Agent>().HasIndex(x => x.Code).IsUnique();

        b.Entity<Collaborator>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.AgentId);
            e.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.CommissionSharePercentage).HasPrecision(5, 2).HasDefaultValue(50m);
        });

        b.Entity<Message>(e =>
        {
            e.HasIndex(x => new { x.RecipientId, x.IsRead });
            e.HasIndex(x => new { x.SenderId, x.RecipientId, x.CreatedAt });
        });

        b.Entity<AgentCommissionConfig>(e =>
        {
            e.Property(x => x.Percentage).HasPrecision(5, 2);
            e.Property(x => x.FixedAmount).HasPrecision(15, 2);
        });

        b.Entity<AgentCommission>(e =>
        {
            e.HasIndex(x => x.AgentId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.BaseAmount).HasPrecision(15, 2);
            e.Property(x => x.CommissionAmount).HasPrecision(15, 2);
        });

        b.Entity<JobOrder>().Property(x => x.CostAmount).HasPrecision(15, 2);

        b.Entity<AuditLog>(e =>
        {
            e.HasIndex(x => new { x.Resource, x.ResourceId });
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.Property(x => x.OldValue).HasColumnType("jsonb");
            e.Property(x => x.NewValue).HasColumnType("jsonb");
        });

        b.Entity<WorkflowStepRecord>().HasIndex(x => x.CandidateJobOrderId);

        b.Entity<Notification>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.Type, x.ReferenceId, x.Channel }).IsUnique();
            e.HasIndex(x => new { x.SentAt, x.Channel });
        });

        b.Entity<NotificationPreference>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.Type }).IsUnique();
        });

        // Lưu mọi enum dưới dạng chuỗi (dễ đọc, ổn định khi bổ sung giá trị mới).
        foreach (var entityType in b.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var underlying = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (!underlying.IsEnum) continue;

                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(underlying);
                var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                property.SetValueConverter(converter);
            }
        }
    }
}
