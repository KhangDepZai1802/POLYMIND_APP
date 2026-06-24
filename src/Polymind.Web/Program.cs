using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Hangfire.PostgreSql;
using MudBlazor.Services;
using Polymind.Infrastructure;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Authorization;
using Polymind.Web.Components;
using Polymind.Web.Identity;
using Polymind.Web.Notifications;
using Polymind.Web.Reporting;
using Polymind.Web.Storage;

// QuestPDF: dùng giấy phép Community (miễn phí) cho xuất PDF.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Blazor (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor UI
builder.Services.AddMudServices();

// MinIO/S3 document storage
builder.Services.Configure<MinioStorageOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.AddScoped<IDocumentStorage, MinioDocumentStorage>();

// Thông báo đa kênh + job nền
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationJob>();
builder.Services.AddScoped<INotificationSender, InAppNotificationSender>();
builder.Services.AddScoped<INotificationSender, SmtpEmailNotificationSender>();
builder.Services.AddScoped<INotificationSender, LoggingSmsNotificationSender>();
builder.Services.AddScoped<INotificationSender, LoggingZaloNotificationSender>();

var dbConnectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(dbConnectionString)));
builder.Services.AddHangfireServer();

// Phạm vi dữ liệu Portal đại lý
builder.Services.AddScoped<Polymind.Web.Identity.AgentScope>();

// EF Core (PostgreSQL) + ASP.NET Core Identity
builder.Services.AddInfrastructure(builder.Configuration);

// Auth cho Blazor
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, PermissionClaimsPrincipalFactory>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Đăng xuất: xóa cookie rồi quay về trang login.
app.MapPost("/Account/Logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

// Xuất báo cáo CSV (gated reports:read).
app.MapCsvExportEndpoints();

RecurringJob.AddOrUpdate<NotificationJob>(
    "polymind-notification-reminders",
    job => job.RunAsync(),
    "*/5 * * * *");

// Áp migration + seed roles/permissions/super_admin + dữ liệu mẫu (bỏ qua nếu DB chưa sẵn sàng).
using (var scope = app.Services.CreateScope())
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        await DbSeeder.SeedAsync(app.Services);
        if (app.Environment.IsDevelopment())
            await DemoDataSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Bỏ qua seed DB — kiểm tra PostgreSQL đã chạy (docker compose up) chưa.");
    }
}

app.Run();
