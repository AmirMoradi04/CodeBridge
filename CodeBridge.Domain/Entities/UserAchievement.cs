using CodeBridge.Domain.Common;

namespace CodeBridge.Domain.Entities;

/// <summary>دستاورد کسب‌شده توسط یک کاربر.</summary>
public class UserAchievement : BaseEntity
{
    public int UserId { get; private set; }

    public int AchievementId { get; private set; }

    /// <summary>زمان کسب دستاورد.</summary>
    public DateTime EarnedAt { get; private set; }

    // --- Navigation Properties ---
    public Identity.User User { get; private set; } = null!;

    public Achievement Achievement { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private UserAchievement()
    {
    }

    public UserAchievement(int userId, int achievementId)
    {
        UserId = userId;
        AchievementId = achievementId;
        EarnedAt = DateTime.UtcNow;

        Validate();
    }

    public override void Validate()
    {
        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر معتبر نیست.");

        if (AchievementId <= 0)
            throw new InvalidOperationException("شناسه‌ی دستاورد معتبر نیست.");
    }
}
