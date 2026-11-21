using CodeBridge.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

/// <summary>
/// کانفیگ مشترک همه‌ی انتیتی‌ها: کلید اصلی، زمان‌ها، حذف نرم و فیلتر سراسری.
/// </summary>
public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : class, IAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(entity => entity.UpdatedAt);

        builder.Property(entity => entity.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // فیلتر سراسری: رکوردهای حذف‌شده به‌صورت نرم در کوئری‌ها دیده نمی‌شوند
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
