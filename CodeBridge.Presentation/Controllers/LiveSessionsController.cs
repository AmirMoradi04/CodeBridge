using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.LiveSession;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// جلسه‌های زنده‌ی «تماشای کار برنامه‌نویس ارشد».
/// برگزاری جلسه بیرون از سیستم است و فقط لینک جلسه/ضبط مدیریت می‌شود.
/// </summary>
[Route("api/live-sessions")]
[Authorize]
public class LiveSessionsController : BaseApiController
{
    private readonly ILiveSessionService _liveSessionService;

    public LiveSessionsController(ILiveSessionService liveSessionService)
    {
        _liveSessionService = liveSessionService;
    }

    /// <summary>ساخت جلسه توسط برنامه‌نویس ارشد برای پروژه‌ی خودش.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<LiveSessionDto>> Create(
        [FromBody] CreateLiveSessionDto dto,
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.CreateSessionAsync(
            CurrentUserId(),
            dto,
            cancellationToken));

    /// <summary>جلسه‌های ساخته‌شده توسط برنامه‌نویس جاری.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<IReadOnlyList<LiveSessionDto>>> GetMine(
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.GetSeniorSessionsAsync(
            CurrentUserId(),
            cancellationToken));

    /// <summary>جلسه‌های در دسترس کارجو؛ لینک فقط پس از پرداخت برمی‌گردد.</summary>
    [HttpGet("available")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<IReadOnlyList<LiveSessionDto>>> GetAvailable(
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.GetAvailableSessionsAsync(
            CurrentUserId(),
            cancellationToken));

    /// <summary>پایش همه‌ی جلسه‌ها برای ادمین.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<LiveSessionDto>>> GetAllForAdmin(
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.GetAdminSessionsAsync(cancellationToken));

    /// <summary>جزئیات جلسه؛ لینک برای کاربر پرداخت‌نکرده پنهان می‌ماند.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LiveSessionDto>> GetDetails(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.GetSessionDetailsAsync(
            id,
            CurrentUserId(),
            CurrentUserRole().ToString(),
            cancellationToken));

    /// <summary>ثبت‌نام و پرداخت کارجو؛ در پاسخ موفق لینک جلسه برگردانده می‌شود.</summary>
    [HttpPost("{id:int}/join")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> Join(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _liveSessionService.JoinSessionAsync(
            id,
            CurrentUserId(),
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(new
        {
            isSuccess = true,
            meetingLink = result.Data
        });
    }

    /// <summary>پایان جلسه و تسویه با برگزارکننده.</summary>
    [HttpPost("{id:int}/end")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> End(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.EndSessionAsync(
            id,
            CurrentUserId(),
            cancellationToken));

    /// <summary>ثبت لینک ضبط پس از پایان جلسه.</summary>
    [HttpPatch("{id:int}/recording")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> SetRecording(
        int id,
        [FromBody] UpdateRecordingLinkDto dto,
        CancellationToken cancellationToken)
        => Handle(await _liveSessionService.UpdateRecordingLinkAsync(
            id,
            CurrentUserId(),
            dto.RecordingLink,
            cancellationToken));
}
