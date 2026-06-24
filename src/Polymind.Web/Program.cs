using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using Polymind.Infrastructure;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Authorization;
using Polymind.Web.Components;
using Polymind.Web.Identity;
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

// Thông báo in-app (stub)
builder.Services.AddScoped<Polymind.Web.Notifications.NotificationService>();

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
