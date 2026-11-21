using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// جلسه‌ی زنده‌ی آموزش کار برنامه‌نویس ارشد برای کارجویان.
/// برگزاری جلسه بیرون از سیستم است و فقط لینک ورود ثبت می‌شود.
/// </summary>
public class LiveSession : BaseEntity
{
    public int ProjectId { get; private set; }

    /// <summary>برنامه‌نویس ارشد برگزارکننده‌ی جلسه.</summary>
    public int SeniorId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public DateTime ScheduledAt { get; private set; }

    public int DurationMinutes { get; private set; }

    /// <summary>لینک بیرونی جلسه (Google Meet، Bale، Skyroom و ...).</summary>
    public string MeetingLink { get; private set; } = string.Empty;

    /// <summary>لینک ضبط جلسه؛ فقط پس از پایان جلسه ثبت می‌شود.</summary>
    public string? RecordingLink { get; private set; }

    public LiveSessionStatus Status { get; private set; } = LiveSessionStatus.Scheduled;

    // --- Navigation Properties ---
    public Project Project { get; private set; } = null!;

    public User Senior { get; private set; } = null!;

    public ICollection<LiveSessionParticipant> Participants { get; private set; }
        = new List<LiveSessionParticipant>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private LiveSession()
    {
    }

    public LiveSession(
        int projectId,
        int seniorId,
        string title,
        string description,
        decimal price,
        DateTime scheduledAt,
        int durationMinutes,
        string meetingLink)
    {
        ProjectId = projectId;
        SeniorId = seniorId;
        Title = title?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        ScheduledAt = scheduledAt;
        DurationMinutes = durationMinutes;
        MeetingLink = meetingLink?.Trim() ?? string.Empty;
        Status = LiveSessionStatus.Scheduled;

        Validate();
    }

    public override void Validate()
    {
        if (ProjectId <= 0)
            throw new InvalidOperationException("شناسه‌ی پروژه معتبر نیست.");

        if (SeniorId <= 0)
            throw new InvalidOperationException("شناسه‌ی برگزارکننده معتبر نیست.");

        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان جلسه الزامی است.");

        if (Price < 0)
            throw new InvalidOperationException("قیمت جلسه نمی‌تواند منفی باشد.");

        if (DurationMinutes <= 0)
            throw new InvalidOperationException("مدت جلسه باید بزرگ‌تر از صفر باشد.");

        if (string.IsNullOrWhiteSpace(MeetingLink))
            throw new InvalidOperationException("لینک جلسه الزامی است.");
    }

    /// <summary>شروع جلسه‌ی زنده.</summary>
    public void Start()
    {
        if (Status is LiveSessionStatus.Ended or LiveSessionStatus.Cancelled)
            throw new InvalidOperationException("جلسه‌ی پایان‌یافته یا لغوشده قابل شروع نیست.");

        Status = LiveSessionStatus.Live;
        SetUpdatedAt();
    }

    /// <summary>پایان جلسه؛ پس از این وضعیت فقط لینک ضبط قابل ثبت است.</summary>
    public void End()
    {
        if (Status == LiveSessionStatus.Cancelled)
            throw new InvalidOperationException("جلسه‌ی لغوشده قابل پایان‌دادن نیست.");

        if (Status == LiveSessionStatus.Ended)
            throw new InvalidOperationException("جلسه قبلاً پایان یافته است.");

        Status = LiveSessionStatus.Ended;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == LiveSessionStatus.Ended)
            throw new InvalidOperationException("جلسه‌ی پایان‌یافته قابل لغو نیست.");

        Status = LiveSessionStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>ثبت لینک ضبط؛ فقط بعد از پایان جلسه مجاز است.</summary>
    public void SetRecordingLink(string recordingLink)
    {
        if (Status != LiveSessionStatus.Ended)
            throw new InvalidOperationException("لینک ضبط فقط بعد از پایان جلسه ثبت می‌شود.");

        if (string.IsNullOrWhiteSpace(recordingLink))
            throw new InvalidOperationException("لینک ضبط الزامی است.");

        RecordingLink = recordingLink.Trim();
        SetUpdatedAt();
    }
}
