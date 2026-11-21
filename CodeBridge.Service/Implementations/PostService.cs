using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Post;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار پست‌ها: ساخت، انتشار و حذف.</summary>
public sealed class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Result<PostDto>> CreateAsync(
        int authorId,
        CreatePostDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<PostDto>.Failure("اطلاعات پست الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return Result<PostDto>.Failure("متن پست الزامی است.");

        var author = await _userRepository.GetByIdAsync(authorId, cancellationToken);

        if (author is null)
            return Result<PostDto>.Failure("نویسنده‌ی مورد نظر پیدا نشد.");

        // فقط کارجو یا کارفرما مجاز به ساخت پست است
        if (author.UserRole is not (UserRole.JobSeeker or UserRole.Employer))
            return Result<PostDto>.Failure("فقط کارجو یا کارفرما می‌تواند پست بسازد.");

        var post = new Post(author.Id, dto.Content, dto.AttachmentId);

        await _postRepository.AddAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            authorId,
            "PostCreated",
            nameof(Post),
            post.Id,
            null,
            null,
            cancellationToken);

        return Result<PostDto>.Success(PostMapper.ToDto(post));
    }

    public async Task<Result<PagedResultDto<PostDto>>> GetFeedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var posts = await _postRepository.GetPublishedAsync(safePage, safePageSize, cancellationToken);
        var totalCount = await _postRepository.CountPublishedAsync(cancellationToken);

        return Result<PagedResultDto<PostDto>>.Success(new PagedResultDto<PostDto>
        {
            Items = posts.Select(PostMapper.ToDto).ToList(),
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)safePageSize)
        });
    }

    public async Task<Result<IReadOnlyList<PostDto>>> GetByAuthorAsync(
        int authorId,
        CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetByAuthorIdAsync(authorId, cancellationToken);

        return Result<IReadOnlyList<PostDto>>.Success(posts.Select(PostMapper.ToDto).ToList());
    }

    public async Task<Result> PublishAsync(
        int postId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

        if (post is null)
            return Result.Failure("پست مورد نظر پیدا نشد.");

        if (post.AuthorId != userId)
            return Result.Failure("فقط نویسنده‌ی پست می‌تواند آن را منتشر کند.");

        if (post.IsPublished)
            return Result.Failure("این پست قبلاً منتشر شده است.");

        post.Publish();
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "PostPublished",
            nameof(Post),
            postId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        int postId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

        if (post is null)
            return Result.Failure("پست مورد نظر پیدا نشد.");

        // حذف فقط توسط نویسنده یا ادمین مجاز است
        if (!isAdmin && post.AuthorId != userId)
            return Result.Failure("فقط نویسنده یا ادمین می‌تواند پست را حذف کند.");

        ((IAuditableEntity)post).MarkAsDeleted();
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "PostDeleted",
            nameof(Post),
            postId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }
}
