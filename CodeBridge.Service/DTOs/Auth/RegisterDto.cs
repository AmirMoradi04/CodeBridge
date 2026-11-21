using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Auth;

public sealed class RegisterDto
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public UserRole UserRole { get; init; } = UserRole.JobSeeker;

    /// <summary>کد معرف دعوت‌کننده (اختیاری).</summary>
    public string? ReferralCode { get; init; }
}
