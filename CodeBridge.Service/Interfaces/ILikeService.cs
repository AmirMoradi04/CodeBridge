using CodeBridge.Service.Common;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد مدیریت لایک پست‌ها.</summary>
public interface ILikeService
{
    Task<Result> ToggleLikeAsync(int userId, int postId, CancellationToken cancellationToken = default);

    Task<Result<bool>> HasLikedAsync(int userId, int postId, CancellationToken cancellationToken = default);

    Task<Result<int>> CountAsync(int postId, CancellationToken cancellationToken = default);
}
