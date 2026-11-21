using CodeBridge.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class SaeedConfiguration : BaseConfiguration<Saeed>
{
    public override void Configure(EntityTypeBuilder<Saeed> builder)
    {
        base.Configure(builder);

        builder.ToTable("Saeeds");

        builder.Property(saeed => saeed.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
