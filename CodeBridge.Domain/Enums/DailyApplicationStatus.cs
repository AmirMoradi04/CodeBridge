namespace CodeBridge.Domain.Enums;

/// <summary>وضعیت‌های درخواست/ کار برنامه‌نویس روی پروژه‌ی روزمره.</summary>
public enum DailyApplicationStatus
{
    Pending = 0,
    InProgress = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Expired = 5
}
