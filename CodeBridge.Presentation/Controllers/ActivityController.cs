using CodeBridge.Service.DTOs.Activity;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>فید فعالیت زنده‌ی صفحه‌ی اصلی.</summary>
[Route("api/[controller]")]
[Authorize]
public class ActivityController : BaseApiController
{
    private readonly IActivityFeedService _activityFeedService;

    public ActivityController(IActivityFeedService activityFeedService)
    {
        _activityFeedService = activityFeedService;
    }

    /// <summary>تازه‌ترین رویدادهای سیستم.</summary>
    [HttpGet("feed")]
    public async Task<ActionResult<IReadOnlyList<ActivityFeedDto>>> GetFeed(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
        => Handle(await _activityFeedService.GetRecentActivitiesAsync(count, cancellationToken));
}
