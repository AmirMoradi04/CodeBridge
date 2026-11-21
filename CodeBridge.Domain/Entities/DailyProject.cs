using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// پروژه‌ی روزمره (میکروتسک) ثبت‌شده توسط کارفرما؛
/// شامل ظرفیت، تایمر، نیاز به اثبات (اسکرین‌شات) و پرداخت خودکار.
/// </summary>
public class DailyProject : BaseEntity
{
    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int EmployerId { get; private set; }

    /// <summary>کل بودجه‌ی تخصیص‌یافته توسط کارفرما.</summary>
    public decimal TotalBudget { get; private set; }

    /// <summary>پرداخت به‌ازای هر تسک تکمیل‌شده.</summary>
    public decimal PricePerUnit { get; private set; }

    /// <summary>ظرفیت (تعداد جایگاه).</summary>
    public int Capacity { get; private set; }

    public int FilledSlots { get; private set; }

    /// <summary>مهلت تکمیل پس از شروع (ثانیه).</summary>
    public int DeadlineSeconds { get; private set; } = 60;

    public DailyProjectStatus Status { get; private set; } = DailyProjectStatus.Pending;

    /// <summary>آیا اثبات (اسکرین‌شات) الزامی است.</summary>
    public bool ProofRequired { get; private set; } = true;

    public string? Instructions { get; private set; }

    public int? ApprovedByAdminId { get; private set; }

    public DateTime? ApprovedAt { get; private set; }

    public string? AdminNote { get; private set; }

    /// <summary>مبلغ واریزشده توسط کارفرما به حساب امانی.</summary>
    public decimal PaidAmount { get; private set; }

    /// <summary>آیا کل بودجه پرداخت شده است.</summary>
    public bool IsPaid { get; private set; }

    // --- Navigation Properties ---
    public User Employer { get; private set; } = null!;

    public ICollection<DailyProjectApplication> Applications { get; private set; } = new List<DailyProjectApplication>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private DailyProject()
    {
    }

    public DailyProject(
        int employerId,
        string title,
        string description,
        decimal totalBudget,
        decimal pricePerUnit,
        int capacity,
        int deadlineSeconds,
        string? instructions = null)
    {
        EmployerId = employerId;
        Title = title;
        Description = description;
        TotalBudget = totalBudget;
        PricePerUnit = pricePerUnit;
        Capacity = capacity;
        FilledSlots = 0;
        DeadlineSeconds = deadlineSeconds;
        Instructions = instructions;
        Status = DailyProjectStatus.Pending;
        ProofRequired = true;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان پروژه‌ی روزمره الزامی است.");

        if (TotalBudget <= 0)
            throw new InvalidOperationException("کل بودجه باید بزرگ‌تر از صفر باشد.");

        if (PricePerUnit <= 0)
            throw new InvalidOperationException("پرداخت هر تسک باید بزرگ‌تر از صفر باشد.");

        if (Capacity <= 0)
            throw new InvalidOperationException("ظرفیت باید بزرگ‌تر از صفر باشد.");

        if (DeadlineSeconds <= 0)
            throw new InvalidOperationException("مهلت تکمیل باید بزرگ‌تر از صفر باشد.");

        if (TotalBudget < PricePerUnit * Capacity)
            throw new InvalidOperationException("کل بودجه باید حداقل برابر پرداخت هر تسک ضرب‌در ظرفیت باشد.");

        if (PaidAmount < 0 || PaidAmount > TotalBudget)
            throw new InvalidOperationException("مبلغ پرداخت‌شده نامعتبر است.");
    }

    /// <summary>ثبت واریز بودجه توسط کارفرما. با تکمیل مبلغ، IsPaid = true می‌شود.</summary>
    public void RegisterPayment(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ واریز باید بزرگ‌تر از صفر باشد.");

        PaidAmount += amount;

        if (PaidAmount >= TotalBudget)
            IsPaid = true;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>تایید پروژه توسط ادمین.</summary>
    public void Approve(int adminId)
    {
        ApprovedByAdminId = adminId;
        ApprovedAt = DateTime.UtcNow;
        Status = DailyProjectStatus.Approved;
        SetUpdatedAt();
    }

    /// <summary>رد پروژه توسط ادمین همراه با یادداشت.</summary>
    public void Reject(int adminId, string note)
    {
        ApprovedByAdminId = adminId;
        AdminNote = note;
        Status = DailyProjectStatus.Rejected;
        SetUpdatedAt();
    }

    /// <summary>فعال‌سازی پس از تایید و پرداخت کامل. توسعه‌دهندگان فقط می‌توانند درخواست دهند.</summary>
    public void Activate()
    {
        if (!IsPaid)
            throw new InvalidOperationException("پروژه باید کاملاً پرداخت شده باشد.");

        Status = DailyProjectStatus.Active;
        SetUpdatedAt();
    }

    public void MarkFull()
    {
        Status = DailyProjectStatus.Full;
        SetUpdatedAt();
    }

    public void Complete()
    {
        Status = DailyProjectStatus.Completed;
        SetUpdatedAt();
    }

    public bool HasCapacity() => FilledSlots < Capacity;

    public void IncrementFilledSlots()
    {
        FilledSlots += 1;
        SetUpdatedAt();

        if (FilledSlots >= Capacity)
            MarkFull();
    }
}
