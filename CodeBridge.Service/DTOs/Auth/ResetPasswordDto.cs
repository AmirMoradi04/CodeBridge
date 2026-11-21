namespace CodeBridge.Service.DTOs.Auth;

public sealed class ResetPasswordDto
{
    public string Email { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}
