using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای کاربران.</summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetWithResumeAndLevelAsync(int userId, CancellationToken cancellationToken = default);

    Task<User?> GetWithRefreshTokensAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

    /// <summary>همه‌ی کاربران همراه با سطح برنامه‌نویسی (برای فیلتر سطح‌بندی).</summary>
    Task<IReadOnlyList<User>> GetAllWithLevelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetJobSeekersByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    /// <summary>تازه‌ترین کاربران ثبت‌نام‌شده برای فید فعالیت.</summary>
    Task<IReadOnlyList<User>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    Task ApproveUserAsync(int userId, CancellationToken cancellationToken = default);

    Task RejectUserAsync(int userId, CancellationToken cancellationToken = default);
}
