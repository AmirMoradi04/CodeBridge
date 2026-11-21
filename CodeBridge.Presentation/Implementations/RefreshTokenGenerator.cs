using System.Security.Cryptography;
using CodeBridge.Service.Common;
using CodeBridge.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace CodeBridge.Presentation.Implementations;

/// <summary>تولید توکن تازه‌سازی تصادفی و محاسبه‌ی زمان انقضای آن.</summary>
public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 64;

    private readonly JwtSettings _settings;

    public RefreshTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }

    public DateTime GetExpiryDate()
        => DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
}
