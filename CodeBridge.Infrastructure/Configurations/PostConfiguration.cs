using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class PostConfiguration : BaseConfiguration<Post>
{
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        base.Configure(builder);

        builder.ToTable("Posts");

        builder.Property(post => post.Content).IsRequired().HasMaxLength(5000);
        builder.Property(post => post.IsPublished).HasDefaultValue(false);

        builder.HasIndex(post => post.AuthorId);
        builder.HasIndex(post => post.IsPublished);

        builder.HasOne(post => post.Author)
            .WithMany(user => user.Posts)
            .HasForeignKey(post => post.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // تصویر پست (اختیاری)
        builder.HasOne(post => post.Attachment)
            .WithOne()
            .HasForeignKey<Post>(post => post.AttachmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // با حذف پست، دیدگاه‌ها و لایک‌های آن هم حذف می‌شوند
        builder.HasMany(post => post.Comments)
            .WithOne(comment => comment.Post)
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(post => post.Likes)
            .WithOne(like => like.Post)
            .HasForeignKey(like => like.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
