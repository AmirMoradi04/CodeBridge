namespace CodeBridge.Service.DTOs.LiveSession;

public sealed class CreateLiveSessionDto
{
    public int ProjectId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public DateTime ScheduledAt { get; init; }

    public int DurationMinutes { get; init; }

    /// <summary>لینک بیرونی جلسه (Google Meet، Bale، Skyroom و ...).</summary>
    public string MeetingLink { get; init; } = string.Empty;
}
