using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Project;

public sealed class CreateProjectDto
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Budget { get; init; }

    public DeveloperLevelType RequiredLevel { get; init; }
}
