using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.DailyProject;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>مدیریت پروژه‌های روزمره (میکروتسک).</summary>
[Route("api/dailyprojects")]
[Authorize]
public class DailyProjectsController : BaseApiController
{
    private readonly IDailyProjectService _dailyProjectService;

    public DailyProjectsController(IDailyProjectService dailyProjectService)
    {
        _dailyProjectService = dailyProjectService;
    }

    // --- ثبت و مشاهده پروژه ---
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<ActionResult<DailyProjectDto>> Create(
        [FromBody] CreateDailyProjectDto dto,
        CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.CreateAsync(CurrentUserId(), dto, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<DailyProjectDto>> GetById(int id, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetByIdAsync(id, cancellationToken));

    /// <summary>پروژه‌های متعلق به کارفرمای جاری.</summary>
    [HttpGet("employer/me")]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<ActionResult<IReadOnlyList<DailyProjectDto>>> GetMyProjects(CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetByEmployerAsync(CurrentUserId(), cancellationToken));

    /// <summary>فهرست پروژه‌های فعال برای توسعه‌دهندگان.</summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,JobSeeker")]
    public async Task<ActionResult<IReadOnlyList<DailyProjectDto>>> GetActive(CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetActiveAsync(cancellationToken));

    /// <summary>فهرست پروژه‌های تاییدشده و آماده‌ی پرداخت/فعال‌سازی (ادمین).</summary>
    [HttpGet("approved")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<DailyProjectDto>>> GetApproved(CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetApprovedAsync(cancellationToken));

    /// <summary>فهرست پروژه‌های در انتظار تایید (ادمین).</summary>
    [HttpGet("pending")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<DailyProjectDto>>> GetPending(CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetPendingAsync(cancellationToken));

    // --- مدیریت توسط ادمین ---
    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.ApproveAsync(id, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Reject(
        int id,
        [FromQuery] string note,
        CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.RejectAsync(id, CurrentUserId(), note, cancellationToken));

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.ActivateAsync(id, CurrentUserId(), cancellationToken));

    // --- عملیات کارفرما ---
    /// <summary>واریز بودجه‌ی پروژه به حساب امانی. فقط کارفرمای پروژه مجاز است.</summary>
    [HttpPost("{id:int}/deposit-funds")]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<IActionResult> DepositFunds(int id, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.DepositFundsAsync(id, CurrentUserId(), cancellationToken));

    // --- عملیات توسعه‌دهنده ---
    /// <summary>درخواست انجام تسک (فقط JobSeeker؛ کارجویان برنامه‌نویسان پلتفرم هستند).</summary>
    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<DailyApplicationDto>> Apply(int id, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.ApplyAsync(CurrentUserId(), id, cancellationToken));

    /// <summary>شروع انجام تسک (تایمر شروع می‌شود).</summary>
    [HttpPost("start")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> StartTask([FromBody] StartTaskDto dto, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.StartTaskAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>ارسال اثبات (اسکرین‌شات).</summary>
    [HttpPost("submit")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> SubmitTask([FromBody] SubmitTaskDto dto, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.SubmitTaskAsync(CurrentUserId(), dto, cancellationToken));

    // --- بررسی ادمین روی تسک ---
    /// <summary>تایید تسک و پرداخت به توسعه‌دهنده.</summary>
    [HttpPatch("tasks/{applicationId:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ApproveTask(int applicationId, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.ApproveTaskAsync(applicationId, CurrentUserId(), cancellationToken));

    /// <summary>رد تسک.</summary>
    [HttpPatch("tasks/{applicationId:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> RejectTask(
        int applicationId,
        [FromQuery] string note,
        CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.RejectTaskAsync(applicationId, CurrentUserId(), note, cancellationToken));

    /// <summary>منقضی کردن تسک (Timeout).</summary>
    [HttpPatch("tasks/{applicationId:int}/expire")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ExpireTask(int applicationId, CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.ExpireTaskAsync(applicationId, CurrentUserId(), cancellationToken));

    // --- مشاهده درخواست‌ها ---
    /// <summary>درخواست‌های یک پروژه (فقط ادمین).</summary>
    [HttpGet("{projectId:int}/applications")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<DailyApplicationDto>>> GetByProject(
        int projectId,
        CancellationToken cancellationToken)
        => Handle(await _dailyProjectService.GetApplicationsByProjectAsync(projectId, cancellationToken));

    /// <summary>درخواست‌های یک توسعه‌دهنده (فقط ادمین یا خود توسعه‌دهنده).</summary>
    [HttpGet("developer/{developerId:int}/applications")]
    [Authorize(Roles = "Admin,JobSeeker")]
    public async Task<ActionResult<IReadOnlyList<DailyApplicationDto>>> GetByDeveloper(
        int developerId,
        CancellationToken cancellationToken)
    {
        if (developerId != CurrentUserId() && CurrentUserRole() != UserRole.Admin)
            return Forbid();

        return Handle(await _dailyProjectService.GetApplicationsByDeveloperAsync(developerId, cancellationToken));
    }
}