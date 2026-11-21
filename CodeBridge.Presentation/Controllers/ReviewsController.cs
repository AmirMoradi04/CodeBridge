using CodeBridge.Service.DTOs.Review;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>امتیاز و نظرات پروژه‌ها؛ ثبت فقط در اختیار کارفرماست.</summary>
[Route("api/[controller]")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>ثبت امتیاز برای برنامه‌نویس یا منتور پروژه‌ی تکمیل‌شده.</summary>
    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Submit(
        [FromBody] CreateReviewDto dto,
        CancellationToken cancellationToken)
        => Handle(await _reviewService.SubmitReviewAsync(
            CurrentUserId(),
            dto,
            cancellationToken));

    /// <summary>امتیازهای دریافت‌شده‌ی یک کاربر؛ برای نمایش پروفایل.</summary>
    [HttpGet("user/{targetUserId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetUserReviews(
        int targetUserId,
        CancellationToken cancellationToken)
        => Handle(await _reviewService.GetUserReviewsAsync(targetUserId, cancellationToken));

    /// <summary>امتیازهای ثبت‌شده برای یک پروژه.</summary>
    [HttpGet("project/{projectId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetProjectReviews(
        int projectId,
        CancellationToken cancellationToken)
        => Handle(await _reviewService.GetProjectReviewsAsync(projectId, cancellationToken));

    /// <summary>فهرست کامل امتیازها برای پنل ادمین.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetAll(
        CancellationToken cancellationToken)
        => Handle(await _reviewService.GetAllReviewsAsync(cancellationToken));
}
