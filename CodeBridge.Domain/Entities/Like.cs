using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// لایک پست؛ ترکیب PostId و UserId در Infrastructure یکتا خواهد بود.
/// </summary>
public class Like : BaseEntity
{
    public int PostId { get; private set; }

    public int UserId { get; private set; }

    // --- Navigation Properties ---
    public Post Post { get; private set; } = null!;

    public User User { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private Like()
    {
    }

    public Like(int postId, int userId)
    {
        PostId = postId;
        UserId = userId;

        Validate();
    }

    public override void Validate()
    {
        if (PostId <= 0)
            throw new InvalidOperationException("شناسه‌ی پست معتبر نیست.");

        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر معتبر نیست.");
    }
}
