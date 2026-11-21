using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.DeveloperLevel;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>سطح و امتیاز برنامه‌نویسان؛ همه‌ی منطق در IDeveloperLevelService است.</summary>
[Route("api/[controller]")]
[Authorize]
public class DeveloperLevelsController : BaseApiController
{
    private readonly IDeveloperLevelService _developerLevelService;

    public DeveloperLevelsController(IDeveloperLevelService developerLevelService)
    {
        _developerLevelService = developerLevelService;
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<DeveloperLevelDto>> GetByUser(
        int userId,
        CancellationToken cancellationToken)
        => Handle(await _developerLevelService.GetByUserIdAsync(userId, cancellationToken));

    [HttpGet("level/{level}")]
    public async Task<ActionResult<IReadOnlyList<DeveloperLevelDto>>> GetByLevel(
        DeveloperLevelType level,
        CancellationToken cancellationToken)
        => Handle(await _developerLevelService.GetByLevelAsync(level, cancellationToken));

    [HttpGet("top")]
    public async Task<ActionResult<IReadOnlyList<DeveloperLevelDto>>> GetTop(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
        => Handle(await _developerLevelService.GetTopAsync(count, cancellationToken));

    /// <summary>تغییر امتیاز؛ فقط ادمین.</summary>
    [HttpPatch("user/{userId:int}/score")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateScore(
        int userId,
        [FromQuery] int scoreDelta,
        CancellationToken cancellationToken)
        => Handle(await _developerLevelService.UpdateScoreAsync(userId, scoreDelta, cancellationToken));
}
