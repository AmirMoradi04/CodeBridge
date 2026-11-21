using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری جلسه‌های زنده؛ فقط دسترسی به داده.</summary>
public sealed class LiveSessionRepository : GenericRepository<LiveSession>, ILiveSessionRepository
{
    public LiveSessionRepository(AppDbContext context) : base(context)
    {
    }

    public Task<LiveSession?> GetWithDetailsAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(session => session.Project)
            .Include(session => session.Senior)
            .Include(session => session.Participants)
                .ThenInclude(participant => participant.Junior)
            .FirstOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public async Task<IReadOnlyList<LiveSession>> GetBySeniorIdAsync(
        int seniorId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(session => session.Project)
            .Include(session => session.Senior)
            .Include(session => session.Participants)
            .Where(session => session.SeniorId == seniorId)
            .OrderByDescending(session => session.ScheduledAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LiveSession>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(session => session.Project)
            .Include(session => session.Senior)
            .Include(session => session.Participants)
            .Where(session => session.Status == LiveSessionStatus.Scheduled
                || session.Status == LiveSessionStatus.Live)
            .OrderBy(session => session.ScheduledAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LiveSession>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(session => session.Project)
            .Include(session => session.Senior)
            .Include(session => session.Participants)
            .OrderByDescending(session => session.ScheduledAt)
            .ToListAsync(cancellationToken);
}
