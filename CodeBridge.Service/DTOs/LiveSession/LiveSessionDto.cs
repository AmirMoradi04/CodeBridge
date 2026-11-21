using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.LiveSession;

public sealed class LiveSessionDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public string? ProjectTitle { get; init; }

    public int SeniorId { get; init; }

    public string? SeniorName { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public DateTime ScheduledAt { get; init; }

    public int DurationMinutes { get; init; }

    /// <summary>فقط برای برگزارکننده، ادمین و کارجوی پرداخت‌کننده پر می‌شود.</summary>
    public string? MeetingLink { get; init; }

    /// <summary>لینک ضبط؛ فقط برای برگزارکننده، ادمین و شرکت‌کننده‌ها.</summary>
    public string? RecordingLink { get; init; }

    public LiveSessionStatus Status { get; init; }

    public int ParticipantCount { get; init; }

    /// <summary>درآمد ناخالص جلسه (قیمت × تعداد شرکت‌کننده).</summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>آیا کاربر جاری در این جلسه ثبت‌نام کرده است.</summary>
    public bool IsJoined { get; init; }

    public DateTime CreatedAt { get; init; }
}
