using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Service.Interfaces;

/// <summary>انتزاع تولید توکن دسترسی.</summary>
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
}
