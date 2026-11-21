using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.LiveSession;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد جلسه‌های زنده‌ی آموزش کار برنامه‌نویس ارشد.</summary>
public interface ILiveSessionService
{
    /// <summary>ساخت جلسه توسط برنامه‌نویس ارشد برای پروژه‌ی تخصیص‌یافته.</summary>
    Task<Result<LiveSessionDto>> CreateSessionAsync(
        int seniorId,
        CreateLiveSessionDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ثبت‌نام و پرداخت کارجو؛ لینک جلسه فقط در خروجی موفق برگردانده می‌شود.
    /// </summary>
    Task<Result<string>> JoinSessionAsync(
        int sessionId,
        int juniorId,
        CancellationToken cancellationToken = default);

    /// <summary>پایان جلسه و تسویه با برگزارکننده از حساب امانی.</summary>
    Task<Result> EndSessionAsync(
        int sessionId,
        int seniorId,
        CancellationToken cancellationToken = default);

    /// <summary>ثبت لینک ضبط پس از پایان جلسه.</summary>
    Task<Result> UpdateRecordingLinkAsync(
        int sessionId,
        int seniorId,
        string recordingLink,
        CancellationToken cancellationToken = default);

    /// <summary>جلسه‌های در دسترس کارجو؛ لینک فقط برای شرکت‌کننده‌ها برگردانده می‌شود.</summary>
    Task<Result<IReadOnlyList<LiveSessionDto>>> GetAvailableSessionsAsync(
        int juniorId,
        CancellationToken cancellationToken = default);

    /// <summary>جلسه‌های ساخته‌شده توسط یک برنامه‌نویس ارشد.</summary>
    Task<Result<IReadOnlyList<LiveSessionDto>>> GetSeniorSessionsAsync(
        int seniorId,
        CancellationToken cancellationToken = default);

    /// <summary>همه‌ی جلسه‌ها برای پایش ادمین.</summary>
    Task<Result<IReadOnlyList<LiveSessionDto>>> GetAdminSessionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>جزئیات جلسه؛ نمایش لینک بر اساس نقش و پرداخت کنترل می‌شود.</summary>
    Task<Result<LiveSessionDto>> GetSessionDetailsAsync(
        int sessionId,
        int userId,
        string role,
        CancellationToken cancellationToken = default);
}
