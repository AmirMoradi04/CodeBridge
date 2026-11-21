using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Resume;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// مدیریت رزومه‌ها؛ همه‌ی منطق در IResumeService است.
/// شناسه‌ی بازبین از توکن خوانده می‌شود و ویرایش رزومه فقط برای صاحب آن یا ادمین است.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class ResumesController : BaseApiController
{
    private readonly IResumeService _resumeService;

    public ResumesController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<ResumeDto>> CreateOrUpdate(
        int userId,
        [FromBody] CreateResumeDto dto,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(userId))
            return Forbid();

        return Handle(await _resumeService.CreateOrUpdateAsync(userId, dto, cancellationToken));
    }

    /// <summary>رزومه‌ی یک کاربر؛ برای صاحب رزومه، ادمین و کارفرما.</summary>
    [HttpGet("user/{userId:int}")]
    [Authorize(Roles = "Admin,Employer,Developer,JobSeeker")]
    public async Task<ActionResult<ResumeDto>> GetByUser(
        int userId,
        CancellationToken cancellationToken)
    {
        var role = CurrentUserRole();

        if (userId != CurrentUserId() && role is not (UserRole.Admin or UserRole.Employer))
            return Forbid();

        return Handle(await _resumeService.GetByUserIdAsync(userId, cancellationToken));
    }

    [HttpGet("pending")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<ResumeDto>>> GetPending(
        CancellationToken cancellationToken)
        => Handle(await _resumeService.GetPendingAsync(cancellationToken));

    /// <summary>فهرست رزومه‌های تاییدشده؛ ادمین و برنامه‌نویسان (کارجویان).</summary>
    [HttpGet("approved")]
    [Authorize(Roles = "Admin,JobSeeker")]
    public async Task<ActionResult<IReadOnlyList<ResumeDto>>> GetApproved(
        CancellationToken cancellationToken)
        => Handle(await _resumeService.GetApprovedAsync(cancellationToken));

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _resumeService.ApproveAsync(id, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Reject(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _resumeService.RejectAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>ویرایش رزومه فقط برای صاحب آن یا ادمین.</summary>
    private bool CanAccess(int userId)
        => userId == CurrentUserId() || CurrentUserRole() == UserRole.Admin;
}
