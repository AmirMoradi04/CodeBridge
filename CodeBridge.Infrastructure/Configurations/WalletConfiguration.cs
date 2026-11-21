using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class WalletConfiguration : BaseConfiguration<Wallet>
{
    public override void Configure(EntityTypeBuilder<Wallet> builder)
    {
        base.Configure(builder);

        builder.ToTable("Wallets");

        builder.Property(wallet => wallet.Balance).HasPrecision(18, 2);
        builder.Property(wallet => wallet.TotalDeposited).HasPrecision(18, 2);
        builder.Property(wallet => wallet.TotalWithdrawn).HasPrecision(18, 2);

        builder.Property(wallet => wallet.OwnerType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // هر کاربر فقط یک کیف پول دارد
        builder.HasIndex(wallet => wallet.UserId).IsUnique();

        builder.HasOne(wallet => wallet.User)
            .WithOne(user => user.Wallet)
            .HasForeignKey<Wallet>(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
