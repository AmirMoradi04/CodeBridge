using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class LikeConfiguration : BaseConfiguration<Like>
{
    public override void Configure(EntityTypeBuilder<Like> builder)
    {
        base.Configure(builder);

        builder.ToTable("Likes");

        // هر کاربر فقط یک بار می‌تواند یک پست را لایک کند
        builder.HasIndex(like => new { like.PostId, like.UserId }).IsUnique();

        builder.HasOne(like => like.Post)
            .WithMany(post => post.Likes)
            .HasForeignKey(like => like.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(like => like.User)
            .WithMany(user => user.Likes)
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
