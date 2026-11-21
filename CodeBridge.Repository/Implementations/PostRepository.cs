using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری پست‌ها؛ فقط دسترسی به داده.</summary>
public sealed class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Post?> GetWithCommentsAndLikesAsync(
        int postId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(post => post.Author)
            .Include(post => post.Attachment)
            .Include(post => post.Comments)
                .ThenInclude(comment => comment.Author)
            .Include(post => post.Likes)
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken);

    public async Task<IReadOnlyList<Post>> GetPublishedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        return await DbSet
            .AsNoTracking()
            .Include(post => post.Author)
            .Include(post => post.Likes)
            .Include(post => post.Comments)
            .Where(post => post.IsPublished && !post.IsDeleted)
            .OrderByDescending(post => post.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByAuthorIdAsync(
        int authorId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(post => post.Author)
            .Include(post => post.Likes)
            .Include(post => post.Comments)
            .Where(post => post.AuthorId == authorId && !post.IsDeleted)
            .OrderByDescending(post => post.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountPublishedAsync(CancellationToken cancellationToken = default)
        => DbSet.CountAsync(post => post.IsPublished, cancellationToken);
}
