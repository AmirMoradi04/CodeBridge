using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری اعلان‌ها؛ جدیدترین اعلان‌ها اول برگردانده می‌شوند.</summary>
public sealed class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => DbSet.CountAsync(
            notification => notification.UserId == userId && !notification.IsRead,
            cancellationToken);

    public async Task MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        // برای تغییر وضعیت باید نمونه‌ها tracked باشند؛ پس AsNoTracking استفاده نمی‌شود.
        var unread = await DbSet
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return;

        foreach (var notification in unread)
            notification.MarkAsRead();

        await Context.SaveChangesAsync(cancellationToken);
    }
}
