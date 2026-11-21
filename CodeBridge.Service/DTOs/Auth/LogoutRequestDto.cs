namespace CodeBridge.Service.DTOs.Auth;

public sealed class LogoutRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
