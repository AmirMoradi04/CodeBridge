using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری تاریخچه‌ی رمزهای عبور.</summary>
public sealed class PasswordHistoryRepository
    : GenericRepository<PasswordHistory>, IPasswordHistoryRepository
{
    public PasswordHistoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PasswordHistory>> GetRecentAsync(
        int userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 1 : count;

        return await DbSet
            .AsNoTracking()
            .Where(history => history.UserId == userId)
            .OrderByDescending(history => history.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }
}
