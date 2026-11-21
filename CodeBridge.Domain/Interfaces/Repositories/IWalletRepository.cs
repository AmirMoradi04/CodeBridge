using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای کیف پول.</summary>
public interface IWalletRepository : IGenericRepository<Wallet>
{
    Task<Wallet?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Wallet?> GetWithTransactionsAsync(int walletId, CancellationToken cancellationToken = default);
}
