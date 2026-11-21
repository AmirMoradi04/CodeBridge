using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Wallet;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد عملیات کیف پول: واریز، برداشت، تراکنش‌ها و گردش مالی.</summary>
public interface IWalletService
{
    /// <summary>دریافت کیف پول کاربر.</summary>
    Task<Result<WalletDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>دریافت کیف پول با تاریخچه‌ی تراکنش‌ها.</summary>
    Task<Result<WalletDto>> GetWithTransactionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>واریز وجه به کیف پول (شارژ).</summary>
    Task<Result<WalletDto>> DepositAsync(int userId, DepositDto dto, CancellationToken cancellationToken = default);

    /// <summary>برداشت وجه از کیف پول.</summary>
    Task<Result<WalletDto>> WithdrawAsync(int userId, WithdrawDto dto, CancellationToken cancellationToken = default);

    /// <summary>افزودن وجه توسط ادمین (اعتباردهی).</summary>
    Task<Result<WalletDto>> CreditUserAsync(int adminId, int targetUserId, decimal amount, string description, CancellationToken cancellationToken = default);

    /// <summary>کسر وجه توسط ادمین (برداشت اجباری).</summary>
    Task<Result<WalletDto>> DebitUserAsync(int adminId, int targetUserId, decimal amount, string description, CancellationToken cancellationToken = default);

    /// <summary>دریافت کیف پول ادمین (حساب امانی).</summary>
    Task<Result<WalletDto>> GetAdminWalletAsync(CancellationToken cancellationToken = default);

    /// <summary>گزارش سراسری تراکنش‌های همه‌ی کیف پول‌ها (فقط ادمین).</summary>
    Task<Result<IReadOnlyList<AdminTransactionDto>>> GetSystemTransactionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>انتقال وجه از حساب امانی ادمین به توسعه‌دهنده.</summary>
    Task<Result> PayDeveloperFromEscrowAsync(
        int adminId,
        int developerId,
        decimal amount,
        WalletTransactionType transactionType,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی. نیاز به نقش ادمین ندارد.</summary>
    Task<Result> EmployerDepositToEscrowAsync(
        int employerId,
        decimal amount,
        int? relatedProjectId,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>پرداخت کاربر (مثلاً کارجو) به حساب امانی ادمین.</summary>
    Task<Result> TransferToEscrowAsync(
        int userId,
        decimal amount,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>پرداخت از حساب امانی به کاربر (مثلاً سهم برگزارکننده‌ی جلسه).</summary>
    Task<Result> PayoutFromEscrowAsync(
        int userId,
        decimal amount,
        WalletTransactionType transactionType,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default);
}