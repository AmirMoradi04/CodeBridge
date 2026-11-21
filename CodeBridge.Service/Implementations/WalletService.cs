using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Wallet;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>سرویس کیف پول: واریز، برداشت، اعتباردهی و گردش مالی.</summary>
public sealed class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>مسیر کیف پول فرانت بر اساس نقش کاربر.</summary>
    // نقش جداگانه‌ای برای برنامه‌نویس وجود ندارد؛ کارجو همان برنامه‌نویس است
    private static string WalletLink(UserRole role) => role switch
    {
        UserRole.Admin => "/admin/wallets",
        UserRole.Employer => "/employer/wallet",
        _ => "/jobseeker/wallet"
    };

    public async Task<Result<WalletDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
            return Result<WalletDto>.Failure("کیف پول یافت نشد.");

        return Result<WalletDto>.Success(WalletMapper.ToDto(wallet));
    }

    public async Task<Result<WalletDto>> GetWithTransactionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
            return Result<WalletDto>.Failure("کیف پول یافت نشد.");

        var withTx = await _walletRepository.GetWithTransactionsAsync(wallet.Id, cancellationToken);

        return withTx is null
            ? Result<WalletDto>.Success(WalletMapper.ToDto(wallet))
            : Result<WalletDto>.Success(WalletMapper.ToDto(withTx));
    }

    public async Task<Result<WalletDto>> DepositAsync(int userId, DepositDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            return Result<WalletDto>.Failure("مبلغ واریز باید بزرگ‌تر از صفر باشد.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<WalletDto>.Failure("کاربر مورد نظر پیدا نشد.");

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
        {
            wallet = new Wallet(userId, DetermineOwnerType(user.UserRole));
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }

        wallet.Deposit(dto.Amount);
        _walletRepository.Update(wallet);

        var transaction = new WalletTransaction(
            wallet.Id,
            dto.Amount,
            WalletTransactionType.Deposit,
            dto.Description ?? "واریز وجه");

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "WalletDeposit",
            nameof(Wallet),
            wallet.Id,
            $"Amount={dto.Amount}",
            null,
            cancellationToken);

        // اعلان واریز موفق به صاحب کیف پول
        await _notificationService.SendAsync(
            userId,
            "واریز موفق به کیف پول",
            $"مبلغ {dto.Amount:N0} تومان به کیف پول شما اضافه شد.",
            NotificationType.Wallet,
            WalletLink(user.UserRole),
            cancellationToken);

        return Result<WalletDto>.Success(WalletMapper.ToDto(wallet));
    }

    public async Task<Result<WalletDto>> WithdrawAsync(int userId, WithdrawDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            return Result<WalletDto>.Failure("مبلغ برداشت باید بزرگ‌تر از صفر باشد.");

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
            return Result<WalletDto>.Failure("کیف پول یافت نشد.");

        // برای انتخاب مسیر اعلان، نقش کاربر لازم است
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (!wallet.TryWithdraw(dto.Amount))
            return Result<WalletDto>.Failure("موجودی کیف پول کافی نیست.");

        _walletRepository.Update(wallet);

        var transaction = new WalletTransaction(
            wallet.Id,
            dto.Amount,
            WalletTransactionType.Withdrawal,
            dto.Description ?? "برداشت وجه");

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "WalletWithdraw",
            nameof(Wallet),
            wallet.Id,
            $"Amount={dto.Amount}",
            null,
            cancellationToken);

        // اعلان برداشت موفق
        await _notificationService.SendAsync(
            userId,
            "برداشت از کیف پول",
            $"مبلغ {dto.Amount:N0} تومان از کیف پول شما برداشت شد.",
            NotificationType.Wallet,
            WalletLink(user?.UserRole ?? UserRole.JobSeeker),
            cancellationToken);

        return Result<WalletDto>.Success(WalletMapper.ToDto(wallet));
    }

    /// <summary>اعتباردهی به کاربر توسط ادمین: از کیف پول ادمین کسر و به کاربر واریز می‌شود.</summary>
    public async Task<Result<WalletDto>> CreditUserAsync(int adminId, int targetUserId, decimal amount, string description, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result<WalletDto>.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);
        if (admin is null)
            return Result<WalletDto>.Failure("ادمین مورد نظر پیدا نشد.");

        if (admin.UserRole != UserRole.Admin)
            return Result<WalletDto>.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");

        var target = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (target is null)
            return Result<WalletDto>.Failure("کاربر مقصد پیدا نشد.");

        // کیف پول ادمین
        var adminWallet = await GetOrCreateAdminWalletAsync(adminId, cancellationToken);

        if (!adminWallet.TryWithdraw(amount))
            return Result<WalletDto>.Failure("موجودی کیف پول ادمین کافی نیست.");

        _walletRepository.Update(adminWallet);

        var adminTx = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.ProjectPayment,
            $"کسر اعتبار کاربر {targetUserId}: {description}",
            targetUserId);

        await _transactionRepository.AddAsync(adminTx, cancellationToken);

        // کیف پول کاربر مقصد
        var targetWallet = await _walletRepository.GetByUserIdAsync(targetUserId, cancellationToken);

        if (targetWallet is null)
        {
            targetWallet = new Wallet(targetUserId, DetermineOwnerType(target.UserRole));
            await _walletRepository.AddAsync(targetWallet, cancellationToken);
        }

        targetWallet.AddFunds(amount);
        _walletRepository.Update(targetWallet);

        var targetTx = new WalletTransaction(
            targetWallet.Id,
            amount,
            WalletTransactionType.Deposit,
            $"اعتبار از ادمین: {description}",
            targetUserId);

        await _transactionRepository.AddAsync(targetTx, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "WalletCredit",
            nameof(Wallet),
            targetWallet.Id,
            $"TargetUserId={targetUserId};Amount={amount}",
            null,
            cancellationToken);

        return Result<WalletDto>.Success(WalletMapper.ToDto(targetWallet));
    }

    /// <summary>کسر وجه از کاربر توسط ادمین: از کیف پول کاربر کسر و به ادمین واریز می‌شود.</summary>
    public async Task<Result<WalletDto>> DebitUserAsync(int adminId, int targetUserId, decimal amount, string description, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result<WalletDto>.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);
        if (admin is null)
            return Result<WalletDto>.Failure("ادمین مورد نظر پیدا نشد.");

        if (admin.UserRole != UserRole.Admin)
            return Result<WalletDto>.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");

        var target = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (target is null)
            return Result<WalletDto>.Failure("کاربر مقصد پیدا نشد.");

        var targetWallet = await _walletRepository.GetByUserIdAsync(targetUserId, cancellationToken);
        if (targetWallet is null)
            return Result<WalletDto>.Failure("کیف پول کاربر مقصد یافت نشد.");

        if (amount > targetWallet.Balance)
            return Result<WalletDto>.Failure("موجودی کیف پول کاربر کافی نیست.");

        targetWallet.DeductFunds(amount);
        _walletRepository.Update(targetWallet);

        var targetTx = new WalletTransaction(
            targetWallet.Id,
            amount,
            WalletTransactionType.PlatformFee,
            $"کسر توسط ادمین: {description}",
            adminId);

        await _transactionRepository.AddAsync(targetTx, cancellationToken);

        // واریز به کیف پول ادمین
        var adminWallet = await GetOrCreateAdminWalletAsync(adminId, cancellationToken);
        adminWallet.Deposit(amount);
        _walletRepository.Update(adminWallet);

        var adminTx = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.Deposit,
            $"واریز از کاربر {targetUserId}: {description}",
            targetUserId);

        await _transactionRepository.AddAsync(adminTx, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "WalletDebit",
            nameof(Wallet),
            targetWallet.Id,
            $"TargetUserId={targetUserId};Amount={amount}",
            null,
            cancellationToken);

        return Result<WalletDto>.Success(WalletMapper.ToDto(targetWallet));
    }

    public async Task<Result<WalletDto>> GetAdminWalletAsync(CancellationToken cancellationToken = default)
    {
        var adminWallet = await GetOrCreateAdminWalletAsync(null, cancellationToken);
        return Result<WalletDto>.Success(WalletMapper.ToDto(adminWallet));
    }

    public async Task<Result<IReadOnlyList<AdminTransactionDto>>> GetSystemTransactionsAsync(
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetAllWithWalletAsync(cancellationToken);

        if (transactions.Count == 0)
            return Result<IReadOnlyList<AdminTransactionDto>>.Success(Array.Empty<AdminTransactionDto>());

        // نام صاحبان کیف پول در یک کوئری خوانده می‌شود تا N+1 رخ ندهد.
        var userIds = transactions
            .Select(transaction => transaction.Wallet?.UserId ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var users = await _userRepository.FindAsync(
            user => userIds.Contains(user.Id),
            cancellationToken);

        var usersById = users.ToDictionary(user => user.Id);

        IReadOnlyList<AdminTransactionDto> dtos = transactions
            .Select(transaction =>
            {
                var ownerId = transaction.Wallet?.UserId ?? 0;
                usersById.TryGetValue(ownerId, out var owner);

                return new AdminTransactionDto
                {
                    Id = transaction.Id,
                    WalletId = transaction.WalletId,
                    UserId = ownerId,
                    UserName = owner is null ? null : $"{owner.FirstName} {owner.LastName}".Trim(),
                    UserEmail = owner?.Email,
                    Amount = transaction.Amount,
                    Type = transaction.Type,
                    Description = transaction.Description,
                    RelatedEntityId = transaction.RelatedEntityId,
                    CreatedAt = transaction.CreatedAt
                };
            })
            .ToList();

        return Result<IReadOnlyList<AdminTransactionDto>>.Success(dtos);
    }

    /// <summary>انتقال وجه از حساب امانی ادمین به توسعه‌دهنده.</summary>
    public async Task<Result> PayDeveloperFromEscrowAsync(
        int adminId,
        int developerId,
        decimal amount,
        WalletTransactionType transactionType,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);
        if (admin is null)
            return Result.Failure("ادمین مورد نظر پیدا نشد.");

        if (admin.UserRole != UserRole.Admin)
            return Result.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");

        var developer = await _userRepository.GetByIdAsync(developerId, cancellationToken);
        if (developer is null)
            return Result.Failure("توسعه‌دهنده مورد نظر پیدا نشد.");

        var adminWallet = await GetOrCreateAdminWalletAsync(adminId, cancellationToken);

        if (adminWallet.Balance < amount)
            return Result.Failure("موجودی حساب امانی ادمین کافی نیست.");

        var devWallet = await _walletRepository.GetByUserIdAsync(developerId, cancellationToken);

        if (devWallet is null)
        {
            devWallet = new Wallet(developerId, WalletOwnerType.Developer);
            await _walletRepository.AddAsync(devWallet, cancellationToken);
        }

        adminWallet.DeductFunds(amount);
        _walletRepository.Update(adminWallet);

        devWallet.AddFunds(amount);
        _walletRepository.Update(devWallet);

        var adminTx = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.PlatformFee,
            $"پرداخت به توسعه‌دهنده از حساب امانی: {description}",
            relatedEntityId);

        await _transactionRepository.AddAsync(adminTx, cancellationToken);

        var devTx = new WalletTransaction(
            devWallet.Id,
            amount,
            transactionType,
            description,
            relatedEntityId);

        await _transactionRepository.AddAsync(devTx, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "EscrowPayment",
            nameof(Wallet),
            devWallet.Id,
            $"DeveloperId={developerId};Amount={amount};Type={transactionType}",
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی. بدون نیاز به نقش ادمین.</summary>
    public async Task<Result> EmployerDepositToEscrowAsync(
        int employerId,
        decimal amount,
        int? relatedProjectId,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var employer = await _userRepository.GetByIdAsync(employerId, cancellationToken);
        if (employer is null)
            return Result.Failure("کارفرما مورد نظر پیدا نشد.");

        if (employer.UserRole != UserRole.Employer)
            return Result.Failure("فقط کارفرما می‌تواند این عملیات را انجام دهد.");

        var employerWallet = await _walletRepository.GetByUserIdAsync(employerId, cancellationToken);
        if (employerWallet is null)
            return Result.Failure("کیف پول کارفرما یافت نشد.");

        if (employerWallet.Balance < amount)
            return Result.Failure("موجودی کیف پول کافی نیست.");

        var adminWallet = await GetOrCreateAdminWalletAsync(employerId, cancellationToken);

        employerWallet.DeductFunds(amount);
        _walletRepository.Update(employerWallet);

        adminWallet.Deposit(amount);
        _walletRepository.Update(adminWallet);

        var employerTx = new WalletTransaction(
            employerWallet.Id,
            amount,
            WalletTransactionType.ProjectPayment,
            description,
            relatedProjectId);

        await _transactionRepository.AddAsync(employerTx, cancellationToken);

        var adminTx = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.Deposit,
            description,
            relatedProjectId);

        await _transactionRepository.AddAsync(adminTx, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            employerId,
            "EmployerDepositToEscrow",
            nameof(Wallet),
            adminWallet.Id,
            $"Amount={amount};ProjectId={relatedProjectId}",
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>پرداخت کاربر به حساب امانی ادمین؛ برای خرید خدمات درون‌برنامه‌ای.</summary>
    public async Task<Result> TransferToEscrowAsync(
        int userId,
        decimal amount,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure("کاربر مورد نظر پیدا نشد.");

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
            return Result.Failure("کیف پول یافت نشد. ابتدا کیف پول خود را شارژ کنید.");

        if (!wallet.TryWithdraw(amount))
            return Result.Failure("موجودی کیف پول کافی نیست.");

        _walletRepository.Update(wallet);

        var paymentTransaction = new WalletTransaction(
            wallet.Id,
            amount,
            WalletTransactionType.LiveSessionPayment,
            description,
            relatedEntityId);

        await _transactionRepository.AddAsync(paymentTransaction, cancellationToken);

        var adminWallet = await GetOrCreateAdminWalletAsync(null, cancellationToken);
        adminWallet.Deposit(amount);
        _walletRepository.Update(adminWallet);

        var escrowTransaction = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.Deposit,
            description,
            relatedEntityId);

        await _transactionRepository.AddAsync(escrowTransaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "EscrowDeposit",
            nameof(Wallet),
            wallet.Id,
            $"Amount={amount};EntityId={relatedEntityId}",
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>پرداخت از حساب امانی ادمین به کاربر؛ کارمزد پلتفرم در حساب امانی می‌ماند.</summary>
    public async Task<Result> PayoutFromEscrowAsync(
        int userId,
        decimal amount,
        WalletTransactionType transactionType,
        string description,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure("مبلغ باید بزرگ‌تر از صفر باشد.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure("کاربر مقصد پیدا نشد.");

        var adminWallet = await GetOrCreateAdminWalletAsync(null, cancellationToken);

        if (adminWallet.Balance < amount)
            return Result.Failure("موجودی حساب امانی کافی نیست.");

        var userWallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (userWallet is null)
        {
            userWallet = new Wallet(userId, DetermineOwnerType(user.UserRole));
            await _walletRepository.AddAsync(userWallet, cancellationToken);
        }

        adminWallet.DeductFunds(amount);
        _walletRepository.Update(adminWallet);

        userWallet.AddFunds(amount);
        _walletRepository.Update(userWallet);

        var adminTransaction = new WalletTransaction(
            adminWallet.Id,
            amount,
            WalletTransactionType.PlatformFee,
            $"پرداخت از حساب امانی: {description}",
            relatedEntityId);

        await _transactionRepository.AddAsync(adminTransaction, cancellationToken);

        var userTransaction = new WalletTransaction(
            userWallet.Id,
            amount,
            transactionType,
            description,
            relatedEntityId);

        await _transactionRepository.AddAsync(userTransaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "EscrowPayout",
            nameof(Wallet),
            userWallet.Id,
            $"Amount={amount};EntityId={relatedEntityId};Type={transactionType}",
            null,
            cancellationToken);

        return Result.Success();
    }

    private async Task<Wallet> GetOrCreateAdminWalletAsync(int? adminId, CancellationToken cancellationToken)
    {
        // اولین ادمین سیستم به‌عنوان حساب امانی
        var admins = await _userRepository.GetByRoleAsync(UserRole.Admin, cancellationToken);
        var adminUser = admins.FirstOrDefault();

        if (adminUser is null)
            throw new InvalidOperationException("ادمینی در سیستم یافت نشد. ابتدا ادمین ایجاد کنید.");

        var wallet = await _walletRepository.GetByUserIdAsync(adminUser.Id, cancellationToken);

        if (wallet is null)
        {
            wallet = new Wallet(adminUser.Id, WalletOwnerType.Admin);
            await _walletRepository.AddAsync(wallet, cancellationToken);
            await _walletRepository.SaveChangesAsync(cancellationToken);
        }

        return wallet;
    }

    private static WalletOwnerType DetermineOwnerType(UserRole role) => role switch
    {
        UserRole.Employer => WalletOwnerType.Employer,
        UserRole.Admin => WalletOwnerType.Admin,
        // کارجویان همان برنامه‌نویسان پلتفرم هستند؛ کیف پول آن‌ها از نوع Developer است
        _ => WalletOwnerType.Developer
    };
}