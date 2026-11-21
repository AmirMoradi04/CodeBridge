using System.Security.Claims;
using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// کنترلر پایه: خواندن شناسه و نقش کاربر از توکن JWT و تبدیل Result به پاسخ HTTP.
/// منطق کسب‌وکار اینجا وجود ندارد؛ فقط انتقال Result سرویس به خروجی کنترلر.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>شناسه‌ی کاربر درخواست‌دهنده از claims توکن.</summary>
    protected int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("کاربر درخواست‌دهنده در توکن شناسایی نشده است.");

        if (!int.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("شناسه‌ی کاربر در توکن نامعتبر است.");

        return userId;
    }

    /// <summary>نقش کاربر درخواست‌دهنده؛ در نبود claim، نقش پیش‌فرض JobSeeker در نظر گرفته می‌شود.</summary>
    protected UserRole CurrentUserRole()
    {
        var raw = User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role");

        return Enum.TryParse<UserRole>(raw, ignoreCase: true, out var role)
            ? role
            : UserRole.JobSeeker;
    }

    /// <summary>آدرس IP درخواست‌دهنده برای ثبت در گزارش ممیزی و Refresh Token.</summary>
    protected string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    protected IActionResult Handle(Result result)
        => result.IsSuccess ? Ok(result) : BadRequest(result);

    protected ActionResult<T> Handle<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Data) : BadRequest(result);
}