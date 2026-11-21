using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.DeveloperLevel;

public sealed class DeveloperLevelDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public DeveloperLevelType Level { get; init; }

    public int Score { get; init; }

    public int ProjectsCompleted { get; init; }

    public decimal? AverageRating { get; init; }
}
