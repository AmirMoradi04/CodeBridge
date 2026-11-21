using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.DeveloperLevel;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد مدیریت سطح و امتیاز برنامه‌نویسان.</summary>
public interface IDeveloperLevelService
{
    Task<Result<DeveloperLevelDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DeveloperLevelDto>>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DeveloperLevelDto>>> GetTopAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateScoreAsync(
        int userId,
        int scoreDelta,
        CancellationToken cancellationToken = default);

    /// <summary>بازمحاسبه‌ی میانگین امتیاز برنامه‌نویس بر اساس نظرات ثبت‌شده.</summary>
    Task<Result> RecalculateRatingAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
