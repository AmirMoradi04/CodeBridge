using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای لایک‌ها.</summary>
public interface ILikeRepository : IGenericRepository<Like>
{
    Task<Like?> GetByUserAndPostAsync(int userId, int postId, CancellationToken cancellationToken = default);

    Task<bool> HasUserLikedAsync(int userId, int postId, CancellationToken cancellationToken = default);

    Task<int> CountByPostIdAsync(int postId, CancellationToken cancellationToken = default);

    Task RemoveByUserAndPostAsync(int userId, int postId, CancellationToken cancellationToken = default);
}
