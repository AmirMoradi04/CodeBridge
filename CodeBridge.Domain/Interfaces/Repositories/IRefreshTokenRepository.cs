using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای توکن‌های تازه‌سازی.</summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(
        int userId,
        string? ip = null,
        CancellationToken cancellationToken = default);

    Task RemoveExpiredAsync(CancellationToken cancellationToken = default);
}
