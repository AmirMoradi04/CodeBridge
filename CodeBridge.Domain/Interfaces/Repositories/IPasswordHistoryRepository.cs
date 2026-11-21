using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>دسترسی به تاریخچه‌ی رمزهای عبور کاربران.</summary>
public interface IPasswordHistoryRepository : IGenericRepository<PasswordHistory>
{
    /// <summary>جدیدترین هش‌های رمز یک کاربر.</summary>
    Task<IReadOnlyList<PasswordHistory>> GetRecentAsync(
        int userId,
        int count,
        CancellationToken cancellationToken = default);
}
