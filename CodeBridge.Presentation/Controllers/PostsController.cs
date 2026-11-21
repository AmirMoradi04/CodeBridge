using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Post;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// مدیریت پست‌ها؛ همه‌ی منطق در IPostService است.
/// نویسنده/حذف‌کننده از توکن خوانده می‌شود، نه از پارامتر ورودی.
/// </summary>
[Route("api/[controller]")]
public class PostsController : BaseApiController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> Create(
        [FromBody] CreatePostDto dto,
        CancellationToken cancellationToken)
        => Handle(await _postService.CreateAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>فید پست‌های منتشرشده.</summary>
    [HttpGet("feed")]
    [Authorize]
    public async Task<ActionResult<PagedResultDto<PostDto>>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Handle(await _postService.GetFeedAsync(page, pageSize, cancellationToken));

    [HttpGet("author/{authorId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PostDto>>> GetByAuthor(
        int authorId,
        CancellationToken cancellationToken)
        => Handle(await _postService.GetByAuthorAsync(authorId, cancellationToken));

    [HttpPatch("{id:int}/publish")]
    [Authorize]
    public async Task<IActionResult> Publish(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _postService.PublishAsync(id, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _postService.DeleteAsync(
            id,
            CurrentUserId(),
            CurrentUserRole() == UserRole.Admin,
            cancellationToken));
}
