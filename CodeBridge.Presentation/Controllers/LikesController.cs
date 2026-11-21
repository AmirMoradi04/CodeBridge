using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>لایک پست‌ها؛ همه‌ی منطق در ILikeService است.</summary>
[Route("api/[controller]")]
public class LikesController : BaseApiController
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    /// <summary>لایک/برداشتن لایک برای کاربر جاری.</summary>
    [HttpPost("post/{postId:int}")]
    [Authorize]
    public async Task<IActionResult> Toggle(
        int postId,
        CancellationToken cancellationToken)
        => Handle(await _likeService.ToggleLikeAsync(CurrentUserId(), postId, cancellationToken));

    [HttpGet("post/{postId:int}/count")]
    [Authorize]
    public async Task<ActionResult<int>> Count(
        int postId,
        CancellationToken cancellationToken)
        => Handle(await _likeService.CountAsync(postId, cancellationToken));

    /// <summary>آیا کاربر جاری این پست را لایک کرده است.</summary>
    [HttpGet("post/{postId:int}/me")]
    [Authorize]
    public async Task<ActionResult<bool>> HasLiked(
        int postId,
        CancellationToken cancellationToken)
        => Handle(await _likeService.HasLikedAsync(CurrentUserId(), postId, cancellationToken));
}
