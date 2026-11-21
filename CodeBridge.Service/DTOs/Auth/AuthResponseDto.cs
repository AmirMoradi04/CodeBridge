using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; init; }

    public DateTime RefreshTokenExpiresAt { get; init; }

    public int UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public UserRole Role { get; init; }
}
