using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری کیف پول؛ فقط دسترسی به داده.</summary>
public sealed class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    public WalletRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Wallet?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public Task<Wallet?> GetWithTransactionsAsync(int walletId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAt))
            .FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
}