using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری دستاوردها؛ فقط دسترسی به داده.</summary>
public sealed class AchievementRepository : GenericRepository<Achievement>, IAchievementRepository
{
    public AchievementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Achievement>> GetAllSeededAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .OrderBy(achievement => achievement.Type)
            .ToListAsync(cancellationToken);
}
