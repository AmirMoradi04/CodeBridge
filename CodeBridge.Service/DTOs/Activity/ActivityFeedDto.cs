namespace CodeBridge.Service.DTOs.Activity;

/// <summary>یک رکورد از فید فعالیت زنده‌ی سایت.</summary>
public sealed class ActivityFeedDto
{
    /// <summary>یکی از مقادیر: user | project | review | deposit.</summary>
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime CreatedAt { get; init; }
}
