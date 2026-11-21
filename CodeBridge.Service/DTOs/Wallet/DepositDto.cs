namespace CodeBridge.Service.DTOs.Wallet;

/// <summary>درخواست واریز وجه به کیف پول.</summary>
public sealed class DepositDto
{
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}