namespace CodeBridge.Service.DTOs.Review;

/// <summary>ورودی ثبت امتیاز توسط کارفرمای پروژه.</summary>
public sealed class CreateReviewDto
{
    public int ProjectId { get; init; }

    /// <summary>برنامه‌نویس یا منتور دریافت‌کننده‌ی امتیاز.</summary>
    public int TargetUserId { get; init; }

    public int Rating { get; init; }

    public string? Comment { get; init; }
}
