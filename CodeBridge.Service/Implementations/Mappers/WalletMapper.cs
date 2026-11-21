using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Wallet;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class WalletMapper
{
    public static WalletDto ToDto(Wallet wallet) => new()
    {
        Id = wallet.Id,
        UserId = wallet.UserId,
        OwnerType = wallet.OwnerType,
        Balance = wallet.Balance,
        TotalDeposited = wallet.TotalDeposited,
        TotalWithdrawn = wallet.TotalWithdrawn,
        CreatedAt = wallet.CreatedAt,
        UpdatedAt = wallet.UpdatedAt ?? wallet.CreatedAt,
        Transactions = wallet.Transactions
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Select(ToTransactionDto)
            .ToList()
    };

    public static WalletTransactionDto ToTransactionDto(WalletTransaction tx) => new()
    {
        Id = tx.Id,
        WalletId = tx.WalletId,
        Amount = tx.Amount,
        Type = tx.Type,
        Description = tx.Description,
        RelatedEntityId = tx.RelatedEntityId,
        CreatedAt = tx.CreatedAt
    };
}