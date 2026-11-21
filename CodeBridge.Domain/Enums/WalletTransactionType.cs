namespace CodeBridge.Domain.Enums;

/// <summary>نوع تراکنش کیف پول.</summary>
public enum WalletTransactionType
{
    Deposit = 0,
    Withdrawal = 1,
    ProjectPayment = 2,
    DailyTaskPayment = 3,
    PlatformFee = 4,
    Refund = 5,
    LiveSessionPayment = 6
}
