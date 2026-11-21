using System.Security.Cryptography;
using System.Text;
using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Referral;
using CodeBridge.Service.DTOs.Wallet;
using CodeBridge.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace CodeBridge.Service.Implementations;

/// <summary>سیستم معرف: اعمال کد معرف، واریز پاداش و گزارش دعوت‌شدگان.</summary>
public sealed class ReferralService : IReferralService
{
    // کد معرف از حروف و ارقام بدون حروف مبهم ساخته می‌شود
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;

    private readonly IUserRepository _userRepository;
    private readonly IReferralRewardRepository _referralRewardRepository;
    private readonly IWalletService _walletService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly ReferralSettings _settings;

    public ReferralService(
        IUserRepository userRepository,
        IReferralRewardRepository referralRewardRepository,
        IWalletService walletService,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IOptions<ReferralSettings> settings)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _referralRewardRepository = referralRewardRepository
            ?? throw new ArgumentNullException(nameof(referralRewardRepository));
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Result> ApplyReferralCodeAsync(
        int newUserId,
        string referralCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
            return Result.Failure("کد معرف الزامی است.");

        // کاربر باید tracked باشد تا تغییر ReferredById ذخیره شود
        var user = await _userRepository.GetByIdAsync(newUserId, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر مورد نظر پیدا نشد.");

        if (user.ReferredById.HasValue)
            return Result.Failure("کد معرف قبلاً برای این حساب اعمال شده است.");

        var code = referralCode.Trim();

        var referrer = await _userRepository.FirstOrDefaultAsync(
            candidate => candidate.ReferralCode == code,
            cancellationToken);

        if (referrer is null)
            return Result.Failure("کد معرف معتبر نیست.");

        if (referrer.Id == newUserId)
            return Result.Failure("کاربر نمی‌تواند کد معرف خودش را وارد کند.");

        // کاربر تازه هم کد معرف خودش را می‌گیرد تا بتواند دیگران را دعوت کند
        if (string.IsNullOrWhiteSpace(user.ReferralCode))
            user.SetReferralCode(await GenerateUniqueCodeAsync(cancellationToken));

        user.ApplyReferral(referrer.Id);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var rewardAmount = _settings.RewardAmount;

        if (rewardAmount > 0)
        {
            // واریز پاداش به هر دو کیف پول؛ شکست مالی باید در لاگ دیده شود
            await CreditWalletAsync(
                referrer.Id,
                rewardAmount,
                $"پاداش معرف کاربر {user.Id}",
                cancellationToken);

            await CreditWalletAsync(
                newUserId,
                rewardAmount,
                $"پاداش خوش‌آمدگویی از طرف {referrer.FirstName}",
                cancellationToken);

            await _referralRewardRepository.AddAsync(
                new ReferralReward(referrer.Id, newUserId, rewardAmount),
                cancellationToken);

            await _referralRewardRepository.SaveChangesAsync(cancellationToken);
        }

        await _auditLogService.LogAsync(
            newUserId,
            "ReferralApplied",
            nameof(ReferralReward),
            null,
            $"ReferrerId={referrer.Id};Reward={rewardAmount}",
            null,
            cancellationToken);

        // اعلان برای دعوت‌کننده: یک دعوت جدید ثبت شد
        await _notificationService.SendAsync(
            referrer.Id,
            "دعوت جدید با کد معرف شما",
            $"{user.FirstName} {user.LastName} با کد معرف شما ثبت‌نام کرد و {rewardAmount:N0} تومان پاداش گرفتید.",
            NotificationType.Wallet,
            "/jobseeker/wallet",
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ReferralInfoDto>> GetMyReferralInfoAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result<ReferralInfoDto>.Failure("کاربر مورد نظر پیدا نشد.");

        // کاربران قدیمی که کد ندارند، همین لحظه یک کد می‌گیرند
        if (string.IsNullOrWhiteSpace(user.ReferralCode))
        {
            var ensured = await EnsureReferralCodeAsync(userId, cancellationToken);

            if (!ensured.IsSuccess || string.IsNullOrWhiteSpace(ensured.Data))
                return Result<ReferralInfoDto>.Failure(ensured.Message ?? "ساخت کد معرف ممکن نشد.");

            user.SetReferralCode(ensured.Data);
        }

        var referredUsers = await _userRepository.FindAsync(
            candidate => candidate.ReferredById == userId,
            cancellationToken);

        var totalEarned = await _referralRewardRepository.GetTotalEarnedAsync(userId, cancellationToken);

        IReadOnlyList<ReferredUserDto> referred = referredUsers
            .OrderByDescending(candidate => candidate.CreatedAt)
            .Select(candidate => new ReferredUserDto
            {
                UserId = candidate.Id,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                IsApproved = candidate.IsApproved,
                JoinedAt = candidate.CreatedAt
            })
            .ToList();

        return Result<ReferralInfoDto>.Success(new ReferralInfoDto
        {
            ReferralCode = user.ReferralCode,
            RewardAmount = _settings.RewardAmount,
            TotalEarned = totalEarned,
            ReferredUsers = referred
        });
    }

    public async Task<Result<string>> EnsureReferralCodeAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر مورد نظر پیدا نشد.");

        if (!string.IsNullOrWhiteSpace(user.ReferralCode))
            return Result<string>.Success(user.ReferralCode);

        var code = await GenerateUniqueCodeAsync(cancellationToken);

        user.SetReferralCode(code);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(user.ReferralCode!);
    }

    /// <summary>واریز پاداش به کیف پول؛ خطای مالی نباید ثبت دعوت را متوقف کند.</summary>
    private async Task CreditWalletAsync(
        int userId,
        decimal amount,
        string description,
        CancellationToken cancellationToken)
    {
        try
        {
            await _walletService.DepositAsync(
                userId,
                new DepositDto { Amount = amount, Description = description },
                cancellationToken);
        }
        catch (Exception)
        {
            // کیف پول در دسترس نیست؛ دعوت ثبت شده و مبلغ بعداً قابل بازپرداخت است
        }
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = GenerateCode();

            var exists = await _userRepository.AnyAsync(
                user => user.ReferralCode == candidate,
                cancellationToken);

            if (!exists)
                return candidate;
        }

        // با وجود احتمال بسیار کم تکرار، یک پسوند زمانی اضافه می‌شود
        return GenerateCode() + DateTime.UtcNow.Second.ToString("D2");
    }

    private static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(CodeLength);
        var builder = new StringBuilder(CodeLength);

        foreach (var value in bytes)
            builder.Append(CodeAlphabet[value % CodeAlphabet.Length]);

        return builder.ToString();
    }
}
