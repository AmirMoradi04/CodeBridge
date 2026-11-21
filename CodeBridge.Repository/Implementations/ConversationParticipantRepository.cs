using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری شرکت‌کنندگان مکالمه؛ فقط دسترسی به داده.</summary>
public sealed class ConversationParticipantRepository
    : GenericRepository<ConversationParticipant>, IConversationParticipantRepository
{
    public ConversationParticipantRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ConversationParticipant>> GetByConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(participant => participant.User)
            .Where(participant => participant.ConversationId == conversationId)
            .OrderBy(participant => participant.JoinedAt)
            .ToListAsync(cancellationToken);

    public Task<ConversationParticipant?> GetByUserAndConversationAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                participant =>
                    participant.ConversationId == conversationId
                    && participant.UserId == userId,
                cancellationToken);

    /// <summary>عضویت تکراری اضافه نمی‌شود؛ ذخیره‌سازی بر عهده‌ی فراخواننده است.</summary>
    public async Task<bool> AddIfMissingAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var exists = await DbSet.AnyAsync(
            participant =>
                participant.ConversationId == conversationId && participant.UserId == userId,
            cancellationToken);

        if (exists)
            return false;

        await AddAsync(new ConversationParticipant(conversationId, userId), cancellationToken);
        return true;
    }
}