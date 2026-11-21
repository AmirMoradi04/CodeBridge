namespace CodeBridge.Service.Common;

/// <summary>تنظیمات صدور توکن دسترسی و تازه‌سازی.</summary>
public sealed class JwtSettings
{
    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; init; } = 15;

    public int RefreshTokenExpirationDays { get; init; } = 7;
}
