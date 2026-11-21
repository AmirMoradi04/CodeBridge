using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری تراکنش‌های کیف پول؛ فقط دسترسی به داده.</summary>
public sealed class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<WalletTransaction>> GetByWalletIdAsync(
        int walletId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WalletTransaction>> GetAllWithWalletAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <summary>تازه‌ترین واریزها برای فید فعالیت.</summary>
    public async Task<IReadOnlyList<WalletTransaction>> GetRecentDepositsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 10 : count;

        return await DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .Where(t => t.Type == WalletTransactionType.Deposit)
            .OrderByDescending(t => t.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }
}