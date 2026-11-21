using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای پاداش‌های معرف.</summary>
public interface IReferralRewardRepository : IGenericRepository<ReferralReward>
{
    Task<IReadOnlyList<ReferralReward>> GetByReferrerIdAsync(
        int referrerId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalEarnedAsync(int referrerId, CancellationToken cancellationToken = default);
}
