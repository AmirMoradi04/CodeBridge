using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class UserAchievementConfiguration : BaseConfiguration<UserAchievement>
{
    public override void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserAchievements");

        builder.Property(userAchievement => userAchievement.EarnedAt).IsRequired();

        // هر کاربر هر دستاورد را فقط یک بار کسب می‌کند
        builder.HasIndex(userAchievement => new
            {
                userAchievement.UserId,
                userAchievement.AchievementId
            })
            .IsUnique();

        builder.HasOne(userAchievement => userAchievement.User)
            .WithMany()
            .HasForeignKey(userAchievement => userAchievement.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(userAchievement => userAchievement.Achievement)
            .WithMany()
            .HasForeignKey(userAchievement => userAchievement.AchievementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
