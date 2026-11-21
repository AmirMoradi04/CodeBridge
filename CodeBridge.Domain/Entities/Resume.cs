using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// رزومه‌ی کارجو؛ هر کاربر یک رزومه دارد و فایل آن از طریق Attachment مدیریت می‌شود.
/// </summary>
public class Resume : BaseEntity
{
    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Skills { get; private set; } = string.Empty;

    public int? AttachmentId { get; private set; }

    public ResumeStatus Status { get; private set; } = ResumeStatus.Pending;

    public int? ReviewedByAdminId { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    // --- Navigation Properties ---
    public User User { get; private set; } = null!;

    public Attachment? Attachment { get; private set; }

    public User? ReviewedByAdmin { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private Resume()
    {
    }

    public Resume(int userId, string title, string summary, string skills, int? attachmentId = null)
    {
        UserId = userId;
        Title = title;
        Summary = summary;
        Skills = skills;
        AttachmentId = attachmentId;
        Status = ResumeStatus.Pending;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان رزومه الزامی است.");

        if (string.IsNullOrWhiteSpace(Summary))
            throw new InvalidOperationException("خلاصه‌ی رزومه الزامی است.");
    }

    public void UpdateDetails(string title, string summary, string skills, int? attachmentId)
    {
        Title = title.Trim();
        Summary = summary;
        Skills = skills;
        AttachmentId = attachmentId;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>تایید رزومه توسط ادمین.</summary>
    public void Approve(int adminId) => Review(ResumeStatus.Approved, adminId);

    /// <summary>رد رزومه توسط ادمین.</summary>
    public void Reject(int adminId) => Review(ResumeStatus.Rejected, adminId);

    private void Review(ResumeStatus status, int adminId)
    {
        Status = status;
        ReviewedByAdminId = adminId;
        ReviewedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
