using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>دسترسی به پیام‌های مکالمه.</summary>
public interface IMessageRepository : IGenericRepository<Message>
{
    /// <summary>پیام‌های یک مکالمه به ترتیب زمانی (قدیمی‌ترین اول) با صفحه‌بندی.</summary>
    Task<IReadOnlyList<Message>> GetByConversationAsync(
        int conversationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>تعداد کل پیام‌های یک مکالمه.</summary>
    Task<int> CountByConversationAsync(int conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعداد پیام‌های خوانده‌نشده برای یک کاربر: پیام‌های دیگران که
    /// بعد از LastReadAt (یا همه‌ی پیام‌های دیگران در صورت خالی بودن) ارسال شده‌اند.
    /// </summary>
    Task<int> CountUnreadAsync(
        int conversationId,
        int userId,
        DateTime? lastReadAt,
        CancellationToken cancellationToken = default);
}