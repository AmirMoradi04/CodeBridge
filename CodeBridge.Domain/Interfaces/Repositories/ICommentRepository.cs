using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای دیدگاه‌ها.</summary>
public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Comment>> GetRepliesAsync(
        int parentCommentId,
        CancellationToken cancellationToken = default);

    Task<int> CountByPostIdAsync(int postId, CancellationToken cancellationToken = default);
}
