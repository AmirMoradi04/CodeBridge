using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Auth;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// ثبت‌نام، ورود، تازه‌سازی توکن و خروج.
/// همه‌ی منطق در IAuthService است و کنترلر فقط Result را به HTTP تبدیل می‌کند.
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// ثبت‌نام عمومی. حساب ساخته‌شده تا تایید ادمین غیرفعال است،
    /// پس در پاسخ توکنی برگردانده نمی‌شود.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.RegisterAsync(dto, resumeUpload: null, ClientIp(), cancellationToken));

    /// <summary>
    /// ثبت‌نام کارجو به‌صورت چندبخشی؛ فایل رزومه در همین درخواست اجباری است.
    /// نقش همیشه JobSeeker است و توکنی برگردانده نمی‌شود.
    /// </summary>
    [HttpPost("register/jobseeker")]
    [AllowAnonymous]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<AuthResponseDto>> RegisterJobSeeker(
        [FromForm] RegisterDto dto,
        IFormFile? resumeFile,
        CancellationToken cancellationToken)
    {
        if (dto.UserRole != UserRole.JobSeeker)
            return BadRequest(new { isSuccess = false, message = "این مسیر فقط برای ثبت‌نام کارجو است." });

        if (resumeFile is null || resumeFile.Length == 0)
            return BadRequest(new { isSuccess = false, message = "بارگذاری فایل رزومه الزامی است." });

        await using var stream = resumeFile.OpenReadStream();

        var upload = new ResumeUpload(stream, resumeFile.FileName, resumeFile.ContentType);

        return Handle(await _authService.RegisterAsync(dto, upload, ClientIp(), cancellationToken));
    }

    /// <summary>ورود با ایمیل و رمز؛ فقط برای حساب‌های تاییدشده.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.LoginAsync(dto, ClientIp(), cancellationToken));

    /// <summary>چرخش Refresh Token و صدور Access Token جدید.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.RefreshTokenAsync(dto, ClientIp(), cancellationToken));

    /// <summary>خروج و باطل‌کردن Refresh Token ارسالی.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.LogoutAsync(dto, ClientIp(), cancellationToken));

    /// <summary>ارسال توکن بازیابی رمز عبور به ایمیل کاربر.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.ForgotPasswordAsync(dto, cancellationToken));

    /// <summary>بازنشانی رمز عبور با توکن بازیابی.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.ResetPasswordAsync(dto, cancellationToken));

    /// <summary>تغییر رمز عبور توسط کاربر واردشده.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken)
        => Handle(await _authService.ChangePasswordAsync(CurrentUserId(), dto, cancellationToken));
}
