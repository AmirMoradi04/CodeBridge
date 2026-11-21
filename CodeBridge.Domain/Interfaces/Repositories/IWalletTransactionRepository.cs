using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای تراکنش‌های کیف پول.</summary>
public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
{
    Task<IReadOnlyList<WalletTransaction>> GetByWalletIdAsync(
        int walletId,
        CancellationToken cancellationToken = default);

    /// <summary>همه‌ی تراکنش‌های سیستم همراه با کیف پول صاحب آن (گزارش ادمین).</summary>
    Task<IReadOnlyList<WalletTransaction>> GetAllWithWalletAsync(
        CancellationToken cancellationToken = default);

    /// <summary>تازه‌ترین واریزها برای فید فعالیت.</summary>
    Task<IReadOnlyList<WalletTransaction>> GetRecentDepositsAsync(
        int count,
        CancellationToken cancellationToken = default);
}
