using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای امتیازها و نظرات.</summary>
public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByTargetUserIdAsync(
        int targetUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    /// <summary>آیا همین کارفرما قبلاً برای همین پروژه و همین کاربر امتیاز ثبت کرده است.</summary>
    Task<bool> HasReviewedAsync(
        int projectId,
        int reviewerId,
        int targetUserId,
        CancellationToken cancellationToken = default);

    /// <summary>همه‌ی امتیازها همراه با اطلاعات نمایشی؛ مخصوص پنل ادمین.</summary>
    Task<IReadOnlyList<Review>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>تازه‌ترین نظرات ۵ ستاره برای فید فعالیت.</summary>
    Task<IReadOnlyList<Review>> GetRecentFiveStarAsync(
        int count,
        CancellationToken cancellationToken = default);
}
