using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.User;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// مدیریت کاربران؛ همه‌ی منطق در IUserService است.
/// کارهای مدیریتی (فهرست کامل، در انتظار تایید، تایید/رد) فقط برای ادمین است.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        [FromQuery] UserRole? role,
        CancellationToken cancellationToken)
        => Handle(await _userService.GetAllAsync(role, cancellationToken));

    /// <summary>پروفایل؛ فقط خود کاربر یا ادمین.</summary>
    [HttpGet("{id:int}/profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(
        int id,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(id))
            return Forbid();

        return Handle(await _userService.GetProfileAsync(id, cancellationToken));
    }

    /// <summary>ویرایش پروفایل؛ فقط خود کاربر یا ادمین.</summary>
    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpdateProfile(
        int id,
        [FromBody] UpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(id))
            return Forbid();

        return Handle(await _userService.UpdateProfileAsync(id, dto, cancellationToken));
    }

    [HttpGet("pending")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetPendingApprovals(
        CancellationToken cancellationToken)
        => Handle(await _userService.GetPendingApprovalsAsync(cancellationToken));

    [HttpGet("job-seekers/{level}")]
    [Authorize(Roles = "Admin,Employer")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetJobSeekersByLevel(
        DeveloperLevelType level,
        CancellationToken cancellationToken)
        => Handle(await _userService.GetJobSeekersByLevelAsync(level, cancellationToken));

    [HttpGet("count/{role}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<int>> CountByRole(
        UserRole role,
        CancellationToken cancellationToken)
        => Handle(await _userService.CountByRoleAsync(role, cancellationToken));

    /// <summary>تایید حساب کاربر؛ شناسه‌ی ادمین از توکن خوانده می‌شود.</summary>
    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _authService.ApproveUserAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>رد یا لغو تایید حساب کاربر.</summary>
    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Reject(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _authService.RejectUserAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>دسترسی به داده‌ی یک کاربر فقط برای خودش یا ادمین.</summary>
    private bool CanAccess(int userId)
        => userId == CurrentUserId() || CurrentUserRole() == UserRole.Admin;
}
