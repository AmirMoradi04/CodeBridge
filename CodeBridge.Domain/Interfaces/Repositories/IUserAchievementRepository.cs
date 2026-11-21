using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای دستاوردهای کسب‌شده توسط کاربران.</summary>
public interface IUserAchievementRepository : IGenericRepository<UserAchievement>
{
    Task<IReadOnlyList<UserAchievement>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int userId,
        int achievementId,
        CancellationToken cancellationToken = default);
}
