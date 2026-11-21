using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// درخواست/کار توسعه‌دهنده روی یک پروژه‌ی روزمره؛
/// شامل شروع (تایمر)، ارسال اثبات و بررسی توسط ادمین.
/// </summary>
public class DailyProjectApplication : BaseEntity
{
    public int DailyProjectId { get; private set; }

    public int DeveloperId { get; private set; }

    public DailyApplicationStatus Status { get; private set; } = DailyApplicationStatus.Pending;

    public DateTime? StartedAt { get; private set; }

    /// <summary>زمان انقضا = زمان شروع + مهلت تایمر.</summary>
    public DateTime? ExpiresAt { get; private set; }

    public DateTime? SubmittedAt { get; private set; }

    /// <summary>شناسه‌ی اسکرین‌شات به‌عنوان اثبات.</summary>
    public int? ProofAttachmentId { get; private set; }

    public int? ReviewedByAdminId { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public string? AdminNote { get; private set; }

    // --- Navigation Properties ---
    public DailyProject DailyProject { get; private set; } = null!;

    public User Developer { get; private set; } = null!;

    public Attachment? ProofAttachment { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private DailyProjectApplication()
    {
    }

    public DailyProjectApplication(int dailyProjectId, int developerId)
    {
        DailyProjectId = dailyProjectId;
        DeveloperId = developerId;
        Status = DailyApplicationStatus.Pending;

        Validate();
    }

    public override void Validate()
    {
        if (DailyProjectId <= 0)
            throw new InvalidOperationException("شناسه‌ی پروژه‌ی روزمره معتبر نیست.");

        if (DeveloperId <= 0)
            throw new InvalidOperationException("شناسه‌ی توسعه‌دهنده معتبر نیست.");
    }

    /// <summary>شروع کار و شروع تایمر.</summary>
    public void Start(int deadlineSeconds)
    {
        if (deadlineSeconds <= 0)
            throw new InvalidOperationException("مهلت تکمیل باید بزرگ‌تر از صفر باشد.");

        Status = DailyApplicationStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        ExpiresAt = StartedAt.Value.AddSeconds(deadlineSeconds);
        SetUpdatedAt();
    }

    /// <summary>ارسال اثبات (اسکرین‌شات) پس از اتمام کار.</summary>
    public void Submit(int proofAttachmentId)
    {
        if (proofAttachmentId <= 0)
            throw new InvalidOperationException("شناسه‌ی فایل اثبات معتبر نیست.");

        Status = DailyApplicationStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        ProofAttachmentId = proofAttachmentId;
        SetUpdatedAt();
    }

    public void Approve(int adminId)
    {
        ReviewedByAdminId = adminId;
        ReviewedAt = DateTime.UtcNow;
        Status = DailyApplicationStatus.Approved;
        SetUpdatedAt();
    }

    public void Reject(int adminId, string note)
    {
        ReviewedByAdminId = adminId;
        AdminNote = note;
        ReviewedAt = DateTime.UtcNow;
        Status = DailyApplicationStatus.Rejected;
        SetUpdatedAt();
    }

    public void Expire()
    {
        Status = DailyApplicationStatus.Expired;
        SetUpdatedAt();
    }

    public bool IsExpired() => Status == DailyApplicationStatus.InProgress && ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}
