using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>عضویت پولی یک کارجو در جلسه‌ی زنده؛ پس از پرداخت ثبت می‌شود.</summary>
public class LiveSessionParticipant : BaseEntity
{
    public int LiveSessionId { get; private set; }

    public int JuniorId { get; private set; }

    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    // --- Navigation Properties ---
    public LiveSession LiveSession { get; private set; } = null!;

    public User Junior { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private LiveSessionParticipant()
    {
    }

    public LiveSessionParticipant(int liveSessionId, int juniorId)
    {
        LiveSessionId = liveSessionId;
        JuniorId = juniorId;
        JoinedAt = DateTime.UtcNow;

        Validate();
    }

    public override void Validate()
    {
        if (LiveSessionId <= 0)
            throw new InvalidOperationException("شناسه‌ی جلسه معتبر نیست.");

        if (JuniorId <= 0)
            throw new InvalidOperationException("شناسه‌ی کارجو معتبر نیست.");
    }
}
