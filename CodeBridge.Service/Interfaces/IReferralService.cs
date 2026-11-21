using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Referral;

namespace CodeBridge.Service.Interfaces;

/// <summary>سیستم معرف: اعمال کد معرف و گزارش پاداش‌ها.</summary>
public interface IReferralService
{
    /// <summary>اعمال کد معرف برای کاربر تازه و واریز پاداش به هر دو طرف.</summary>
    Task<Result> ApplyReferralCodeAsync(
        int newUserId,
        string referralCode,
        CancellationToken cancellationToken = default);

    /// <summary>اطلاعات پنل معرف: کد، فهرست دعوت‌شدگان و مجموع پاداش.</summary>
    Task<Result<ReferralInfoDto>> GetMyReferralInfoAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>دریافت یا ساخت کد معرف کاربر؛ همیشه یک کد یکتا برمی‌گرداند.</summary>
    Task<Result<string>> EnsureReferralCodeAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
