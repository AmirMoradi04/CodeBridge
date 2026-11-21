using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// قرارداد یک‌به‌یک با پروژه؛ سهم‌ها باید با مبلغ کل هم‌خوانی داشته باشند.
/// </summary>
public class Contract : BaseEntity
{
    public int ProjectId { get; private set; }

    public int DeveloperId { get; private set; }

    public int EmployerId { get; private set; }

    public int AdminId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal PlatformFee { get; private set; }

    public decimal DeveloperShare { get; private set; }

    public decimal? MentorShare { get; private set; }

    public int? MentorId { get; private set; }

    public ContractStatus Status { get; private set; } = ContractStatus.Draft;

    public DateTime? SignedAt { get; private set; }

    /// <summary>امضای توافق‌نامه‌ی عدم افشا (NDA) توسط برنامه‌نویس.</summary>
    public bool IsNdaSigned { get; private set; }

    public DateTime? NdaSignedAt { get; private set; }

    /// <summary>مبلغ واریزشده توسط کارفرما برای تسویه.</summary>
    public decimal DepositPaid { get; private set; }

    public bool IsFullyPaid { get; private set; }

    // --- Navigation Properties ---
    public Project Project { get; private set; } = null!;

    public User Developer { get; private set; } = null!;

    public User Employer { get; private set; } = null!;

    public User Admin { get; private set; } = null!;

    public User? Mentor { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private Contract()
    {
    }

    public Contract(
        int projectId,
        int developerId,
        int employerId,
        int adminId,
        decimal totalAmount,
        decimal platformFee,
        decimal developerShare,
        decimal? mentorShare = null)
    {
        ProjectId = projectId;
        DeveloperId = developerId;
        EmployerId = employerId;
        AdminId = adminId;
        TotalAmount = totalAmount;
        PlatformFee = platformFee;
        DeveloperShare = developerShare;
        MentorShare = mentorShare;
        Status = ContractStatus.Draft;

        Validate();
    }

    public override void Validate()
    {
        if (TotalAmount <= 0)
            throw new InvalidOperationException("مبلغ کل قرارداد باید بزرگ‌تر از صفر باشد.");

        if (PlatformFee < 0 || DeveloperShare < 0 || MentorShare < 0)
            throw new InvalidOperationException("سهم‌های قرارداد نمی‌توانند منفی باشند.");

        // مجموع سهم‌ها باید دقیقاً برابر مبلغ کل باشد
        var sharesTotal = PlatformFee + DeveloperShare + (MentorShare ?? 0m);

        if (sharesTotal != TotalAmount)
            throw new InvalidOperationException("مجموع سهم‌ها با مبلغ کل قرارداد هم‌خوانی ندارد.");

        // مبلغ واریز شده نباید از مبلغ کل بیشتر شود
        if (DepositPaid > TotalAmount)
            throw new InvalidOperationException("مبلغ واریزشده نمی‌تواند از مبلغ کل قرارداد بیشتر باشد.");
    }

    public void Sign()
    {
        if (Status != ContractStatus.Draft)
            throw new InvalidOperationException("فقط قرارداد پیش‌نویس قابل امضا است.");

        Status = ContractStatus.Signed;
        SignedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Activate()
    {
        if (Status != ContractStatus.Signed)
            throw new InvalidOperationException("فقط قرارداد امضاشده قابل فعال‌سازی است.");

        Status = ContractStatus.Active;
        SetUpdatedAt();
    }

    public void Complete()
    {
        if (Status != ContractStatus.Active)
            throw new InvalidOperationException("فقط قرارداد فعال قابل تکمیل است.");

        Status = ContractStatus.Completed;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == ContractStatus.Completed)
            throw new InvalidOperationException("قرارداد تکمیل‌شده قابل لغو نیست.");

        Status = ContractStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>امضای NDA توسط برنامه‌نویس؛ فقط یک بار امکان‌پذیر است.</summary>
    public void SignNda()
    {
        if (IsNdaSigned)
            throw new InvalidOperationException("توافق‌نامه‌ی عدم افشا قبلاً امضا شده است.");

        IsNdaSigned = true;
        NdaSignedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>ثبت واریز کارفرما؛ با تکمیل مبلغ، قرارداد fully-paid می‌شود.</summary>
    public void RegisterDeposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ واریز باید بزرگ‌تر از صفر باشد.");

        if (IsFullyPaid)
            throw new InvalidOperationException("قرارداد قبلاً کامل تسویه شده است.");

        DepositPaid += amount;

        if (DepositPaid >= TotalAmount)
            IsFullyPaid = true;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>تنظیم منتور قرارداد. فقط ادمین می‌تواند منتور را تغییر دهد.</summary>
    public void SetMentor(int? mentorId)
    {
        MentorId = mentorId;
        SetUpdatedAt();
    }
}
