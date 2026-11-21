using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Auth;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ثبت‌نام، ورود، چرخش توکن و مدیریت تایید حساب.</summary>
public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterDto dto,
        ResumeUpload? resumeUpload = null,
        string? ip = null,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> LoginAsync(
        LoginDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        LogoutRequestDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default);

    Task<Result> ApproveUserAsync(int userId, int adminId, CancellationToken cancellationToken = default);

    Task<Result> RejectUserAsync(int userId, int adminId, CancellationToken cancellationToken = default);

    /// <summary>ارسال لینک/توکن بازیابی رمز عبور به ایمیل کاربر.</summary>
    Task<Result> ForgotPasswordAsync(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>بازنشانی رمز عبور با توکن بازیابی.</summary>
    Task<Result> ResetPasswordAsync(
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>تغییر رمز عبور توسط کاربر واردشده.</summary>
    Task<Result> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto,
        CancellationToken cancellationToken = default);
}
