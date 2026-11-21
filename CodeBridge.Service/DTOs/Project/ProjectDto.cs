using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Project;

public sealed class ProjectDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Budget { get; init; }

    public int EmployerId { get; init; }

    public string? EmployerName { get; init; }

    public int? AssignedDeveloperId { get; init; }

    public string? AssignedDeveloperName { get; init; }

    public DeveloperLevelType RequiredLevel { get; init; }

    public ProjectStatus Status { get; init; }

    public string? AdminNote { get; init; }

    /// <summary>انتشار عمومی پروژه.</summary>
    public bool IsPublic { get; init; }

    public DateTime? PublishedAt { get; init; }

    public bool IsFinalApproved { get; init; }

    public DateTime? FinalApprovedAt { get; init; }

    public DateTime CreatedAt { get; init; }
}
