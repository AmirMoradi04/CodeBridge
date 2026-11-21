using System.Security.Claims;
using System.Text;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using CodeBridge.Presentation.Implementations;
using CodeBridge.Presentation.Middlewares;
using CodeBridge.Repository.Implementations;
using CodeBridge.Service.Common;
using CodeBridge.Service.Implementations;
using CodeBridge.Service.Interfaces;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// ۱) دیتابیس: رشته‌ی اتصال از appsettings.json
// ------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});
// ------------------------------------------------------------
// ۲) ASP.NET Core Identity
// ------------------------------------------------------------
builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ------------------------------------------------------------
// ۳) ریپازیتوری‌ها (پیاده‌سازی‌های EF Core از پروژه‌ی Repository)
// ------------------------------------------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IDeveloperLevelRepository, DeveloperLevelRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IConversationParticipantRepository, ConversationParticipantRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IDailyProjectRepository, DailyProjectRepository>();
builder.Services.AddScoped<IDailyProjectApplicationRepository, DailyProjectApplicationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ILiveSessionRepository, LiveSessionRepository>();
builder.Services.AddScoped<ILiveSessionParticipantRepository, LiveSessionParticipantRepository>();
builder.Services.AddScoped<IReferralRewardRepository, ReferralRewardRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
builder.Services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();

// ------------------------------------------------------------
// ۴) سرویس‌های کسب‌وکار
// ------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IDeveloperLevelService, DeveloperLevelService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IDailyProjectService, DailyProjectService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IActivityFeedService, ActivityFeedService>();
builder.Services.AddScoped<ILiveSessionService, LiveSessionService>();

// انتزاع‌های بیرونی سرویس (هش رمز، JWT، RefreshToken و ذخیره‌فایل)
builder.Services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IEmailService, MockConsoleEmailService>();

// Hangfire: پردازش پس‌زمینه (انقضای خودکار تسک‌ها) روی همان دیتابیس SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

// ------------------------------------------------------------
// ۵) کنترلرها
// ------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// ------------------------------------------------------------
// ۶) Swagger + Bearer
// ------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CodeBridge API",
        Version = "v1",
        Description = "API پلتفرم اتصال کارفرمایان و برنامه‌نویسان"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "توکن JWT را به صورت: Bearer {token} وارد کنید"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------------------------------------------------
// ۷) احراز هویت JWT
// ------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var configuredSecret = jwtSection["SecretKey"];
const string secretPlaceholder = "REPLACE_ME_IN_PRODUCTION";

// مقدار پیش‌فرض appsettings فقط یک placeholder است؛ در تولید باید کلید واقعی
// از طریق متغیر محیطی JWT_SECRET یا مقداردهی appsettings واقعی تأمین شود.
var jwtSecretKey = !string.IsNullOrWhiteSpace(configuredSecret)
        && configuredSecret != secretPlaceholder
    ? configuredSecret
    : Environment.GetEnvironmentVariable("JWT_SECRET");

if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "کلید Jwt:SecretKey باید حداقل ۳۲ کاراکتر باشد؛ مقدار placeholder باید با کلید واقعی جایگزین شود (متغیر محیطی JWT_SECRET).");
}
var jwtIssuer = jwtSection["Issuer"] ?? "CodeBridge";
var jwtAudience = jwtSection["Audience"] ?? "CodeBridgeClient";

// تنظیمات تایپ‌شده‌ی JWT برای سرویس احراز هویت
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new JwtSettings
{
    Key = jwtSecretKey,
    Issuer = jwtIssuer,
    Audience = jwtAudience,
    AccessTokenExpirationMinutes = jwtSection.GetValue<int?>("ExpirationMinutes") ?? 15,
    RefreshTokenExpirationDays = jwtSection.GetValue<int?>("RefreshTokenExpirationDays") ?? 7
}));

// سیاست قفل موقت حساب پس از تلاش‌های ناموفق ورود
var securitySection = builder.Configuration.GetSection("Security");
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SecuritySettings
{
    MaxFailedLoginAttempts = securitySection.GetValue<int?>("MaxFailedLoginAttempts") ?? 5,
    LockoutMinutes = securitySection.GetValue<int?>("LockoutMinutes") ?? 15
}));

// تنظیمات پاداش سیستم معرف
var referralSection = builder.Configuration.GetSection("Referral");
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ReferralSettings
{
    RewardAmount = referralSection.GetValue<decimal?>("RewardAmount") ?? 50000m
}));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            // claim نقشِ تولیدشده توسط JwtTokenGenerator برای [Authorize(Roles=...)] خوانده شود
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// ۷.۱) CORS برای فرانت‌اند Next.js
// ------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// ------------------------------------------------------------
// ۸) آماده‌سازی دیتابیس: اعمال مهاجرت‌ها + نقش‌ها + ادمین اولیه از appsettings
// ------------------------------------------------------------
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var adminEmail = app.Configuration["Seed:AdminEmail"]?.Trim();
        var adminPassword = app.Configuration["Seed:AdminPassword"];

        // ادمین فقط از روی تنظیمات ساخته می‌شود؛ مقدار پیش‌فرضی در کد وجود ندارد
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Seed:AdminEmail و Seed:AdminPassword باید در appsettings.json تنظیم شوند.");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await DbInitializer.InitializeAsync(dbContext, adminEmail, adminPassword);

        logger.LogInformation("آماده‌سازی دیتابیس انجام شد؛ حساب ادمین «{AdminEmail}» آماده است.", adminEmail);
    }
    catch (Exception exception)
    {
        // در دسترس نبودن دیتابیس نباید مانع بالا آمدن API و نمایش خطای واقعی در لاگ شود
        logger.LogError(exception, "آماده‌سازی دیتابیس ناموفق بود؛ ادمین اولیه ساخته نشد.");
    }
}

// ------------------------------------------------------------
// ۹) خط لوله‌ی پردازش درخواست
// ------------------------------------------------------------


app.UseCors("Frontend");


app.UseMiddleware<ExceptionHandlingMiddleware>();



// فایل‌های آپلودی (wwwroot/uploads) برای دانلود مستقیم
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeBridge API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// داشبورد Hangfire؛ فقط برای ادمین‌ها
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminOnlyDashboardAuthorizationFilter() }
});

// انقضای خودکار تسک‌های روزمره‌ی مهلت‌گذشته (هر دقیقه)
RecurringJob.AddOrUpdate<IDailyProjectService>(
    "expire-stale-daily-applications",
    service => service.ExpireStaleApplicationsAsync(default),
    Cron.Minutely());

app.MapControllers();

app.Run();
