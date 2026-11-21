using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// تاریخچه‌ی رمزهای عبور کاربر برای جلوگیری از استفاده‌ی مجدد از رمزهای اخیر.
/// فقط هش رمز ذخیره می‌شود؛ هرگز متن رمز.
/// </summary>
public class PasswordHistory : BaseEntity
{
    public int UserId { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    // --- Navigation Properties ---
    public User User { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private PasswordHistory()
    {
    }

    public PasswordHistory(int userId, string passwordHash)
    {
        UserId = userId;
        PasswordHash = passwordHash;

        Validate();
    }

    public override void Validate()
    {
        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر معتبر نیست.");

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new InvalidOperationException("هش رمز عبور الزامی است.");
    }
}
