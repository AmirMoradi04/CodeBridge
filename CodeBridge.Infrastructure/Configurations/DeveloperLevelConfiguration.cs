using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class DeveloperLevelConfiguration : BaseConfiguration<DeveloperLevel>
{
    public override void Configure(EntityTypeBuilder<DeveloperLevel> builder)
    {
        base.Configure(builder);

        builder.ToTable("DeveloperLevels");

        builder.Property(level => level.Level)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(level => level.AverageRating).HasPrecision(3, 2);

        // هر کاربر یک سطح برنامه‌نویسی دارد
        builder.HasIndex(level => level.UserId).IsUnique();

        builder.HasOne(level => level.User)
            .WithOne(user => user.DeveloperLevel)
            .HasForeignKey<DeveloperLevel>(level => level.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
