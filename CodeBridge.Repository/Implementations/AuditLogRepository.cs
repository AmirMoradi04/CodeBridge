using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری گزارش‌های ممیزی؛ فقط دسترسی به داده.</summary>
public sealed class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(log => log.EntityType == entityType && log.EntityId == entityId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 20 : count;

        return await DbSet
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }
}
