using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ProjectMemberConfiguration : BaseConfiguration<ProjectMember>
{
    public override void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProjectMembers");

        builder.Property(member => member.JoinedAt).IsRequired();

        // هر کاربر فقط یک بار در هر پروژه عضو می‌شود
        builder.HasIndex(member => new { member.ProjectId, member.UserId }).IsUnique();

        builder.HasOne(member => member.Project)
            .WithMany(project => project.Members)
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany(user => user.ProjectMemberships)
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.AddedByAdmin)
            .WithMany()
            .HasForeignKey(member => member.AddedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}