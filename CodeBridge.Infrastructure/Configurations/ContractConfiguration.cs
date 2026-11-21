using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ContractConfiguration : BaseConfiguration<Contract>
{
    public override void Configure(EntityTypeBuilder<Contract> builder)
    {
        base.Configure(builder);

        builder.ToTable("Contracts");

        builder.Property(contract => contract.TotalAmount).HasPrecision(18, 2);
        builder.Property(contract => contract.PlatformFee).HasPrecision(18, 2);
        builder.Property(contract => contract.DeveloperShare).HasPrecision(18, 2);
        builder.Property(contract => contract.MentorShare).HasPrecision(18, 2);
        builder.Property(contract => contract.DepositPaid).HasPrecision(18, 2).HasDefaultValue(0m);

        // وضعیت تسویه کامل
        builder.Property(contract => contract.IsFullyPaid).IsRequired().HasDefaultValue(false);

        builder.Property(contract => contract.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(contract => contract.DeveloperId);
        builder.HasIndex(contract => contract.Status);

        // وضعیت امضای NDA
        builder.Property(contract => contract.IsNdaSigned).IsRequired().HasDefaultValue(false);
        builder.Property(contract => contract.NdaSignedAt);

        // هر پروژه فقط یک قرارداد دارد
        builder.HasOne(contract => contract.Project)
            .WithOne(project => project.Contract)
            .HasForeignKey<Contract>(contract => contract.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(contract => contract.Developer)
            .WithMany(user => user.DeveloperContracts)
            .HasForeignKey(contract => contract.DeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(contract => contract.Employer)
            .WithMany(user => user.EmployerContracts)
            .HasForeignKey(contract => contract.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(contract => contract.Admin)
            .WithMany(user => user.AdminContracts)
            .HasForeignKey(contract => contract.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
