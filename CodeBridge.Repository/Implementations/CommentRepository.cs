using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری دیدگاه‌ها؛ فقط دسترسی به داده.</summary>
public sealed class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(
        int postId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Comment>> GetRepliesAsync(
        int parentCommentId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(comment => comment.ParentCommentId == parentCommentId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountByPostIdAsync(int postId, CancellationToken cancellationToken = default)
        => DbSet.CountAsync(comment => comment.PostId == postId, cancellationToken);
}
