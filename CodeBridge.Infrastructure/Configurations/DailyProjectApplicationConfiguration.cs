using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class DailyProjectApplicationConfiguration : BaseConfiguration<DailyProjectApplication>
{
    public override void Configure(EntityTypeBuilder<DailyProjectApplication> builder)
    {
        base.Configure(builder);

        builder.ToTable("DailyProjectApplications");

        builder.Property(application => application.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(application => application.AdminNote).HasMaxLength(500);

        builder.HasIndex(application => application.DailyProjectId);
        builder.HasIndex(application => application.DeveloperId);
        builder.HasIndex(application => application.Status);

        // تنها یک درخواست برای هر توسعه‌دهنده در هر پروژه
        builder.HasIndex(application => new { application.DailyProjectId, application.DeveloperId })
            .IsUnique();

        builder.HasOne(application => application.DailyProject)
            .WithMany(project => project.Applications)
            .HasForeignKey(application => application.DailyProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(application => application.Developer)
            .WithMany(user => user.DailyApplications)
            .HasForeignKey(application => application.DeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(application => application.ProofAttachment)
            .WithMany()
            .HasForeignKey(application => application.ProofAttachmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
