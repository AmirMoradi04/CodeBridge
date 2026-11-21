using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Wallet;

/// <summary>اطلاعات کیف پول کاربر.</summary>
public sealed class WalletDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public WalletOwnerType OwnerType { get; init; }
    public decimal Balance { get; init; }
    public decimal TotalDeposited { get; init; }
    public decimal TotalWithdrawn { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<WalletTransactionDto> Transactions { get; init; } = Array.Empty<WalletTransactionDto>();
}