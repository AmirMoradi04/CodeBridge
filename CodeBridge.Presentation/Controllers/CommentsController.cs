using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Comment;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>مدیریت دیدگاه‌ها؛ همه‌ی منطق در ICommentService است.</summary>
[Route("api/[controller]")]
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentDto>> Add(
        [FromBody] CreateCommentDto dto,
        CancellationToken cancellationToken)
        => Handle(await _commentService.AddAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>دیدگاه‌های یک پست.</summary>
    [HttpGet("post/{postId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetByPost(
        int postId,
        CancellationToken cancellationToken)
        => Handle(await _commentService.GetByPostAsync(postId, cancellationToken));

    /// <summary>حذف دیدگاه؛ نویسنده یا ادمین.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _commentService.DeleteAsync(
            id,
            CurrentUserId(),
            CurrentUserRole() == UserRole.Admin,
            cancellationToken));
}
