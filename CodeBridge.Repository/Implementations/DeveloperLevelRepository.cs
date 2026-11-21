using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری سطح برنامه‌نویسان؛ فقط دسترسی به داده.</summary>
public sealed class DeveloperLevelRepository : GenericRepository<DeveloperLevel>, IDeveloperLevelRepository
{
    public DeveloperLevelRepository(AppDbContext context) : base(context)
    {
    }

    public Task<DeveloperLevel?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(level => level.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<DeveloperLevel>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(developerLevel => developerLevel.Level == level)
            .OrderByDescending(developerLevel => developerLevel.Score)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DeveloperLevel>> GetTopDevelopersAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 10 : count;

        return await DbSet
            .AsNoTracking()
            .OrderByDescending(level => level.Score)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>افزودن امتیاز و محاسبه‌ی مجدد سطح.</summary>
    public async Task UpdateScoreAsync(
        int userId,
        int scoreDelta,
        CancellationToken cancellationToken = default)
    {
        var developerLevel = await DbSet
            .FirstOrDefaultAsync(level => level.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("سطح برنامه‌نویس مورد نظر پیدا نشد.");

        developerLevel.AddScore(scoreDelta);
        Update(developerLevel);

        await SaveChangesAsync(cancellationToken);
    }
}
