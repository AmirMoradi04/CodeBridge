using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ReviewConfiguration : BaseConfiguration<Review>
{
    public override void Configure(EntityTypeBuilder<Review> builder)
    {
        base.Configure(builder);

        builder.ToTable("Reviews");

        builder.Property(review => review.Rating).IsRequired();
        builder.Property(review => review.Comment).HasMaxLength(2000);

        // هر کارفرما فقط یک بار می‌تواند برای هر پروژه به هر کاربر امتیاز بدهد
        builder.HasIndex(review => new
            {
                review.ProjectId,
                review.ReviewerId,
                review.TargetUserId
            })
            .IsUnique();

        builder.HasOne(review => review.Project)
            .WithMany()
            .HasForeignKey(review => review.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(review => review.Contract)
            .WithMany()
            .HasForeignKey(review => review.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(review => review.Reviewer)
            .WithMany()
            .HasForeignKey(review => review.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(review => review.TargetUser)
            .WithMany()
            .HasForeignKey(review => review.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
