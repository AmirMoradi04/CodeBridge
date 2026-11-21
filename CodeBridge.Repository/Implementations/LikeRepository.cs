using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری لایک‌ها؛ فقط دسترسی به داده.</summary>
public sealed class LikeRepository : GenericRepository<Like>, ILikeRepository
{
    public LikeRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Like?> GetByUserAndPostAsync(
        int userId,
        int postId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(like => like.UserId == userId && like.PostId == postId, cancellationToken);

    public Task<bool> HasUserLikedAsync(
        int userId,
        int postId,
        CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(like => like.UserId == userId && like.PostId == postId, cancellationToken);

    public Task<int> CountByPostIdAsync(int postId, CancellationToken cancellationToken = default)
        => DbSet.CountAsync(like => like.PostId == postId, cancellationToken);

    /// <summary>حذف لایک کاربر روی پست.</summary>
    public async Task RemoveByUserAndPostAsync(
        int userId,
        int postId,
        CancellationToken cancellationToken = default)
    {
        var like = await DbSet
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId, cancellationToken);

        if (like is null)
            return;

        Remove(like);
        await SaveChangesAsync(cancellationToken);
    }
}
