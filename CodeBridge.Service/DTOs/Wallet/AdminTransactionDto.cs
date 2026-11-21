using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Wallet;

/// <summary>یک تراکنش کیف پول همراه با صاحب آن؛ برای گزارش سراسری ادمین.</summary>
public sealed class AdminTransactionDto
{
    public int Id { get; init; }

    public int WalletId { get; init; }

    public int UserId { get; init; }

    public string? UserName { get; init; }

    public string? UserEmail { get; init; }

    public decimal Amount { get; init; }

    public WalletTransactionType Type { get; init; }

    public string? Description { get; init; }

    public int? RelatedEntityId { get; init; }

    public DateTime CreatedAt { get; init; }
}
