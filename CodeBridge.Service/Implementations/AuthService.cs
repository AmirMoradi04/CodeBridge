using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Auth;
using CodeBridge.Service.DTOs.Resume;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق احراز هویت با Access Token و چرخش Refresh Token.</summary>
public sealed class AuthService : IAuthService
{
    /// <summary>حجم مجاز فایل رزومه در زمان ثبت‌نام (۱۰ مگابایت).</summary>
    private const long MaxResumeFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedResumeContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    /// <summary>تعداد هش‌های اخیر که برای جلوگیری از تکرار رمز بررسی می‌شود.</summary>
    private const int PasswordHistoryCheckCount = 3;

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IResumeService _resumeService;
    private readonly IReferralService _referralService;
    private readonly IAuditLogService _auditLogService;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAttachmentService attachmentService,
        IResumeService resumeService,
        IReferralService referralService,
        IAuditLogService auditLogService,
        IPasswordHistoryRepository passwordHistoryRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        UserManager<User> userManager,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _resumeService = resumeService ?? throw new ArgumentNullException(nameof(resumeService));
        _referralService = referralService ?? throw new ArgumentNullException(nameof(referralService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _passwordHistoryRepository = passwordHistoryRepository
            ?? throw new ArgumentNullException(nameof(passwordHistoryRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _refreshTokenGenerator = refreshTokenGenerator ?? throw new ArgumentNullException(nameof(refreshTokenGenerator));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _securitySettings = securitySettings?.Value ?? throw new ArgumentNullException(nameof(securitySettings));
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterDto dto,
        ResumeUpload? resumeUpload = null,
        string? ip = null,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<AuthResponseDto>.Failure("اطلاعات ثبت‌نام الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return Result<AuthResponseDto>.Failure("نام و نام خانوادگی الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return Result<AuthResponseDto>.Failure("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        // ثبت‌نام عمومی فقط برای کارجو و کارفرماست؛ ادمین از DbInitializer ساخته می‌شود
        // و برنامه‌نویس فقط از سمت ادمین مدیریت می‌شود.
        if (dto.UserRole is not (UserRole.JobSeeker or UserRole.Employer))
            return Result<AuthResponseDto>.Failure("ثبت‌نام فقط برای کارجو و کارفرما مجاز است.");

        // رزومه برای کارجو اجباری است؛ حساب بدون فایل ساخته نمی‌شود
        if (dto.UserRole == UserRole.JobSeeker)
        {
            if (resumeUpload is null || string.IsNullOrWhiteSpace(resumeUpload.FileName))
                return Result<AuthResponseDto>.Failure("بارگذاری فایل رزومه برای ثبت‌نام کارجو الزامی است.");

            if (!AllowedResumeContentTypes.Contains(resumeUpload.ContentType))
                return Result<AuthResponseDto>.Failure("فایل رزومه باید PDF یا DOCX باشد.");

            if (resumeUpload.Content.CanSeek && resumeUpload.Content.Length == 0)
                return Result<AuthResponseDto>.Failure("فایل رزومه خالی است.");

            if (resumeUpload.Content.CanSeek && resumeUpload.Content.Length > MaxResumeFileSize)
                return Result<AuthResponseDto>.Failure("حجم فایل رزومه نباید بیشتر از ۱۰ مگابایت باشد.");
        }

        var email = NormalizeEmail(dto.Email);

        if (email is null)
            return Result<AuthResponseDto>.Failure("ایمیل معتبر الزامی است.");

        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            return Result<AuthResponseDto>.Failure("کاربری با این ایمیل قبلاً ثبت شده است.");

        var user = new User(dto.FirstName, dto.LastName, email, dto.UserRole)
        {
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = _passwordHasher.Hash(dto.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // رمز اولیه در تاریخچه ثبت می‌شود تا استفاده‌ی مجدد از آن در آینده ممنوع باشد
        await AppendPasswordHistoryAsync(user.Id, user.PasswordHash ?? string.Empty, cancellationToken);

        // حساب جدید تا تایید ادمین غیرفعال می‌ماند (LoginAsync هم این شرط را بررسی می‌کند)

        // رزومه‌ی کارجو همراه با ثبت‌نام ذخیره می‌شود تا پیش از تایید هم در دسترس ادمین باشد
        if (dto.UserRole == UserRole.JobSeeker && resumeUpload is not null)
        {
            var attachmentResult = await _attachmentService.UploadAsync(
                user.Id,
                resumeUpload.Content,
                resumeUpload.FileName,
                resumeUpload.ContentType,
                "Resume",
                null,
                cancellationToken);

            if (!attachmentResult.IsSuccess || attachmentResult.Data is null)
            {
                // کاربر نیمه‌ساخته حذف می‌شود تا ایمیل برای تلاش دوباره آزاد بماند
                _userRepository.Remove(user);
                await _userRepository.SaveChangesAsync(cancellationToken);

                return Result<AuthResponseDto>.Failure(
                    attachmentResult.Message ?? "آپلود فایل رزومه ناموفق بود.");
            }

            var resumeResult = await _resumeService.CreateOrUpdateAsync(
                user.Id,
                new CreateResumeDto
                {
                    Title = $"رزومه‌ی {dto.FirstName} {dto.LastName}".Trim(),
                    Summary = "فایل رزومه در زمان ثبت‌نام بارگذاری شده است.",
                    Skills = string.Empty,
                    AttachmentId = attachmentResult.Data.Id
                },
                cancellationToken);

            if (!resumeResult.IsSuccess)
            {
                // فایل و کاربر نیمه‌ساخته پاک می‌شوند تا ثبت‌نام قابل تکرار بماند
                await _attachmentService.DeleteAsync(
                    attachmentResult.Data.Id,
                    user.Id,
                    isAdmin: false,
                    cancellationToken);

                _userRepository.Remove(user);
                await _userRepository.SaveChangesAsync(cancellationToken);

                return Result<AuthResponseDto>.Failure(
                    resumeResult.Message ?? "ثبت رزومه ناموفق بود.");
            }
        }

        // کد معرف (در صورت ارسال) باعث ثبت دعوت و واریز پاداش به هر دو طرف می‌شود.
        // کد نامعتبر مانع ثبت‌نام نمی‌شود چون ورودی اختیاری است.
        if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
            await _referralService.ApplyReferralCodeAsync(
                user.Id,
                dto.ReferralCode,
                cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "UserRegistered",
            nameof(User),
            user.Id,
            $"ثبت‌نام با نقش {user.UserRole}",
            ip,
            cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = user.UserRole
        });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<AuthResponseDto>.Failure("ایمیل و رمز عبور الزامی است.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            return Result<AuthResponseDto>.Failure("اطلاعات ورود نامعتبر است.");

        var utcNow = DateTime.UtcNow;

        // حساب قفل‌شده تا پایان دوره‌ی قفل حتی با رمز درست هم پذیرفته نمی‌شود
        if (user.IsLockedOut(utcNow))
            return Result<AuthResponseDto>.Failure(BuildLockoutMessage(user, utcNow));

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash ?? string.Empty))
        {
            // تلاش ناموفق ثبت می‌شود و در صورت رسیدن به سقف، حساب موقتاً قفل می‌شود
            user.RegisterFailedLogin(
                _securitySettings.MaxFailedLoginAttempts,
                TimeSpan.FromMinutes(_securitySettings.LockoutMinutes),
                utcNow);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                user.Id,
                "UserLoginFailed",
                nameof(User),
                user.Id,
                null,
                ip,
                cancellationToken);

            return Result<AuthResponseDto>.Failure(
                user.IsLockedOut(utcNow)
                    ? BuildLockoutMessage(user, utcNow)
                    : "اطلاعات ورود نامعتبر است.");
        }

        // ورود موفق شمارنده‌ی تلاش‌های ناموفق را پاک می‌کند
        if (user.AccessFailedCount > 0 || user.LockoutEnd is not null)
        {
            user.ResetFailedLogin();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        if (!user.IsApproved)
            return Result<AuthResponseDto>.Failure("حساب کاربری هنوز تایید نشده است.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("حساب کاربری غیرفعال است.");

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, await GetRoleNamesAsync(user, cancellationToken));
        var refreshTokenValue = _refreshTokenGenerator.Generate();
        var refreshTokenExpiresAt = _refreshTokenGenerator.GetExpiryDate();

        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, refreshTokenExpiresAt, ip);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "UserLoggedIn",
            nameof(User),
            user.Id,
            null,
            ip,
            cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = user.UserRole
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return Result<AuthResponseDto>.Failure("توکن تازه‌سازی الزامی است.");

        var currentToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);

        if (currentToken is null || !currentToken.IsActive)
            return Result<AuthResponseDto>.Failure("توکن تازه‌سازی نامعتبر یا منقضی شده است.");

        var user = await _userRepository.GetByIdAsync(currentToken.UserId, cancellationToken);

        if (user is null)
            return Result<AuthResponseDto>.Failure("کاربر متعلق به توکن پیدا نشد.");

        // حساب قفل‌شده نباید با Refresh Token هم توکن تازه بگیرد
        if (user.IsLockedOut(DateTime.UtcNow))
            return Result<AuthResponseDto>.Failure("حساب کاربری موقتاً قفل شده است.");

        if (!user.IsApproved)
            return Result<AuthResponseDto>.Failure("حساب کاربری هنوز تایید نشده است.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("حساب کاربری غیرفعال است.");

        // چرخش توکن: توکن قبلی باطل و توکن جدید صادر می‌شود
        var newRefreshTokenValue = _refreshTokenGenerator.Generate();
        var newExpiresAt = _refreshTokenGenerator.GetExpiryDate();

        currentToken.Revoke(ip, newRefreshTokenValue);
        _refreshTokenRepository.Update(currentToken);

        var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenValue, newExpiresAt, ip);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, await GetRoleNamesAsync(user, cancellationToken));

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = newExpiresAt,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = user.UserRole
        });
    }

    public async Task<Result> LogoutAsync(
        LogoutRequestDto dto,
        string? ip = null,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return Result.Failure("توکن تازه‌سازی الزامی است.");

        var token = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);

        if (token is not null)
        {
            token.Revoke(ip);
            _refreshTokenRepository.Update(token);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                token.UserId,
                "UserLoggedOut",
                nameof(RefreshToken),
                token.Id,
                null,
                ip,
                cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ApproveUserAsync(
        int userId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر مورد نظر پیدا نشد.");

        await _userRepository.ApproveUserAsync(userId, cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "UserApproved",
            nameof(User),
            userId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectUserAsync(
        int userId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر مورد نظر پیدا نشد.");

        await _userRepository.RejectUserAsync(userId, cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "UserRejected",
            nameof(User),
            userId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// ارسال توکن بازیابی به ایمیل کاربر. برای جلوگیری از افشای وجود حساب،
    /// در نبود کاربر هم پاسخ موفق برمی‌گرداند.
    /// </summary>
    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
            return Result.Failure("ایمیل الزامی است.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            return Result.Success();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = $"/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendAsync(
            email,
            "بازیابی رمز عبور CodeBridge",
            $"برای تنظیم رمز جدید، از این لینک استفاده کنید: {resetLink}{Environment.NewLine}توکن بازیابی: {token}",
            cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "PasswordResetRequested",
            nameof(User),
            user.Id,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>بازنشانی رمز عبور با توکن بازیابی؛ رمزهای اخیر قابل استفاده مجدد نیستند.</summary>
    public async Task<Result> ResetPasswordAsync(
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null
            || string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Token)
            || string.IsNullOrWhiteSpace(dto.NewPassword))
            return Result.Failure("ایمیل، توکن و رمز جدید الزامی است.");

        if (dto.NewPassword.Length < 6)
            return Result.Failure("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر یافت نشد.");

        // صحت‌سنجی توکن تولیدشده توسط Identity
        var isValidToken = await _userManager.VerifyUserTokenAsync(
            user,
            TokenOptions.DefaultEmailProvider,
            "ResetPassword",
            dto.Token);

        if (!isValidToken)
            return Result.Failure("توکن بازیابی نامعتبر یا منقضی شده است.");

        if (await IsPasswordReusedAsync(user.Id, dto.NewPassword, cancellationToken))
            return Result.Failure("این رمز عبور اخیراً استفاده شده است؛ رمز دیگری انتخاب کنید.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await AppendPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "PasswordReset",
            nameof(User),
            user.Id,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>تغییر رمز عبور توسط کاربر واردشده؛ رمزهای اخیر قابل استفاده مجدد نیستند.</summary>
    public async Task<Result> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null
            || string.IsNullOrWhiteSpace(dto.CurrentPassword)
            || string.IsNullOrWhiteSpace(dto.NewPassword))
            return Result.Failure("رمز فعلی و رمز جدید الزامی است.");

        if (dto.NewPassword.Length < 6)
            return Result.Failure("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        if (dto.CurrentPassword == dto.NewPassword)
            return Result.Failure("رمز جدید باید با رمز فعلی متفاوت باشد.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر یافت نشد.");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash ?? string.Empty))
            return Result.Failure("رمز عبور فعلی نادرست است.");

        if (await IsPasswordReusedAsync(user.Id, dto.NewPassword, cancellationToken))
            return Result.Failure("این رمز عبور اخیراً استفاده شده است؛ رمز دیگری انتخاب کنید.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await AppendPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "PasswordChanged",
            nameof(User),
            user.Id,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>بررسی اینکه رمز پیشنهادی در بین N هش اخیر کاربر نباشد.</summary>
    private async Task<bool> IsPasswordReusedAsync(
        int userId,
        string candidatePassword,
        CancellationToken cancellationToken)
    {
        var recent = await _passwordHistoryRepository.GetRecentAsync(
            userId,
            PasswordHistoryCheckCount,
            cancellationToken);

        foreach (var history in recent)
        {
            if (_passwordHasher.Verify(candidatePassword, history.PasswordHash))
                return true;
        }

        return false;
    }

    /// <summary>ثبت هش رمز در تاریخچه‌ی رمزهای کاربر.</summary>
    private async Task AppendPasswordHistoryAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return;

        await _passwordHistoryRepository.AddAsync(new PasswordHistory(userId, passwordHash), cancellationToken);
        await _passwordHistoryRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result> ValidateAdminAsync(int adminId, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);

        if (admin is null)
            return Result.Failure("ادمین مورد نظر پیدا نشد.");

        if (admin.UserRole != UserRole.Admin)
            return Result.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");

        return Result.Success();
    }

    private async Task<IEnumerable<string>> GetRoleNamesAsync(User user, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByNameAsync(user.UserRole.ToString(), cancellationToken);

        return new[] { role?.Name ?? user.UserRole.ToString() };
    }

    /// <summary>پیام قفل با ذکر دقیقه‌های باقی‌مانده.</summary>
    private static string BuildLockoutMessage(User user, DateTime utcNow)
    {
        var remainingMinutes = user.LockoutEnd.HasValue
            ? Math.Max(1, (int)Math.Ceiling((user.LockoutEnd.Value.UtcDateTime - utcNow).TotalMinutes))
            : 0;

        return $"حساب کاربری موقتاً قفل شده است؛ {remainingMinutes} دقیقه دیگر تلاش کنید.";
    }

    private static string? NormalizeEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}
