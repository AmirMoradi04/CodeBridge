using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Comment;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد مدیریت دیدگاه‌ها.</summary>
public interface ICommentService
{
    Task<Result<CommentDto>> AddAsync(
        int authorId,
        CreateCommentDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CommentDto>>> GetByPostAsync(
        int postId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        int commentId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
