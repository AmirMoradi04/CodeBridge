namespace CodeBridge.Service.DTOs.Wallet;

/// <summary>درخواست برداشت وجه از کیف پول.</summary>
public sealed class WithdrawDto
{
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}