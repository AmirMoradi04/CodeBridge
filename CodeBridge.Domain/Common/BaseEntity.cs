namespace CodeBridge.Domain.Common;

/// <summary>
/// پایه‌ی انتیتی‌های دامنه؛ وضعیت حذف نرم و زمان‌ها را مدیریت می‌کند.
/// </summary>
public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    /// <summary>قواعد اعتبارسنجی هر انتیتی باید در همین متد پیاده‌سازی شود.</summary>
    public abstract void Validate();

    // پیاده‌سازی صریح اینترفیس تا MarkAsDeleted در سطح دامنه protected بماند
    void IAuditableEntity.MarkAsDeleted() => MarkAsDeleted();

    /// <summary>حذف نرم انتیتی به‌جای حذف فیزیکی رکورد.</summary>
    protected void MarkAsDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
