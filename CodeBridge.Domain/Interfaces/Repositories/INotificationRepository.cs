using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای اعلان‌های کاربران.</summary>
public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>همه‌ی اعلان‌های خوانده‌نشده‌ی کاربر را خوانده‌شده می‌کند.</summary>
    Task MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
