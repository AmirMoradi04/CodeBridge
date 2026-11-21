using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>عضویت یک کاربر در تیم پروژه؛ توسط ادمین اضافه می‌شود.</summary>
public class ProjectMember : BaseEntity
{
    public int ProjectId { get; private set; }

    public int UserId { get; private set; }

    /// <summary>ادمینی که عضو را به پروژه افزود.</summary>
    public int AddedByAdminId { get; private set; }

    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    // --- Navigation Properties ---
    public Project Project { get; private set; } = null!;

    public User User { get; private set; } = null!;

    public User AddedByAdmin { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private ProjectMember()
    {
    }

    public ProjectMember(int projectId, int userId, int addedByAdminId)
    {
        ProjectId = projectId;
        UserId = userId;
        AddedByAdminId = addedByAdminId;
        JoinedAt = DateTime.UtcNow;

        Validate();
    }

    public override void Validate()
    {
        if (ProjectId <= 0)
            throw new InvalidOperationException("شناسه‌ی پروژه معتبر نیست.");

        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر معتبر نیست.");
    }
}