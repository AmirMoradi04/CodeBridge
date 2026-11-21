using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ResumeConfiguration : BaseConfiguration<Resume>
{
    public override void Configure(EntityTypeBuilder<Resume> builder)
    {
        base.Configure(builder);

        builder.ToTable("Resumes");

        builder.Property(resume => resume.Title).IsRequired().HasMaxLength(200);
        builder.Property(resume => resume.Summary).HasMaxLength(2000);
        builder.Property(resume => resume.Skills).HasMaxLength(1000);

        builder.Property(resume => resume.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // هر کاربر فقط یک رزومه دارد
        builder.HasIndex(resume => resume.UserId).IsUnique();

        // رابطه‌ی یک‌به‌یک با فایل رزومه
        builder.HasOne(resume => resume.Attachment)
            .WithOne()
            .HasForeignKey<Resume>(resume => resume.AttachmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // ادمین بررسی‌کننده‌ی رزومه
        builder.HasOne(resume => resume.ReviewedByAdmin)
            .WithMany()
            .HasForeignKey(resume => resume.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
