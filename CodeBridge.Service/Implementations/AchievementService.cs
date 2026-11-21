using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Achievement;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>
/// ارزیابی قواعد دستاورد و اهدای نشان به کاربر.
/// هر قاعده جداگانه بررسی می‌شود و اهدای تکراری هرگز رخ نمی‌دهد.
/// </summary>
public sealed class AchievementService : IAchievementService
{
    // آستانه‌ی دستاورد «کسب‌کار برتر» به تومان
    private const decimal TopEarnerThreshold = 1_000_000m;

    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserAchievementRepository _userAchievementRepository;
    private readonly IDeveloperLevelRepository _developerLevelRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IWalletRepository _walletRepository;

    public AchievementService(
        IAchievementRepository achievementRepository,
        IUserAchievementRepository userAchievementRepository,
        IDeveloperLevelRepository developerLevelRepository,
        IReviewRepository reviewRepository,
        IWalletRepository walletRepository)
    {
        _achievementRepository = achievementRepository
            ?? throw new ArgumentNullException(nameof(achievementRepository));
        _userAchievementRepository = userAchievementRepository
            ?? throw new ArgumentNullException(nameof(userAchievementRepository));
        _developerLevelRepository = developerLevelRepository
            ?? throw new ArgumentNullException(nameof(developerLevelRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
    }

    public async Task<Result> CheckAndAwardAchievementsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result.Failure("شناسه‌ی کاربر معتبر نیست.");

        var achievements = await _achievementRepository.GetAllSeededAsync(cancellationToken);

        if (achievements.Count == 0)
            return Result.Success(); // هنوز دستاوردی کاشت نشده است

        var alreadyEarned = await _userAchievementRepository.GetByUserIdAsync(userId, cancellationToken);
        var earnedIds = alreadyEarned
            .Select(userAchievement => userAchievement.AchievementId)
            .ToHashSet();

        var satisfied = await EvaluateRulesAsync(userId, cancellationToken);

        var newAwards = new List<UserAchievement>();

        foreach (var achievement in achievements)
        {
            if (earnedIds.Contains(achievement.Id))
                continue;

            if (!satisfied.Contains(achievement.Type))
                continue;

            newAwards.Add(new UserAchievement(userId, achievement.Id));
        }

        if (newAwards.Count == 0)
            return Result.Success();

        await _userAchievementRepository.AddRangeAsync(newAwards, cancellationToken);
        await _userAchievementRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AchievementDto>>> GetUserAchievementsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var userAchievements = await _userAchievementRepository.GetByUserIdAsync(userId, cancellationToken);

        IReadOnlyList<AchievementDto> dtos = userAchievements
            .Select(userAchievement => new AchievementDto
            {
                Id = userAchievement.Id,
                Title = userAchievement.Achievement?.Title ?? string.Empty,
                Description = userAchievement.Achievement?.Description ?? string.Empty,
                IconUrl = userAchievement.Achievement?.IconUrl,
                Type = userAchievement.Achievement?.Type ?? AchievementType.FirstProject,
                EarnedAt = userAchievement.EarnedAt
            })
            .ToList();

        return Result<IReadOnlyList<AchievementDto>>.Success(dtos);
    }

    /// <summary>ارزیابی همه‌ی قواعد برای یک کاربر در یک بار خواندن داده‌ها.</summary>
    private async Task<HashSet<AchievementType>> EvaluateRulesAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var satisfied = new HashSet<AchievementType>();

        var level = await _developerLevelRepository.GetByUserIdAsync(userId, cancellationToken);

        // قاعده‌ی اولین پروژه
        if (level is { ProjectsCompleted: >= 1 })
            satisfied.Add(AchievementType.FirstProject);

        var reviews = await _reviewRepository.GetByTargetUserIdAsync(userId, cancellationToken);

        // قاعده‌ی ستاره‌ی پنج‌ستاره
        if (reviews.Any(review => review.Rating == Review.MaxRating))
            satisfied.Add(AchievementType.FiveStarHero);

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        // موجودی فعلی + برداشت‌ها = مجموع کل دریافتی کاربر
        if (wallet is not null && wallet.Balance + wallet.TotalWithdrawn >= TopEarnerThreshold)
            satisfied.Add(AchievementType.TopEarner);

        return satisfied;
    }
}
