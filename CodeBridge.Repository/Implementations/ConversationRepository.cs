using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری مکالمه‌ها؛ فقط دسترسی به داده.</summary>
public sealed class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Conversation>> GetForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(conversation => conversation.Participants)
                .ThenInclude(participant => participant.User)
            .Where(conversation =>
                conversation.Participants.Any(participant => participant.UserId == userId))
            .OrderByDescending(conversation => conversation.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Conversation?> GetWithParticipantsAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(conversation => conversation.Participants)
                .ThenInclude(participant => participant.User)
            .Include(conversation => conversation.Project)
            .FirstOrDefaultAsync(conversation => conversation.Id == conversationId, cancellationToken);

    public Task<Conversation?> GetProjectGroupAsync(
        int projectId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(conversation => conversation.Participants)
            .FirstOrDefaultAsync(
                conversation =>
                    conversation.Type == ConversationType.ProjectGroup
                    && conversation.ProjectId == projectId,
                cancellationToken);
}