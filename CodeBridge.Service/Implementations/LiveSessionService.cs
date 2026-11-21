using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.LiveSession;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>جلسه‌های زنده: ساخت توسط ارشد، خرید کارجو و تسویه پس از پایان.</summary>
public sealed class LiveSessionService : ILiveSessionService
{
    /// <summary>سهم پلتفرم از مبلغ هر جلسه؛ مابقی به برگزارکننده پرداخت می‌شود.</summary>
    private const decimal PlatformFeePercent = 0.10m;

    private const int MinimumDurationMinutes = 5;
    private const int MaximumDurationMinutes = 600;

    private readonly ILiveSessionRepository _sessionRepository;
    private readonly ILiveSessionParticipantRepository _participantRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperLevelRepository _developerLevelRepository;
    private readonly IWalletService _walletService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public LiveSessionService(
        ILiveSessionRepository sessionRepository,
        ILiveSessionParticipantRepository participantRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IDeveloperLevelRepository developerLevelRepository,
        IWalletService walletService,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _sessionRepository = sessionRepository
            ?? throw new ArgumentNullException(nameof(sessionRepository));
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
        _projectRepository = projectRepository
            ?? throw new ArgumentNullException(nameof(projectRepository));
        _userRepository = userRepository
            ?? throw new ArgumentNullException(nameof(userRepository));
        _developerLevelRepository = developerLevelRepository
            ?? throw new ArgumentNullException(nameof(developerLevelRepository));
        _walletService = walletService
            ?? throw new ArgumentNullException(nameof(walletService));
        _auditLogService = auditLogService
            ?? throw new ArgumentNullException(nameof(auditLogService));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<LiveSessionDto>> CreateSessionAsync(
        int seniorId,
        CreateLiveSessionDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<LiveSessionDto>.Failure("عنوان جلسه الزامی است.");

        if (dto.Price < 0)
            return Result<LiveSessionDto>.Failure("قیمت جلسه نمی‌تواند منفی باشد.");

        if (dto.DurationMinutes is < MinimumDurationMinutes or > MaximumDurationMinutes)
            return Result<LiveSessionDto>.Failure(
                $"مدت جلسه باید بین {MinimumDurationMinutes} تا {MaximumDurationMinutes} دقیقه باشد.");

        if (!IsHttpUrl(dto.MeetingLink))
            return Result<LiveSessionDto>.Failure("لینک جلسه باید یک آدرس معتبر http یا https باشد.");

        var scheduledAt = NormalizeToUtc(dto.ScheduledAt);

        if (scheduledAt <= DateTime.UtcNow.AddMinutes(-1))
            return Result<LiveSessionDto>.Failure("زمان جلسه باید در آینده باشد.");

        var senior = await _userRepository.GetByIdAsync(seniorId, cancellationToken);

        if (senior is null)
            return Result<LiveSessionDto>.Failure("برنامه‌نویس پیدا نشد.");

        // کارجویان همان برنامه‌نویسان پلتفرم هستند
        if (senior.UserRole != UserRole.JobSeeker)
            return Result<LiveSessionDto>.Failure("فقط برنامه‌نویس می‌تواند جلسه‌ی زنده بسازد.");

        // قاعده‌ی کسب‌وکار: فقط برنامه‌نویس سطح Senior مجاز به برگزاری است
        var level = await _developerLevelRepository.GetByUserIdAsync(seniorId, cancellationToken);

        if (level is null || level.Level != DeveloperLevelType.Senior)
            return Result<LiveSessionDto>.Failure(
                "فقط برنامه‌نویسان سطح Senior می‌توانند جلسه‌ی زنده برگزار کنند.");

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, cancellationToken);

        if (project is null)
            return Result<LiveSessionDto>.Failure("پروژه پیدا نشد.");

        if (project.AssignedDeveloperId != seniorId)
            return Result<LiveSessionDto>.Failure(
                "شما به این پروژه تخصیص نیافته‌اید و نمی‌توانید برای آن جلسه بسازید.");

        LiveSession session;

        try
        {
            session = new LiveSession(
                project.Id,
                seniorId,
                dto.Title,
                dto.Description,
                dto.Price,
                scheduledAt,
                dto.DurationMinutes,
                dto.MeetingLink);
        }
        catch (InvalidOperationException exception)
        {
            return Result<LiveSessionDto>.Failure(exception.Message);
        }

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            seniorId,
            "LiveSessionCreated",
            nameof(LiveSession),
            session.Id,
            $"ProjectId={project.Id};Price={session.Price}",
            null,
            cancellationToken);

        return Result<LiveSessionDto>.Success(
            LiveSessionMapper.ToDto(session, isJoined: false, includeLinks: true));
    }

    public async Task<Result<string>> JoinSessionAsync(
        int sessionId,
        int juniorId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId, cancellationToken);

        if (session is null)
            return Result<string>.Failure("جلسه پیدا نشد.");

        if (session.Status is not (LiveSessionStatus.Scheduled or LiveSessionStatus.Live))
            return Result<string>.Failure("این جلسه قابل ثبت‌نام نیست.");

        var junior = await _userRepository.GetByIdAsync(juniorId, cancellationToken);

        if (junior is null)
            return Result<string>.Failure("کارجو پیدا نشد.");

        if (junior.UserRole != UserRole.JobSeeker)
            return Result<string>.Failure("فقط کارجو می‌تواند در جلسه‌ها شرکت کند.");

        var existing = await _participantRepository.GetBySessionAndJuniorAsync(
            sessionId,
            juniorId,
            cancellationToken);

        if (existing is not null)
            return Result<string>.Failure("شما قبلاً در این جلسه ثبت‌نام کرده‌اید.");

        // پرداخت به حساب امانی؛ اگر قیمت صفر باشد نیازی به تراکنش نیست
        if (session.Price > 0)
        {
            var payment = await _walletService.TransferToEscrowAsync(
                juniorId,
                session.Price,
                $"پرداخت جلسه‌ی زنده: {session.Title}",
                session.Id,
                cancellationToken);

            if (!payment.IsSuccess)
                return Result<string>.Failure(payment.Message ?? "پرداخت ناموفق بود.");
        }

        var participant = new LiveSessionParticipant(session.Id, juniorId);
        await _participantRepository.AddAsync(participant, cancellationToken);
        await _participantRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            juniorId,
            "LiveSessionJoined",
            nameof(LiveSessionParticipant),
            participant.Id,
            $"SessionId={session.Id};Amount={session.Price}",
            null,
            cancellationToken);

        await _notificationService.SendAsync(
            session.SeniorId,
            "شرکت‌کننده‌ی جدید در جلسه‌ی زنده",
            $"{junior.FirstName} {junior.LastName} در جلسه‌ی «{session.Title}» ثبت‌نام کرد.",
            NotificationType.Project,
            "/developer/live-sessions",
            cancellationToken);

        // لینک جلسه فقط در پاسخ موفق پرداخت‌کننده برگردانده می‌شود
        return Result<string>.Success(session.MeetingLink);
    }

    public async Task<Result> EndSessionAsync(
        int sessionId,
        int seniorId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId, cancellationToken);

        if (session is null)
            return Result.Failure("جلسه پیدا نشد.");

        if (session.SeniorId != seniorId)
            return Result.Failure("فقط برگزارکننده‌ی جلسه می‌تواند آن را پایان دهد.");

        if (session.Status is LiveSessionStatus.Ended or LiveSessionStatus.Cancelled)
            return Result.Failure("این جلسه قابل پایان‌دادن نیست.");

        var participantCount = session.Participants?.Count ?? 0;
        var grossAmount = session.Price * participantCount;
        var platformFee = Math.Round(grossAmount * PlatformFeePercent, 2);
        var payout = grossAmount - platformFee;

        if (payout > 0)
        {
            var payment = await _walletService.PayoutFromEscrowAsync(
                seniorId,
                payout,
                WalletTransactionType.LiveSessionPayment,
                $"تسویه‌ی جلسه‌ی زنده: {session.Title}",
                session.Id,
                cancellationToken);

            if (!payment.IsSuccess)
                return Result.Failure("تسویه‌ی جلسه ناموفق بود: " + payment.Message);
        }

        try
        {
            session.End();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(exception.Message);
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            seniorId,
            "LiveSessionEnded",
            nameof(LiveSession),
            session.Id,
            $"Participants={participantCount};Gross={grossAmount};Payout={payout}",
            null,
            cancellationToken);

        // اطلاع پایان جلسه به شرکت‌کننده‌ها
        foreach (var participant in session.Participants ?? Array.Empty<LiveSessionParticipant>())
        {
            await _notificationService.SendAsync(
                participant.JuniorId,
                "جلسه‌ی زنده پایان یافت",
                $"جلسه‌ی «{session.Title}» به پایان رسید.",
                NotificationType.Project,
                "/jobseeker/live-sessions",
                cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> UpdateRecordingLinkAsync(
        int sessionId,
        int seniorId,
        string recordingLink,
        CancellationToken cancellationToken = default)
    {
        if (!IsHttpUrl(recordingLink))
            return Result.Failure("لینک ضبط باید یک آدرس معتبر http یا https باشد.");

        var session = await _sessionRepository.GetWithDetailsAsync(sessionId, cancellationToken);

        if (session is null)
            return Result.Failure("جلسه پیدا نشد.");

        if (session.SeniorId != seniorId)
            return Result.Failure("فقط برگزارکننده‌ی جلسه می‌تواند لینک ضبط را ثبت کند.");

        if (session.Status != LiveSessionStatus.Ended)
            return Result.Failure("لینک ضبط فقط بعد از پایان جلسه ثبت می‌شود.");

        try
        {
            session.SetRecordingLink(recordingLink);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(exception.Message);
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            seniorId,
            "LiveSessionRecordingSet",
            nameof(LiveSession),
            session.Id,
            recordingLink,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<LiveSessionDto>>> GetAvailableSessionsAsync(
        int juniorId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAvailableAsync(cancellationToken);

        var dtos = sessions
            .Select(session =>
            {
                var isJoined = session.Participants?.Any(
                    participant => participant.JuniorId == juniorId) ?? false;

                // بدون پرداخت، لینک جلسه و ضبط هرگز در پاسخ قرار نمی‌گیرد
                return LiveSessionMapper.ToDto(session, isJoined, includeLinks: isJoined);
            })
            .ToList();

        return Result<IReadOnlyList<LiveSessionDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<LiveSessionDto>>> GetSeniorSessionsAsync(
        int seniorId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetBySeniorIdAsync(seniorId, cancellationToken);

        var dtos = sessions
            .Select(session => LiveSessionMapper.ToDto(session, isJoined: false, includeLinks: true))
            .ToList();

        return Result<IReadOnlyList<LiveSessionDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<LiveSessionDto>>> GetAdminSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllForAdminAsync(cancellationToken);

        var dtos = sessions
            .Select(session => LiveSessionMapper.ToDto(session, isJoined: false, includeLinks: true))
            .ToList();

        return Result<IReadOnlyList<LiveSessionDto>>.Success(dtos);
    }

    public async Task<Result<LiveSessionDto>> GetSessionDetailsAsync(
        int sessionId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId, cancellationToken);

        if (session is null)
            return Result<LiveSessionDto>.Failure("جلسه پیدا نشد.");

        var isAdmin = string.Equals(role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase);
        var isSenior = session.SeniorId == userId;
        var isParticipant = session.Participants?.Any(
            participant => participant.JuniorId == userId) ?? false;

        var includeLinks = isAdmin || isSenior || isParticipant;

        return Result<LiveSessionDto>.Success(
            LiveSessionMapper.ToDto(session, isParticipant, includeLinks));
    }

    private static DateTime NormalizeToUtc(DateTime value)
        => value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

    private static bool IsHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
