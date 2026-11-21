using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Project;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>مدیریت پروژه‌ها؛ همه‌ی منطق در IProjectService است.</summary>
[Route("api/[controller]")]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>ثبت پروژه فقط توسط کارفرما.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] CreateProjectDto dto,
        CancellationToken cancellationToken)
        => Handle(await _projectService.CreateAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>همه‌ی پروژه‌ها برای پنل ادمین با فیلتر اختیاری وضعیت.</summary>
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(
        [FromQuery] ProjectStatus? status,
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetAllAsync(status, cancellationToken));

    /// <summary>جزئیات پروژه؛ کنترل مالکیت/عضویت در سرویس انجام می‌شود.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<ProjectDto>> GetById(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetByIdAsync(id, CurrentUserId(), CurrentUserRole(), cancellationToken));

    [HttpGet("employer/{employerId:int}")]
    [Authorize(Roles = "Admin,Employer")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetByEmployer(
        int employerId,
        CancellationToken cancellationToken)
    {
        // کارفرما فقط پروژه‌های خودش را می‌بیند؛ ادمین همه را
        if (employerId != CurrentUserId() && CurrentUserRole() != UserRole.Admin)
            return Forbid();

        return Handle(await _projectService.GetByEmployerAsync(employerId, cancellationToken));
    }

    [HttpGet("developer/{developerId:int}")]
    [Authorize(Roles = "Admin,JobSeeker")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetByDeveloper(
        int developerId,
        CancellationToken cancellationToken)
    {
        // برنامه‌نویس فقط پروژه‌های خودش را می‌بیند؛ ادمین همه را
        if (developerId != CurrentUserId() && CurrentUserRole() != UserRole.Admin)
            return Forbid();

        return Handle(await _projectService.GetByDeveloperAsync(developerId, cancellationToken));
    }

    /// <summary>پروژه‌های تخصیص‌یافته یا عضویت‌دارِ کاربر جاری.</summary>
    [HttpGet("assigned")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAssigned(
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetAssignedAsync(CurrentUserId(), cancellationToken));

    [HttpGet("pending")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetPending(
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetPendingAsync(cancellationToken));

    [HttpGet("unassigned")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetUnassigned(
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetUnassignedAsync(cancellationToken));

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _projectService.ApproveAsync(id, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Reject(
        int id,
        [FromQuery] string reason,
        CancellationToken cancellationToken)
        => Handle(await _projectService.RejectAsync(id, CurrentUserId(), reason, cancellationToken));

    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Assign(
        int id,
        [FromBody] AssignProjectDto dto,
        CancellationToken cancellationToken)
        => Handle(await _projectService.AssignToDeveloperAsync(id, dto, CurrentUserId(), cancellationToken));

    /// <summary>انتشار عمومی پروژه‌ی تاییدشده.</summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Publish(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _projectService.PublishAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>افزودن عضو به تیم پروژه؛ ProjectId بدن باید با مسیر یکی باشد.</summary>
    [HttpPost("{id:int}/members")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(
        int id,
        [FromBody] AddProjectMemberDto dto,
        CancellationToken cancellationToken)
    {
        // تطابق شناسه‌ی پروژه در بدنه با شناسه‌ی مسیر اجباری است
        if (dto.ProjectId != id)
            return BadRequest(new { isSuccess = false, message = "شناسه‌ی پروژه در بدنه با مسیر یکی نیست." });

        return Handle(await _projectService.AddMemberAsync(dto, CurrentUserId(), cancellationToken));
    }

    /// <summary>فهرست اعضا؛ ادمین یا اعضای خود پروژه (کنترل در سرویس).</summary>
    [HttpGet("{id:int}/members")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberDto>>> GetMembers(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _projectService.GetMembersAsync(id, CurrentUserId(), CurrentUserRole(), cancellationToken));

    [HttpDelete("{id:int}/members/{userId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> RemoveMember(
        int id,
        int userId,
        CancellationToken cancellationToken)
        => Handle(await _projectService.RemoveMemberAsync(id, userId, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromQuery] ProjectStatus status,
        CancellationToken cancellationToken)
        => Handle(await _projectService.UpdateStatusAsync(id, status, cancellationToken));

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی.</summary>
    [HttpPost("{id:int}/deposit")]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<IActionResult> Deposit(
        int id,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
        => Handle(await _projectService.EmployerDepositAsync(id, CurrentUserId(), amount, cancellationToken));

    /// <summary>تایید نهایی و پرداخت پروژه. فقط ادمین.</summary>
    [HttpPost("{id:int}/final-approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> FinalApprove(int id, CancellationToken cancellationToken)
        => Handle(await _projectService.FinalApproveAsync(id, CurrentUserId(), cancellationToken));
}