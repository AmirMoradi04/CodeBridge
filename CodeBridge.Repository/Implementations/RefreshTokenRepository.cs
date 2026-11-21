using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری توکن‌های تازه‌سازی؛ فقط دسترسی به داده.</summary>
public sealed class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(refreshToken => refreshToken.Token == token, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Where(refreshToken => refreshToken.UserId == userId
                && refreshToken.RevokedAt == null
                && refreshToken.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        int userId,
        string? ip = null,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveByUserIdAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(ip);
            Update(token);
        }

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>حذف فیزیکی توکن‌های منقضی‌شده.</summary>
    public Task RemoveExpiredAsync(CancellationToken cancellationToken = default)
        => DbSet
            .Where(refreshToken => refreshToken.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
}
