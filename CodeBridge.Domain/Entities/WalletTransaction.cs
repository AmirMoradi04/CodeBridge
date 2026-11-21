using CodeBridge.Domain.Common;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// تراکنش کیف پول؛ هر جابه‌جایی مالی باید یک رکورد تراکنش داشته باشد.
/// RelatedEntityId برای ارجاع به ProjectId یا DailyProjectId استفاده می‌شود.
/// </summary>
public class WalletTransaction : BaseEntity
{
    public int WalletId { get; private set; }

    public decimal Amount { get; private set; }

    public WalletTransactionType Type { get; private set; }

    public string? Description { get; private set; }

    public int? RelatedEntityId { get; private set; }

    // --- Navigation Properties ---
    public Wallet Wallet { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private WalletTransaction()
    {
    }

    public WalletTransaction(
        int walletId,
        decimal amount,
        WalletTransactionType type,
        string? description = null,
        int? relatedEntityId = null)
    {
        WalletId = walletId;
        Amount = amount;
        Type = type;
        Description = description;
        RelatedEntityId = relatedEntityId;

        Validate();
    }

    public override void Validate()
    {
        if (Amount == 0)
            throw new InvalidOperationException("مبلغ تراکنش نمی‌تواند صفر باشد.");

        if (WalletId <= 0)
            throw new InvalidOperationException("شناسه‌ی کیف پول معتبر نیست.");
    }
}
