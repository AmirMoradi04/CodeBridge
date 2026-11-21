using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ProjectConfiguration : BaseConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.ToTable("Projects");

        builder.Property(project => project.Title).IsRequired().HasMaxLength(200);
        builder.Property(project => project.Description).HasMaxLength(5000);
        builder.Property(project => project.Budget).HasPrecision(18, 2);

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(project => project.RequiredLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(project => project.EmployerId);
        builder.HasIndex(project => project.Status);

        // انتشار عمومی پروژه
        builder.Property(project => project.IsPublic).IsRequired().HasDefaultValue(false);
        builder.Property(project => project.PublishedAt);

        // تایید نهایی پروژه
        builder.Property(project => project.IsFinalApproved).IsRequired().HasDefaultValue(false);
        builder.Property(project => project.FinalApprovedAt);

        // کارفرمای پروژه
        builder.HasOne(project => project.Employer)
            .WithMany(user => user.EmployerProjects)
            .HasForeignKey(project => project.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        // برنامه‌نویس تخصیص‌یافته
        builder.HasOne(project => project.AssignedDeveloper)
            .WithMany(user => user.AssignedProjects)
            .HasForeignKey(project => project.AssignedDeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        // ادمین تاییدکننده
        builder.HasOne(project => project.ApprovedByAdmin)
            .WithMany()
            .HasForeignKey(project => project.ApprovedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // رابطه‌ی یک‌به‌یک با قرارداد
        builder.HasOne(project => project.Contract)
            .WithOne(contract => contract.Project)
            .HasForeignKey<Contract>(contract => contract.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
