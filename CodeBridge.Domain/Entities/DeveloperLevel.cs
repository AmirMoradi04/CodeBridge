using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// سطح و امتیاز برنامه‌نویس؛ رابطه‌ی یک‌به‌یک با کاربر دارد.
/// </summary>
public class DeveloperLevel : BaseEntity
{
    public int UserId { get; private set; }

    public DeveloperLevelType Level { get; private set; }

    public int Score { get; private set; }

    public int ProjectsCompleted { get; private set; }

    public decimal? AverageRating { get; private set; }

    // --- Navigation Properties ---
    public User User { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private DeveloperLevel()
    {
    }

    public DeveloperLevel(int userId, DeveloperLevelType level, int score = 0, int projectsCompleted = 0)
    {
        UserId = userId;
        Level = level;
        Score = score;
        ProjectsCompleted = projectsCompleted;

        Validate();
    }

    public override void Validate()
    {
        if (Score < 0)
            throw new InvalidOperationException("امتیاز نمی‌تواند منفی باشد.");

        if (ProjectsCompleted < 0)
            throw new InvalidOperationException("تعداد پروژه‌های تکمیل‌شده نمی‌تواند منفی باشد.");
    }

    /// <summary>افزودن امتیاز و محاسبه‌ی مجدد سطح.</summary>
    public void AddScore(int scoreDelta)
    {
        var newScore = Score + scoreDelta;

        if (newScore < 0)
            throw new InvalidOperationException("امتیاز نمی‌تواند منفی باشد.");

        SetScoreAndLevel(newScore, CalculateLevel(newScore));
    }

    /// <summary>ثبت امتیاز و سطح محاسبه‌شده توسط لایه‌ی سرویس.</summary>
    public void SetScoreAndLevel(int score, DeveloperLevelType level)
    {
        if (score < 0)
            throw new InvalidOperationException("امتیاز نمی‌تواند منفی باشد.");

        Score = score;
        Level = level;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>ثبت میانگین امتیاز نظرات؛ مقدار null یعنی هنوز نظری ثبت نشده است.</summary>
    public void SetAverageRating(decimal? averageRating)
    {
        if (averageRating is < 0m or > 5m)
            throw new InvalidOperationException("میانگین امتیاز باید بین ۰ و ۵ باشد.");

        AverageRating = averageRating;
        SetUpdatedAt();
    }

    /// <summary>ثبت تکمیل پروژه و افزودن امتیاز.</summary>
    public void RecordCompletedProject(int scoreDelta)
    {
        if (ProjectsCompleted < 0)
            throw new InvalidOperationException("تعداد پروژه‌های تکمیل‌شده معتبر نیست.");

        ProjectsCompleted += 1;
        AddScore(scoreDelta);
    }

    private static DeveloperLevelType CalculateLevel(int score) => score switch
    {
        < 50 => DeveloperLevelType.JuniorA,
        < 100 => DeveloperLevelType.JuniorB,
        < 150 => DeveloperLevelType.JuniorC,
        < 250 => DeveloperLevelType.MidA,
        < 350 => DeveloperLevelType.MidB,
        < 500 => DeveloperLevelType.MidC,
        _ => DeveloperLevelType.Senior
    };
}
