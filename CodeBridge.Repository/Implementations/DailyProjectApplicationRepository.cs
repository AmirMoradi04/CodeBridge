using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری درخواست‌های پروژه‌ی روزمره؛ فقط دسترسی به داده.</summary>
public sealed class DailyProjectApplicationRepository : GenericRepository<DailyProjectApplication>, IDailyProjectApplicationRepository
{
    public DailyProjectApplicationRepository(AppDbContext context) : base(context)
    {
    }

    public Task<DailyProjectApplication?> GetByProjectAndDeveloperAsync(
        int projectId,
        int developerId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.DailyProjectId == projectId && a.DeveloperId == developerId,
                cancellationToken);

    public async Task<IReadOnlyList<DailyProjectApplication>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(a => a.DailyProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyProjectApplication>> GetByDeveloperIdAsync(
        int developerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(a => a.DeveloperId == developerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyProjectApplication>> GetExpiredAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(a => a.Status == DailyApplicationStatus.InProgress
                && a.ExpiresAt.HasValue
                && DateTime.UtcNow > a.ExpiresAt.Value)
            .OrderBy(a => a.ExpiresAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountApprovedForProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => DbSet.CountAsync(
            a => a.DailyProjectId == projectId && a.Status == DailyApplicationStatus.Approved,
            cancellationToken);
}