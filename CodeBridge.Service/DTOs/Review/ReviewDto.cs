namespace CodeBridge.Service.DTOs.Review;

/// <summary>اطلاعات نمایشی یک امتیاز و نظر.</summary>
public sealed class ReviewDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public string? ProjectTitle { get; init; }

    public int ContractId { get; init; }

    public int ReviewerId { get; init; }

    public string? ReviewerName { get; init; }

    public int TargetUserId { get; init; }

    public string? TargetUserName { get; init; }

    public int Rating { get; init; }

    public string? Comment { get; init; }

    public DateTime CreatedAt { get; init; }
}
