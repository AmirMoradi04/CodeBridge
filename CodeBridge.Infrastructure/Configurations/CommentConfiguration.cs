using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class CommentConfiguration : BaseConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Comments");

        builder.Property(comment => comment.Content).IsRequired().HasMaxLength(2000);

        builder.HasIndex(comment => comment.PostId);
        builder.HasIndex(comment => comment.AuthorId);

        builder.HasOne(comment => comment.Post)
            .WithMany(post => post.Comments)
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(comment => comment.Author)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // پاسخ تودرتو به دیدگاه والد؛ حذف والد باعث حذف آبشاری پاسخ‌ها نمی‌شود
        builder.HasOne(comment => comment.ParentComment)
            .WithMany(parent => parent.Replies)
            .HasForeignKey(comment => comment.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
