using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>پاداش ثبت‌شده برای یک دعوت موفق؛ هر زوج دعوت‌کننده/دعوت‌شونده یک رکورد دارد.</summary>
public class ReferralReward : BaseEntity
{
    public int ReferrerId { get; private set; }

    public int RefereeId { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>زمان ثبت پاداش.</summary>
    public DateTime Date { get; private set; }

    // --- Navigation Properties ---
    public User Referrer { get; private set; } = null!;

    public User Referee { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private ReferralReward()
    {
    }

    public ReferralReward(int referrerId, int refereeId, decimal amount)
    {
        ReferrerId = referrerId;
        RefereeId = refereeId;
        Amount = amount;
        Date = DateTime.UtcNow;

        Validate();
    }

    public override void Validate()
    {
        if (ReferrerId <= 0 || RefereeId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربران پاداش معتبر نیست.");

        if (ReferrerId == RefereeId)
            throw new InvalidOperationException("کاربر نمی‌تواند به خودش پاداش معرف بدهد.");

        if (Amount < 0)
            throw new InvalidOperationException("مبلغ پاداش نمی‌تواند منفی باشد.");
    }
}
