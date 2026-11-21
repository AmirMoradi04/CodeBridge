using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class RefreshTokenConfiguration : BaseConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens");

        builder.Property(token => token.Token).IsRequired().HasMaxLength(512);
        builder.HasIndex(token => token.Token).IsUnique();

        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.RevokedByIp).HasMaxLength(64);
        builder.Property(token => token.CreatedByIp).HasMaxLength(64);
        builder.Property(token => token.ReplacedByToken).HasMaxLength(512);

        // مقدار محاسبه‌شده است و در دیتابیس ذخیره نمی‌شود
        builder.Ignore(token => token.IsActive);

        builder.HasIndex(token => token.UserId);

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
