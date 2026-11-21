using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// گزارش وقایع مهم سیستم برای ممیزی و بررسی‌های امنیتی.
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public int? EntityId { get; private set; }

    public string? Details { get; private set; }

    public string? IpAddress { get; private set; }

    // --- Navigation Properties ---
    public User? User { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private AuditLog()
    {
    }

    public AuditLog(
        string action,
        string entityType,
        int? userId = null,
        int? entityId = null,
        string? details = null,
        string? ipAddress = null)
    {
        Action = action;
        EntityType = entityType;
        UserId = userId;
        EntityId = entityId;
        Details = details;
        IpAddress = ipAddress;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Action))
            throw new InvalidOperationException("نوع عملیات ثبت‌شده در AuditLog الزامی است.");
    }
}
