using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Wallet;

/// <summary>تراکنش کیف پول.</summary>
public sealed class WalletTransactionDto
{
    public int Id { get; init; }
    public int WalletId { get; init; }
    public decimal Amount { get; init; }
    public WalletTransactionType Type { get; init; }
    public string? Description { get; init; }
    public int? RelatedEntityId { get; init; }
    public DateTime CreatedAt { get; init; }
}