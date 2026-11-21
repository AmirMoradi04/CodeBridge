using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری شرکت‌کنندگان جلسه‌ی زنده؛ فقط دسترسی به داده.</summary>
public sealed class LiveSessionParticipantRepository
    : GenericRepository<LiveSessionParticipant>, ILiveSessionParticipantRepository
{
    public LiveSessionParticipantRepository(AppDbContext context) : base(context)
    {
    }

    public Task<LiveSessionParticipant?> GetBySessionAndJuniorAsync(
        int sessionId,
        int juniorId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                participant => participant.LiveSessionId == sessionId
                    && participant.JuniorId == juniorId,
                cancellationToken);

    public async Task<IReadOnlyList<LiveSessionParticipant>> GetBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(participant => participant.Junior)
            .Where(participant => participant.LiveSessionId == sessionId)
            .OrderBy(participant => participant.JoinedAt)
            .ToListAsync(cancellationToken);
}
