using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class AuditLogConfiguration : BaseConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("AuditLogs");

        builder.Property(log => log.Action).IsRequired().HasMaxLength(200);
        builder.Property(log => log.EntityType).HasMaxLength(100);
        builder.Property(log => log.Details).HasMaxLength(2000);
        builder.Property(log => log.IpAddress).HasMaxLength(50);

        builder.HasIndex(log => log.CreatedAt);

        builder.HasOne(log => log.User)
            .WithMany(user => user.AuditLogs)
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
