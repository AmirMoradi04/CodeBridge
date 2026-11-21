using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class PasswordHistoryConfiguration : BaseConfiguration<PasswordHistory>
{
    public override void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable("PasswordHistories");

        builder.Property(history => history.PasswordHash).IsRequired().HasMaxLength(500);

        builder.HasIndex(history => new { history.UserId, history.CreatedAt });

        builder.HasOne(history => history.User)
            .WithMany()
            .HasForeignKey(history => history.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
