namespace CodeBridge.Service.DTOs.Auth;

public sealed class RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
