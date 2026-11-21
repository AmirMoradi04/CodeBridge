using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// پروژه‌ی ثبت‌شده توسط کارفرما؛ پس از تایید ادمین به برنامه‌نویس تخصیص می‌یابد.
/// </summary>
public class Project : BaseEntity
{
    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Budget { get; private set; }

    public int EmployerId { get; private set; }

    public int? AssignedDeveloperId { get; private set; }

    public DeveloperLevelType RequiredLevel { get; private set; }

    public ProjectStatus Status { get; private set; } = ProjectStatus.Pending;

    public string? AdminNote { get; private set; }

    public int? ApprovedByAdminId { get; private set; }

    public DateTime? ApprovedAt { get; private set; }

    /// <summary>انتشار عمومی پروژه پس از تایید ادمین.</summary>
    public bool IsPublic { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    /// <summary>تایید نهایی پروژه توسط کارفرما پس از تسویه کامل.</summary>
    public DateTime? FinalApprovedAt { get; private set; }

    public bool IsFinalApproved { get; private set; }

    // --- Navigation Properties ---
    public User Employer { get; private set; } = null!;

    public User? AssignedDeveloper { get; private set; }

    public User? ApprovedByAdmin { get; private set; }

    public Contract? Contract { get; private set; }

    public ICollection<ProjectMember> Members { get; private set; } = new List<ProjectMember>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private Project()
    {
    }

    public Project(
        int employerId,
        string title,
        string description,
        decimal budget,
        DeveloperLevelType requiredLevel)
    {
        EmployerId = employerId;
        Title = title;
        Description = description;
        Budget = budget;
        RequiredLevel = requiredLevel;
        Status = ProjectStatus.Pending;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان پروژه الزامی است.");

        if (Budget <= 0)
            throw new InvalidOperationException("بودجه‌ی پروژه باید بزرگ‌تر از صفر باشد.");

        // زمان تایید نهایی نباید مغایر با پرچم تایید نهایی باشد
        if (FinalApprovedAt is not null && !IsFinalApproved)
            throw new InvalidOperationException("پرچم تایید نهایی با زمان آن هم‌خوانی ندارد.");
    }

    public void UpdateDetails(string title, string description, decimal budget, DeveloperLevelType requiredLevel)
    {
        Title = title.Trim();
        Description = description;
        Budget = budget;
        RequiredLevel = requiredLevel;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>تایید پروژه توسط ادمین.</summary>
    public void Approve(int adminId)
    {
        ApprovedByAdminId = adminId;
        ApprovedAt = DateTime.UtcNow;
        Status = ProjectStatus.Approved;
        SetUpdatedAt();
    }

    /// <summary>رد پروژه همراه با یادداشت ادمین.</summary>
    public void Reject(int adminId, string? note)
    {
        ApprovedByAdminId = adminId;
        AdminNote = note;
        Status = ProjectStatus.Rejected;
        SetUpdatedAt();
    }

    /// <summary>تخصیص برنامه‌نویس و شروع پروژه.</summary>
    public void AssignDeveloper(int developerId)
    {
        if (developerId <= 0)
            throw new InvalidOperationException("شناسه‌ی برنامه‌نویس معتبر نیست.");

        AssignedDeveloperId = developerId;
        Status = ProjectStatus.InProgress;
        SetUpdatedAt();
    }

    public void ChangeStatus(ProjectStatus status)
    {
        Status = status;
        SetUpdatedAt();
    }

    /// <summary>انتشار عمومی پروژه؛ فقط پروژه‌ی تاییدشده قابل انتشار است.</summary>
    public void Publish()
    {
        if (Status != ProjectStatus.Approved)
            throw new InvalidOperationException("فقط پروژه‌ی تاییدشده قابل انتشار است.");

        if (IsPublic)
            throw new InvalidOperationException("پروژه قبلاً منتشر شده است.");

        IsPublic = true;
        PublishedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>تایید نهایی توسط کارفرما پس از اطمینان از کیفیت کار.</summary>
    public void FinalApprove()
    {
        if (IsFinalApproved)
            throw new InvalidOperationException("پروژه قبلاً تأیید نهایی شده است.");

        IsFinalApproved = true;
        FinalApprovedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
