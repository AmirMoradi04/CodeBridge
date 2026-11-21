using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// توکن تازه‌سازی نشست؛ امکان ابطال و بررسی اعتبار دارد.
/// </summary>
public class RefreshToken : BaseEntity
{
    public int UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? CreatedByIp { get; private set; }

    /// <summary>توکن جایگزینی که هنگام چرخش توکن صادر شده است.</summary>
    public string? ReplacedByToken { get; private set; }

    /// <summary>توکنی که منقضی یا باطل نشده باشد فعال است.</summary>
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    // --- Navigation Properties ---
    public User User { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private RefreshToken()
    {
    }

    public RefreshToken(int userId, string token, DateTime expiresAt, string? createdByIp = null)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Token))
            throw new InvalidOperationException("توکن تازه‌سازی الزامی است.");

        if (ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("زمان انقضای توکن باید در آینده باشد.");
    }

    /// <summary>ابطال توکن همراه با ثبت زمان، IP و توکن جایگزین.</summary>
    public void Revoke(string? ip = null, string? replacedByToken = null)
    {
        if (RevokedAt is not null)
            return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ip;
        ReplacedByToken = replacedByToken;
    }
}
