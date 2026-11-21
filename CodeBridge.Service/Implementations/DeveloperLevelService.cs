using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.DeveloperLevel;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>مدیریت سطح برنامه‌نویسان و محاسبه‌ی مجدد آن بر اساس امتیاز.</summary>
public sealed class DeveloperLevelService : IDeveloperLevelService
{
    // آستانه‌های سطح‌بندی طبق قواعد کسب‌وکار
    private const int JuniorBThreshold = 50;
    private const int JuniorCThreshold = 100;
    private const int MidAThreshold = 150;
    private const int MidBThreshold = 250;
    private const int MidCThreshold = 350;
    private const int SeniorThreshold = 500;

    private readonly IDeveloperLevelRepository _developerLevelRepository;
    private readonly IReviewRepository _reviewRepository;

    public DeveloperLevelService(
        IDeveloperLevelRepository developerLevelRepository,
        IReviewRepository reviewRepository)
    {
        _developerLevelRepository = developerLevelRepository
            ?? throw new ArgumentNullException(nameof(developerLevelRepository));
        _reviewRepository = reviewRepository
            ?? throw new ArgumentNullException(nameof(reviewRepository));
    }

    public async Task<Result<DeveloperLevelDto>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var level = await _developerLevelRepository.GetByUserIdAsync(userId, cancellationToken);

        return level is null
            ? Result<DeveloperLevelDto>.Failure("سطح برنامه‌نویس پیدا نشد.")
            : Result<DeveloperLevelDto>.Success(DeveloperLevelMapper.ToDto(level));
    }

    public async Task<Result<IReadOnlyList<DeveloperLevelDto>>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
    {
        var levels = await _developerLevelRepository.GetByLevelAsync(level, cancellationToken);

        return Result<IReadOnlyList<DeveloperLevelDto>>.Success(
            levels.Select(DeveloperLevelMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DeveloperLevelDto>>> GetTopAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            return Result<IReadOnlyList<DeveloperLevelDto>>.Failure("تعداد باید بزرگ‌تر از صفر باشد.");

        var levels = await _developerLevelRepository.GetTopDevelopersAsync(count, cancellationToken);

        return Result<IReadOnlyList<DeveloperLevelDto>>.Success(
            levels.Select(DeveloperLevelMapper.ToDto).ToList());
    }

    public async Task<Result> UpdateScoreAsync(
        int userId,
        int scoreDelta,
        CancellationToken cancellationToken = default)
    {
        var level = await _developerLevelRepository.GetByUserIdAsync(userId, cancellationToken);

        if (level is null)
            return Result.Failure("سطح برنامه‌نویس پیدا نشد.");

        var newScore = level.Score + scoreDelta;

        if (newScore < 0)
            return Result.Failure("امتیاز نمی‌تواند منفی باشد.");

        level.SetScoreAndLevel(newScore, CalculateLevel(newScore));

        _developerLevelRepository.Update(level);
        await _developerLevelRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// میانگین امتیاز برنامه‌نویس را از روی همه‌ی نظراتش بازمحاسبه و ذخیره می‌کند.
    /// اگر هیچ نظری ثبت نشده باشد، مقدار میانگین خالی می‌شود.
    /// </summary>
    public async Task<Result> RecalculateRatingAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var level = await _developerLevelRepository.GetByUserIdAsync(userId, cancellationToken);

        if (level is null)
            return Result.Failure("سطح برنامه‌نویس پیدا نشد.");

        var reviews = await _reviewRepository.GetByTargetUserIdAsync(userId, cancellationToken);

        decimal? averageRating = reviews.Count == 0
            ? null
            : Math.Round((decimal)reviews.Average(review => review.Rating), 2);

        level.SetAverageRating(averageRating);

        _developerLevelRepository.Update(level);
        await _developerLevelRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>تعیین سطح بر اساس آستانه‌های تعریف‌شده.</summary>
    private static DeveloperLevelType CalculateLevel(int score) => score switch
    {
        < JuniorBThreshold => DeveloperLevelType.JuniorA,
        < JuniorCThreshold => DeveloperLevelType.JuniorB,
        < MidAThreshold => DeveloperLevelType.JuniorC,
        < MidBThreshold => DeveloperLevelType.MidA,
        < MidCThreshold => DeveloperLevelType.MidB,
        < SeniorThreshold => DeveloperLevelType.MidC,
        _ => DeveloperLevelType.Senior
    };
}
