using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class AchievementConfiguration : BaseConfiguration<Achievement>
{
    public override void Configure(EntityTypeBuilder<Achievement> builder)
    {
        base.Configure(builder);

        builder.ToTable("Achievements");

        builder.Property(achievement => achievement.Title).IsRequired().HasMaxLength(200);
        builder.Property(achievement => achievement.Description).IsRequired().HasMaxLength(1000);
        builder.Property(achievement => achievement.IconUrl).HasMaxLength(500);

        builder.Property(achievement => achievement.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // هر نوع دستاورد فقط یک بار تعریف می‌شود
        builder.HasIndex(achievement => achievement.Type).IsUnique();
    }
}
