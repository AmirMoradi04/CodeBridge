using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار لایک پست‌ها.</summary>
public sealed class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;

    public LikeService(ILikeRepository likeRepository, IPostRepository postRepository)
    {
        _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
    }

    public async Task<Result> ToggleLikeAsync(
        int userId,
        int postId,
        CancellationToken cancellationToken = default)
    {
        if (postId <= 0)
            return Result.Failure("شناسه‌ی پست معتبر نیست.");

        if (userId <= 0)
            return Result.Failure("شناسه‌ی کاربر معتبر نیست.");

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

        if (post is null)
            return Result.Failure("پست مورد نظر پیدا نشد.");

        // اگر کاربر قبلاً لایک کرده باشد، لایک حذف می‌شود؛ در غیر این صورت ثبت می‌شود
        if (await _likeRepository.HasUserLikedAsync(userId, postId, cancellationToken))
        {
            await _likeRepository.RemoveByUserAndPostAsync(userId, postId, cancellationToken);

            return Result.Success();
        }

        await _likeRepository.AddAsync(new Like(postId, userId), cancellationToken);
        await _likeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<bool>> HasLikedAsync(
        int userId,
        int postId,
        CancellationToken cancellationToken = default)
    {
        if (postId <= 0 || userId <= 0)
            return Result<bool>.Failure("شناسه‌ی کاربر یا پست معتبر نیست.");

        var hasLiked = await _likeRepository.HasUserLikedAsync(userId, postId, cancellationToken);

        return Result<bool>.Success(hasLiked);
    }

    public async Task<Result<int>> CountAsync(
        int postId,
        CancellationToken cancellationToken = default)
    {
        if (postId <= 0)
            return Result<int>.Failure("شناسه‌ی پست معتبر نیست.");

        var count = await _likeRepository.CountByPostIdAsync(postId, cancellationToken);

        return Result<int>.Success(count);
    }
}
