using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری پاداش‌های معرف؛ فقط دسترسی به داده.</summary>
public sealed class ReferralRewardRepository : GenericRepository<ReferralReward>, IReferralRewardRepository
{
    public ReferralRewardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ReferralReward>> GetByReferrerIdAsync(
        int referrerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(reward => reward.Referee)
            .Where(reward => reward.ReferrerId == referrerId)
            .OrderByDescending(reward => reward.Date)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalEarnedAsync(
        int referrerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(reward => reward.ReferrerId == referrerId)
            .SumAsync(reward => reward.Amount, cancellationToken);
}
