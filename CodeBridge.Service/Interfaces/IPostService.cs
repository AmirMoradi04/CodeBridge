using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Post;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ساخت، انتشار و حذف پست‌ها.</summary>
public interface IPostService
{
    Task<Result<PostDto>> CreateAsync(
        int authorId,
        CreatePostDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResultDto<PostDto>>> GetFeedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PostDto>>> GetByAuthorAsync(
        int authorId,
        CancellationToken cancellationToken = default);

    Task<Result> PublishAsync(int postId, int userId, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        int postId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
