using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.DeveloperLevel;
using CodeBridge.Service.DTOs.Resume;

namespace CodeBridge.Service.DTOs.User;

public sealed class UserProfileDto
{
    public int Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public UserRole UserRole { get; init; }

    public bool IsApproved { get; init; }

    public bool IsActive { get; init; }

    public string? Bio { get; init; }

    public string? ProfilePictureUrl { get; init; }

    public string? CompanyLogoUrl { get; init; }

    public DateTime CreatedAt { get; init; }

    public ResumeDto? Resume { get; init; }

    public DeveloperLevelDto? DeveloperLevel { get; init; }
}
