using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری دستاوردهای کاربران؛ فقط دسترسی به داده.</summary>
public sealed class UserAchievementRepository : GenericRepository<UserAchievement>, IUserAchievementRepository
{
    public UserAchievementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UserAchievement>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(userAchievement => userAchievement.Achievement)
            .Where(userAchievement => userAchievement.UserId == userId)
            .OrderByDescending(userAchievement => userAchievement.EarnedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(
        int userId,
        int achievementId,
        CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(
            userAchievement => userAchievement.UserId == userId
                && userAchievement.AchievementId == achievementId,
            cancellationToken);
}
