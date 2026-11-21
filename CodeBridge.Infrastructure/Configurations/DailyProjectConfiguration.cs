using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class DailyProjectConfiguration : BaseConfiguration<DailyProject>
{
    public override void Configure(EntityTypeBuilder<DailyProject> builder)
    {
        base.Configure(builder);

        builder.ToTable("DailyProjects");

        builder.Property(project => project.Title).IsRequired().HasMaxLength(200);
        builder.Property(project => project.Description).HasMaxLength(5000);
        builder.Property(project => project.TotalBudget).HasPrecision(18, 2);
        builder.Property(project => project.PricePerUnit).HasPrecision(18, 2);
        builder.Property(project => project.PaidAmount).HasPrecision(18, 2);
        builder.Property(project => project.Instructions).HasMaxLength(5000);

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(project => project.ProofRequired).IsRequired().HasDefaultValue(true);
        builder.Property(project => project.DeadlineSeconds).IsRequired().HasDefaultValue(60);

        builder.HasIndex(project => project.EmployerId);
        builder.HasIndex(project => project.Status);

        // کارفرمای پروژه
        builder.HasOne(project => project.Employer)
            .WithMany(user => user.DailyProjects)
            .HasForeignKey(project => project.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        // درخواست‌ها
        builder.HasMany(project => project.Applications)
            .WithOne(application => application.DailyProject)
            .HasForeignKey(application => application.DailyProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
