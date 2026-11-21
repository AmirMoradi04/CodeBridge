using CodeBridge.Service.DTOs.Achievement;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>دستاوردها و نشان‌های کاربران.</summary>
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : BaseApiController
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>دستاوردهای کسب‌شده‌ی یک کاربر.</summary>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IReadOnlyList<AchievementDto>>> GetUserAchievements(
        int userId,
        CancellationToken cancellationToken)
        => Handle(await _achievementService.GetUserAchievementsAsync(userId, cancellationToken));
}
