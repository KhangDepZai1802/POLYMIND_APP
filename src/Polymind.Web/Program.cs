using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Hangfire;
using Hangfire.PostgreSql;
using MudBlazor.Services;
using Polymind.Infrastructure;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Api;
using Polymind.Web.Authorization;
using Polymind.Web.Components;
using Polymind.Web.Health;
using Polymind.Web.Identity;
using Polymind.Web.Notifications;
using Polymind.Web.Reporting;
using Polymind.Web.Storage;
using Serilog;

// QuestPDF: dùng giấy phép Community (miễn phí) cho xuất PDF.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/polymind-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14));

// Blazor (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor UI
builder.Services.AddMudServices();

// MinIO/S3 document storage
builder.Services.Configure<MinioStorageOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.AddScoped<IDocumentStorage, MinioDocumentStorage>();

// AI (Gemini) — trợ lý hỏi-đáp, phân tích hồ sơ, trích xuất CV. Key tạm thời (free) ở Ai:Gemini.
builder.Services.Configure<Polymind.Web.Ai.GeminiOptions>(builder.Configuration.GetSection("Ai:Gemini"));
builder.Services.AddHttpClient<Polymind.Web.Ai.GeminiClient>(c => c.Timeout = TimeSpan.FromSeconds(60));

// Thông báo đa kênh + job nền
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationJob>();
builder.Services.AddScoped<INotificationSender, InAppNotificationSender>();
builder.Services.AddScoped<INotificationSender, SmtpEmailNotificationSender>();
builder.Services.AddScoped<INotificationSender, LoggingSmsNotificationSender>();
builder.Services.AddScoped<INotificationSender, LoggingZaloNotificationSender>();

var dbConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:Default. Cấu hình bằng biến môi trường ConnectionStrings__Default hoặc appsettings.Development.json.");
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(dbConnectionString)));
builder.Services.AddHangfireServer();

// Phạm vi dữ liệu Portal đại lý
builder.Services.AddScoped<Polymind.Web.Identity.AgentScope>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Polymind.Web.Identity.TwoFactorStatusCache>();

// EF Core (PostgreSQL) + ASP.NET Core Identity
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck<MinioHealthCheck>("minio", tags: new[] { "ready" });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

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
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// ----- REST API (Phase H): JWT Bearer song song Cookie + Swagger/OpenAPI -----
// Giải quyết khóa ký JWT 1 lần để dùng chung cho cả sinh token và xác thực token.
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    if (builder.Environment.IsDevelopment())
        jwtOptions.Key = "dev-only-polymind-jwt-signing-key-change-in-production-1234567890";
    else
        throw new InvalidOperationException("Thiếu Jwt:Key — cấu hình biến môi trường Jwt__Key cho production.");
}
builder.Services.Configure<JwtOptions>(o =>
{
    o.Issuer = jwtOptions.Issuer;
    o.Audience = jwtOptions.Audience;
    o.Key = jwtOptions.Key;
    o.ExpiryMinutes = jwtOptions.ExpiryMinutes;
});
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // giữ nguyên claim "permission"/role như khi sinh token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,
        ClockSkew = TimeSpan.FromMinutes(1),
    };
});

// JSON cho API: enum hiển thị dạng chuỗi (vd "Facebook", "New").
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger/OpenAPI + nút Authorize JWT.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "POLYMIND API",
        Version = "v1",
        Description = "REST API (JWT) cho tích hợp ngoài: mobile, đối tác, lead intake. Lấy token tại POST /api/auth/login."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Dán JWT lấy từ /api/auth/login (không cần gõ tiền tố 'Bearer ')."
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc, null)] = new List<string>()
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// 2FA bắt buộc toàn hệ thống: tài khoản đăng nhập cookie nhưng chưa bật 2FA
// chỉ được vào trang cài đặt 2FA / đăng xuất, mọi trang khác chuyển về /account/2fa-setup.
app.Use(async (context, next) =>
{
    if (Polymind.Web.Identity.TwoFactorEnforcement.AppliesTo(context))
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var twoFactorCache = context.RequestServices.GetRequiredService<Polymind.Web.Identity.TwoFactorStatusCache>();
        var userId = userManager.GetUserId(context.User);
        if (userId is not null && !await twoFactorCache.IsEnabledAsync(userManager, userId))
        {
            context.Response.Redirect(Polymind.Web.Identity.TwoFactorEnforcement.SetupPath);
            return;
        }
    }
    await next();
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
            })
        });
    },
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    }
});

// Đăng xuất: xóa cookie rồi quay về trang login.
app.MapPost("/Account/Logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

// Xuất báo cáo CSV (gated reports:read).
app.MapCsvExportEndpoints();

// REST API (Phase H) + tài liệu Swagger.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "POLYMIND API v1");
    options.DocumentTitle = "POLYMIND API";
});
app.MapAuthApi();
app.MapLeadsApi();
app.MapCandidatesApi();
app.MapJobOrdersApi();

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
