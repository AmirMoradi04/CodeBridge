using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای پست‌ها.</summary>
public interface IPostRepository : IGenericRepository<Post>
{
    Task<Post?> GetWithCommentsAndLikesAsync(int postId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Post>> GetPublishedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Post>> GetByAuthorIdAsync(int authorId, CancellationToken cancellationToken = default);

    Task<int> CountPublishedAsync(CancellationToken cancellationToken = default);
}
