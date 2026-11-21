using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class AttachmentConfiguration : BaseConfiguration<Attachment>
{
    public override void Configure(EntityTypeBuilder<Attachment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Attachments");

        builder.Property(attachment => attachment.FileName).IsRequired().HasMaxLength(255);
        builder.Property(attachment => attachment.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(attachment => attachment.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(attachment => attachment.RelatedEntityType).IsRequired().HasMaxLength(100);

        builder.HasIndex(attachment => new { attachment.RelatedEntityType, attachment.RelatedEntityId });

        // مالک فایل
        builder.HasOne(attachment => attachment.Owner)
            .WithMany(user => user.Attachments)
            .HasForeignKey(attachment => attachment.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
