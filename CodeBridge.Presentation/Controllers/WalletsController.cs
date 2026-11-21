using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Wallet;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>مدیریت کیف پول کاربران.</summary>
[Route("api/[controller]")]
[Authorize]
public class WalletsController : BaseApiController
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>دریافت کیف پول جاری.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<WalletDto>> GetMyWallet(CancellationToken cancellationToken)
        => Handle(await _walletService.GetByUserIdAsync(CurrentUserId(), cancellationToken));

    /// <summary>کیف پول با تاریخچه‌ی تراکنش‌ها.</summary>
    [HttpGet("me/transactions")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<WalletDto>> GetMyWalletWithTransactions(CancellationToken cancellationToken)
        => Handle(await _walletService.GetWithTransactionsAsync(CurrentUserId(), cancellationToken));

    /// <summary>واریز وجه به کیف پول خود.</summary>
    [HttpPost("deposit")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<WalletDto>> Deposit([FromBody] DepositDto dto, CancellationToken cancellationToken)
        => Handle(await _walletService.DepositAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>برداشت وجه از کیف پول خود.</summary>
    [HttpPost("withdraw")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<WalletDto>> Withdraw([FromBody] WithdrawDto dto, CancellationToken cancellationToken)
        => Handle(await _walletService.WithdrawAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>اعتباردهی به کاربر توسط ادمین.</summary>
    [HttpPost("credit")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<WalletDto>> Credit(
        [FromBody] CreditDebitDto dto,
        CancellationToken cancellationToken)
        => Handle(await _walletService.CreditUserAsync(
            CurrentUserId(),
            dto.TargetUserId,
            dto.Amount,
            dto.Description ?? string.Empty,
            cancellationToken));

    /// <summary>کسر وجه از کاربر توسط ادمین.</summary>
    [HttpPost("debit")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<WalletDto>> Debit(
        [FromBody] CreditDebitDto dto,
        CancellationToken cancellationToken)
        => Handle(await _walletService.DebitUserAsync(
            CurrentUserId(),
            dto.TargetUserId,
            dto.Amount,
            dto.Description ?? string.Empty,
            cancellationToken));

    /// <summary>دریافت کیف پول ادمین (حساب امانی).</summary>
    [HttpGet("admin-wallet")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<WalletDto>> GetAdminWallet(CancellationToken cancellationToken)
        => Handle(await _walletService.GetAdminWalletAsync(cancellationToken));

    /// <summary>گزارش سراسری همه‌ی تراکنش‌های سیستم برای پنل ادمین.</summary>
    [HttpGet("admin/transactions")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<AdminTransactionDto>>> GetSystemTransactions(
        CancellationToken cancellationToken)
        => Handle(await _walletService.GetSystemTransactionsAsync(cancellationToken));
}

/// <summary>انتقال وجه توسط ادمین (credit/debit).</summary>
public class CreditDebitDto
{
    public int TargetUserId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}