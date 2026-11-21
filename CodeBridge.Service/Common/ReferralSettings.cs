namespace CodeBridge.Service.Common;

/// <summary>تنظیمات سیستم معرف؛ از بخش Referral در appsettings خوانده می‌شود.</summary>
public class ReferralSettings
{
    /// <summary>مبلغ پاداش هر دعوت موفق برای هر یک از دو طرف (تومان).</summary>
    public decimal RewardAmount { get; set; } = 50000m;
}
