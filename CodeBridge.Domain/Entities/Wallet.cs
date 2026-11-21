using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// کیف پول کاربر؛ تمام گردش مالی از طریق همین انتیتی انجام می‌شود.
/// ادمین نیز به‌عنوان حساب امانی (Escrow) کیف پول دارد.
/// </summary>
public class Wallet : BaseEntity
{
    public int UserId { get; private set; }

    public WalletOwnerType OwnerType { get; private set; }

    public decimal Balance { get; private set; }

    public decimal TotalDeposited { get; private set; }

    public decimal TotalWithdrawn { get; private set; }

    // --- Navigation Properties ---
    public User User { get; private set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private Wallet()
    {
    }

    public Wallet(int userId, WalletOwnerType ownerType)
    {
        UserId = userId;
        OwnerType = ownerType;
        Balance = 0;
        TotalDeposited = 0;
        TotalWithdrawn = 0;

        Validate();
    }

    public override void Validate()
    {
        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر کیف پول معتبر نیست.");

        if (Balance < 0)
            throw new InvalidOperationException("موجودی کیف پول نمی‌تواند منفی باشد.");

        if (TotalDeposited < 0)
            throw new InvalidOperationException("مجموع واریزها نمی‌تواند منفی باشد.");

        if (TotalWithdrawn < 0)
            throw new InvalidOperationException("مجموع برداشت‌ها نمی‌تواند منفی باشد.");
    }

    /// <summary>واریز مستقیم (شارژ توسط کاربر یا درگاه پرداخت).</summary>
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ واریز باید بزرگ‌تر از صفر باشد.");

        Balance += amount;
        TotalDeposited += amount;
        SetUpdatedAt();
    }

    /// <summary>برداشت؛ تنها در صورت کفایت موجودی موفق است.</summary>
    public bool TryWithdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ برداشت باید بزرگ‌تر از صفر باشد.");

        if (Balance < amount)
            return false;

        Balance -= amount;
        TotalWithdrawn += amount;
        SetUpdatedAt();

        return true;
    }

    /// <summary>افزایش موجودی (مثلاً اعتباردهی توسط ادمین به توسعه‌دهنده).</summary>
    public void AddFunds(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ افزایش باید بزرگ‌تر از صفر باشد.");

        Balance += amount;
        SetUpdatedAt();
    }

    /// <summary>کاهش موجودی (مثلاً کسر کارمزد).</summary>
    public void DeductFunds(decimal amount)
    {
        if (amount < 0)
            throw new InvalidOperationException("مبلغ کسر نمی‌تواند منفی باشد.");

        if (amount > Balance)
            throw new InvalidOperationException("موجودی کیف پول کافی نیست.");

        Balance -= amount;
        SetUpdatedAt();
    }
}
