using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class WalletTransactionConfiguration : BaseConfiguration<WalletTransaction>
{
    public override void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("WalletTransactions");

        builder.Property(transaction => transaction.Amount).HasPrecision(18, 2);

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(transaction => transaction.Description).HasMaxLength(500);

        builder.HasIndex(transaction => transaction.WalletId);
        builder.HasIndex(transaction => transaction.RelatedEntityId);

        builder.HasOne(transaction => transaction.Wallet)
            .WithMany(wallet => wallet.Transactions)
            .HasForeignKey(transaction => transaction.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
