using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.User;

public sealed class UserDto
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

    /// <summary>سطح برنامه‌نویسی (فقط برای کارجویان پر می‌شود).</summary>
    public DeveloperLevelType? Level { get; init; }

    /// <summary>امتیاز برنامه‌نویسی (فقط برای کارجویان پر می‌شود).</summary>
    public int? Score { get; init; }

    public DateTime CreatedAt { get; init; }
}
