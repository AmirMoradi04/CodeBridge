namespace CodeBridge.Domain.Common;

/// <summary>
/// قرارداد مشترک انتیتی‌های قابل ممیزی؛ شامل شناسه، زمان‌ها و حذف نرم.
/// </summary>
public interface IAuditableEntity
{
    int Id { get; }

    DateTime CreatedAt { get; }

    DateTime? UpdatedAt { get; }

    bool IsDeleted { get; }

    void MarkAsDeleted();
}
