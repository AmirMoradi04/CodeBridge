using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری پیام‌ها؛ فقط دسترسی به داده.</summary>
public sealed class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Message>> GetByConversationAsync(
        int conversationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 20 : pageSize;

        return await DbSet
            .AsNoTracking()
            .Include(message => message.Sender)
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.CreatedAt)
            .ThenBy(message => message.Id)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
        => DbSet.CountAsync(
            message => message.ConversationId == conversationId,
            cancellationToken);

    public Task<int> CountUnreadAsync(
        int conversationId,
        int userId,
        DateTime? lastReadAt,
        CancellationToken cancellationToken = default)
    {
        if (lastReadAt.HasValue)
        {
            return DbSet.CountAsync(
                message =>
                    message.ConversationId == conversationId
                    && message.SenderId != userId
                    && message.CreatedAt > lastReadAt.Value,
                cancellationToken);
        }

        // اگر کاربر هرگز مکالمه را نخوانده باشد، همه‌ی پیام‌های دیگران خوانده‌نشده‌اند
        return DbSet.CountAsync(
            message =>
                message.ConversationId == conversationId
                && message.SenderId != userId,
            cancellationToken);
    }
}