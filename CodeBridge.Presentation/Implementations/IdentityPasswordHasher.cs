using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CodeBridge.Presentation.Implementations;

/// <summary>پیاده‌سازی هش رمز عبور با PasswordHasher خود ASP.NET Core Identity.</summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(new User(), password);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        var result = _hasher.VerifyHashedPassword(new User(), hash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
