namespace CodeBridge.Service.Common;

/// <summary>سیاست قفل موقت حساب پس از تلاش‌های ناموفق ورود.</summary>
public sealed class SecuritySettings
{
    /// <summary>تعداد تلاش ناموفق مجاز پیش از قفل شدن حساب.</summary>
    public int MaxFailedLoginAttempts { get; init; } = 5;

    /// <summary>مدت قفل حساب به دقیقه.</summary>
    public int LockoutMinutes { get; init; } = 15;
}
