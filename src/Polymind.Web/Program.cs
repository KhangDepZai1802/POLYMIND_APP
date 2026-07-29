using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;
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
using Polymind.Web.Finance;
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

// Giữ cookie đăng nhập/antiforgery hợp lệ sau khi container restart hoặc redeploy.
// Production mount một Docker volume vào đường dẫn này; local/dev tiếp tục dùng key ring mặc định.
var dataProtectionBuilder = builder.Services
    .AddDataProtection()
    .SetApplicationName("POLYMIND");
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

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
// RB-5: lưu hội thoại AI + kết quả trích xuất CV theo user, sống suốt phiên đăng nhập (xóa khi logout).
builder.Services.AddSingleton<Polymind.Web.Ai.AiSessionStore>();

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

// Rate limiting: chống dò mật khẩu (brute force) ở đăng nhập API. Phân vùng theo IP thật
// (đặt UseRateLimiter SAU UseForwardedHeaders để lấy đúng IP client sau reverse proxy/tunnel).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // Form đăng nhập WEB (`POST /login`) render tĩnh nên là một endpoint HTTP thật → chặn dò mật khẩu
    // được ở đây. Hạn mức nới hơn API (30/phút/IP) vì cả văn phòng thường dùng chung một IP NAT và
    // hay đăng nhập cùng lúc đầu giờ; brute force cần hàng nghìn lượt nên vẫn bị chặn.
    // Mọi request khác trả NoLimiter để không ảnh hưởng Blazor/SignalR.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        if (!HttpMethods.IsPost(httpContext.Request.Method)
            || !httpContext.Request.Path.StartsWithSegments("/login"))
            return RateLimitPartition.GetNoLimiter("unlimited");

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
    });
});

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
app.UseRateLimiter();
app.UseSerilogRequestLogging();
// Content-Security-Policy: chặn XSS/chèn script lạ. Để trong config (Security:ContentSecurityPolicy)
// để production siết/nới được bằng biến môi trường mà không phải build lại; đặt rỗng = tắt hẳn.
//   script-src 'self'        — toàn bộ script là file tĩnh, KHÔNG có inline script nào (đã rà App.razor).
//   style-src 'unsafe-inline'— MudBlazor đặt style trực tiếp trên element, bắt buộc phải cho.
//   blob:/data:              — nghe lại ghi âm tin nhắn thoại + xem trước ảnh giấy tờ đã upload.
//   ws:/wss:                 — SignalR circuit của Blazor Server.
//   fonts.googleapis/gstatic — font Roboto nạp từ Google Fonts trong App.razor.
var contentSecurityPolicy = builder.Configuration["Security:ContentSecurityPolicy"] ?? string.Join("; ",
    "default-src 'self'",
    "base-uri 'self'",
    "object-src 'none'",
    "frame-ancestors 'self'",
    "form-action 'self'",
    "script-src 'self'",
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
    "font-src 'self' data: https://fonts.gstatic.com",
    "img-src 'self' data: blob:",
    "media-src 'self' data: blob:",
    "connect-src 'self' ws: wss:");

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    // Cho phép MICRO cùng origin (self) để ghi âm tin nhắn thoại; camera/định vị vẫn chặn.
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(self), geolocation=()");
    if (!string.IsNullOrWhiteSpace(contentSecurityPolicy))
        context.Response.Headers.TryAdd("Content-Security-Policy", contentSecurityPolicy);
    await next();
});

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
app.MapPost("/Account/Logout", async (SignInManager<ApplicationUser> signInManager,
    Microsoft.AspNetCore.Http.HttpContext httpContext, Polymind.Web.Ai.AiSessionStore aiSessions) =>
{
    // RB-5: xóa hội thoại AI + kết quả trích xuất CV của người dùng khi đăng xuất.
    var uid = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (Guid.TryParse(uid, out var userId)) aiSessions.Clear(userId);
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

// Xuất báo cáo CSV (gated reports:read).
app.MapCsvExportEndpoints();

// REST API (Phase H) + tài liệu Swagger — CHỈ bật ở Development, tránh lộ "bản đồ" API ở production/public.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "POLYMIND API v1");
        options.DocumentTitle = "POLYMIND API";
    });
}
app.MapAuthApi();
app.MapLeadsApi();
app.MapCandidatesApi();
app.MapJobOrdersApi();

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        RecurringJob.AddOrUpdate<NotificationJob>(
            "polymind-notification-reminders",
            job => job.RunAsync(),
            "*/5 * * * *");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogWarning(ex, "Không thể đăng ký recurring job lúc khởi động — sẽ không có nhắc nhở tự động cho tới khi khắc phục kết nối Hangfire storage.");
    }
});

// Áp migration + seed roles/permissions/super_admin + dữ liệu mẫu (bỏ qua nếu DB chưa sẵn sàng).
using (var scope = app.Services.CreateScope())
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        await DbSeeder.SeedAsync(app.Services);
        if (app.Environment.IsDevelopment())
            await DemoDataSeeder.SeedAsync(app.Services);
        // Khoản thu đã duyệt mà thiếu phiếu thu (seed cũ / dữ liệu trước khi duyệt tự lập phiếu) → lập bù.
        await ReceiptBackfillService.RunAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Bỏ qua seed DB — kiểm tra PostgreSQL đã chạy (docker compose up) chưa.");
    }
}

app.Run();
