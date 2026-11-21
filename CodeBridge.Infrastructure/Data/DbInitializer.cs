using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Infrastructure.Data;

/// <summary>
/// آماده‌سازی دیتابیس: اعمال مهاجرت‌ها، ساخت نقش‌های پایه و ادمین اولیه.
/// بدون ادمین اولیه هیچ حساب تازه‌ای تایید نمی‌شد و ورود ممکن نبود.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await context.Database.MigrateAsync(cancellationToken);

        // نقش‌های Identity هم‌نام با UserRole
        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            var exists = await context.Roles.AnyAsync(role => role.Name == roleName, cancellationToken);

            if (!exists)
                context.Roles.Add(new Role(roleName));
        }

        await context.SaveChangesAsync(cancellationToken);

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();

        var admin = await context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (admin is null)
        {
            // ادمین اولیه مستقیماً تاییدشده ساخته می‌شود
            admin = new User("مدیر", "سیستم", normalizedEmail, UserRole.Admin)
            {
                UserName = normalizedEmail,
                NormalizedUserName = normalizedEmail.ToUpperInvariant(),
                NormalizedEmail = normalizedEmail.ToUpperInvariant()
            };

            admin.Approve();
            admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, adminPassword);

            context.Users.Add(admin);
            await context.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await context.Roles
            .FirstAsync(role => role.Name == nameof(UserRole.Admin), cancellationToken);

        var hasIdentityRole = await context.UserRoles
            .AnyAsync(userRole => userRole.UserId == admin.Id && userRole.RoleId == adminRole.Id, cancellationToken);

        if (!hasIdentityRole)
        {
            context.UserRoles.Add(new IdentityUserRole<int>
            {
                UserId = admin.Id,
                RoleId = adminRole.Id
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        await SeedAchievementsAsync(context, cancellationToken);
    }

    /// <summary>کاشت دستاوردهای پایه؛ هر نوع فقط یک بار ثبت می‌شود.</summary>
    private static async Task SeedAchievementsAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new Achievement(
                "اولین پروژه",
                "تکمیل اولین پروژه‌ی سفارشی در CodeBridge",
                AchievementType.FirstProject,
                "/badges/first-project.svg"),
            new Achievement(
                "ستاره‌ی پنج‌ستاره",
                "دریافت امتیاز ۵ ستاره از یک کارفرما",
                AchievementType.FiveStarHero,
                "/badges/five-star-hero.svg"),
            new Achievement(
                "کسب‌کار برتر",
                "دریافت مجموع ۱,۰۰۰,۰۰۰ تومان از پلتفرم",
                AchievementType.TopEarner,
                "/badges/top-earner.svg")
        };

        foreach (var seed in seeds)
        {
            var exists = await context.Achievements
                .AnyAsync(achievement => achievement.Type == seed.Type, cancellationToken);

            if (!exists)
                context.Achievements.Add(seed);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
