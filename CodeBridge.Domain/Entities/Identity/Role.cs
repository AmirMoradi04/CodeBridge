using CodeBridge.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CodeBridge.Domain.Entities.Identity;

/// <summary>
/// نقش‌های Identity؛ از BaseEntity ارث نمی‌برد و فقط IAuditableEntity را پیاده‌سازی می‌کند.
/// </summary>
public class Role : IdentityRole<int>, IAuditableEntity
{
    // Id از IdentityRole<int> به ارث می‌رسد
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    // سازنده‌ی بدون پارامتر برای Identity و EF Core
    public Role()
    {
    }

    public Role(string roleName) : base(roleName)
    {
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    /// <summary>اعتبارسنجی نام نقش.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("نام نقش الزامی است.");
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
