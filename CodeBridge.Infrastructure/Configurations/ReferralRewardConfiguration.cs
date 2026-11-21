using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ReferralRewardConfiguration : BaseConfiguration<ReferralReward>
{
    public override void Configure(EntityTypeBuilder<ReferralReward> builder)
    {
        base.Configure(builder);

        builder.ToTable("ReferralRewards");

        builder.Property(reward => reward.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(reward => reward.Date).IsRequired();

        builder.HasIndex(reward => new { reward.ReferrerId, reward.RefereeId }).IsUnique();

        builder.HasOne(reward => reward.Referrer)
            .WithMany()
            .HasForeignKey(reward => reward.ReferrerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reward => reward.Referee)
            .WithMany()
            .HasForeignKey(reward => reward.RefereeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
