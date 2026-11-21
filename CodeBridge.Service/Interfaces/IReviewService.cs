using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Review;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ثبت و مشاهده‌ی امتیازهای پروژه‌ها.</summary>
public interface IReviewService
{
    /// <summary>ثبت امتیاز توسط کارفرمای پروژه‌ی تکمیل‌شده.</summary>
    Task<Result> SubmitReviewAsync(
        int reviewerId,
        CreateReviewDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ReviewDto>>> GetUserReviewsAsync(
        int targetUserId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ReviewDto>>> GetProjectReviewsAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    /// <summary>فهرست کامل امتیازها برای پنل ادمین.</summary>
    Task<Result<IReadOnlyList<ReviewDto>>> GetAllReviewsAsync(
        CancellationToken cancellationToken = default);
}
