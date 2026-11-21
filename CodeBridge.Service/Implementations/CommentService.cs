using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Comment;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار دیدگاه‌ها.</summary>
public sealed class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IAuditLogService _auditLogService;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IAuditLogService auditLogService)
    {
        _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Result<CommentDto>> AddAsync(
        int authorId,
        CreateCommentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<CommentDto>.Failure("اطلاعات دیدگاه الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return Result<CommentDto>.Failure("متن دیدگاه الزامی است.");

        if (dto.PostId <= 0)
            return Result<CommentDto>.Failure("شناسه‌ی پست معتبر نیست.");

        var post = await _postRepository.GetByIdAsync(dto.PostId, cancellationToken);

        if (post is null)
            return Result<CommentDto>.Failure("پست مورد نظر پیدا نشد.");

        if (dto.ParentCommentId is int parentId)
        {
            var parent = await _commentRepository.GetByIdAsync(parentId, cancellationToken);

            if (parent is null)
                return Result<CommentDto>.Failure("دیدگاه والد پیدا نشد.");

            if (parent.PostId != post.Id)
                return Result<CommentDto>.Failure("دیدگاه والد به این پست تعلق ندارد.");
        }

        var comment = new Comment(post.Id, authorId, dto.Content, dto.ParentCommentId);

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            authorId,
            "CommentAdded",
            nameof(Comment),
            comment.Id,
            $"PostId={post.Id}",
            null,
            cancellationToken);

        return Result<CommentDto>.Success(CommentMapper.ToDto(comment));
    }

    public async Task<Result<IReadOnlyList<CommentDto>>> GetByPostAsync(
        int postId,
        CancellationToken cancellationToken = default)
    {
        if (postId <= 0)
            return Result<IReadOnlyList<CommentDto>>.Failure("شناسه‌ی پست معتبر نیست.");

        var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken);

        return Result<IReadOnlyList<CommentDto>>.Success(
            comments.Select(CommentMapper.ToDto).ToList());
    }

    public async Task<Result> DeleteAsync(
        int commentId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);

        if (comment is null)
            return Result.Failure("دیدگاه مورد نظر پیدا نشد.");

        // حذف فقط توسط نویسنده یا ادمین مجاز است
        if (!isAdmin && comment.AuthorId != userId)
            return Result.Failure("فقط نویسنده یا ادمین می‌تواند دیدگاه را حذف کند.");

        ((IAuditableEntity)comment).MarkAsDeleted();
        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "CommentDeleted",
            nameof(Comment),
            commentId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }
}
