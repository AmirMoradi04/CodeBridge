using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Achievement;

/// <summary>اطلاعات نمایشی یک دستاورد کسب‌شده.</summary>
public sealed class AchievementDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? IconUrl { get; init; }

    public AchievementType Type { get; init; }

    /// <summary>زمان کسب دستاورد.</summary>
    public DateTime EarnedAt { get; init; }
}
