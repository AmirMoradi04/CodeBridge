using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Project;

/// <summary>عضویت یک کاربر در تیم پروژه.</summary>
public sealed class ProjectMemberDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public int UserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string UserEmail { get; init; } = string.Empty;

    public UserRole UserRole { get; init; }

    public bool IsApproved { get; init; }

    public DateTime JoinedAt { get; init; }
}