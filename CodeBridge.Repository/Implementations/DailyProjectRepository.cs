using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری پروژه‌های روزمره؛ فقط دسترسی به داده.</summary>
public sealed class DailyProjectRepository : GenericRepository<DailyProject>, IDailyProjectRepository
{
    public DailyProjectRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<DailyProject>> GetByEmployerIdAsync(
        int employerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.EmployerId == employerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyProject>> GetActiveForDevelopersAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.Status == DailyProjectStatus.Active && p.FilledSlots < p.Capacity)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyProject>> GetPendingAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(p => p.Employer)
            .Where(p => p.Status == DailyProjectStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyProject>> GetApprovedAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(p => p.Employer)
            .Where(p => p.Status == DailyProjectStatus.Approved)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<DailyProject?> GetWithApplicationsAsync(
        int projectId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(p => p.Applications)
                .ThenInclude(a => a.Developer)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
}