using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.DailyProject;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>سرویس پروژه‌های روزمره و تسک‌ها.</summary>
public sealed class DailyProjectService : IDailyProjectService
{
    private readonly IDailyProjectRepository _projectRepository;
    private readonly IDailyProjectApplicationRepository _applicationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWalletService _walletService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IAchievementService _achievementService;

    public DailyProjectService(
        IDailyProjectRepository projectRepository,
        IDailyProjectApplicationRepository applicationRepository,
        IUserRepository userRepository,
        IWalletService walletService,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IAchievementService achievementService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _achievementService = achievementService ?? throw new ArgumentNullException(nameof(achievementService));
    }

    /// <summary>مسیر صفحه‌ی تسک‌ها در فرانت بر اساس نقش کاربر.</summary>
    private static string DailyTasksLink(UserRole role)
        => role == UserRole.JobSeeker ? "/jobseeker/daily-projects" : "/developer";

    public async Task<Result<DailyProjectDto>> CreateAsync(int employerId, CreateDailyProjectDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<DailyProjectDto>.Failure("عنوان پروژه الزامی است.");

        if (dto.TotalBudget <= 0 || dto.PricePerUnit <= 0 || dto.Capacity <= 0)
            return Result<DailyProjectDto>.Failure("بودجه، قیمت واحد و ظرفیت باید بزرگ‌تر از صفر باشند.");

        if (dto.DeadlineSeconds <= 0)
            return Result<DailyProjectDto>.Failure("مهلت تکمیل باید بزرگ‌تر از صفر باشد.");

        var employer = await _userRepository.GetByIdAsync(employerId, cancellationToken);
        if (employer is null)
            return Result<DailyProjectDto>.Failure("کارفرما پیدا نشد.");

        if (employer.UserRole != UserRole.Employer)
            return Result<DailyProjectDto>.Failure("فقط کارفرما می‌تواند پروژه‌ی روزمره ثبت کند.");

        var project = new DailyProject(
            employerId,
            dto.Title,
            dto.Description,
            dto.TotalBudget,
            dto.PricePerUnit,
            dto.Capacity,
            dto.DeadlineSeconds,
            dto.Instructions);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            employerId,
            "DailyProjectCreated",
            nameof(DailyProject),
            project.Id,
            null,
            null,
            cancellationToken);

        return Result<DailyProjectDto>.Success(DailyProjectMapper.ToDto(project));
    }

    public async Task<Result<DailyProjectDto>> GetByIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithApplicationsAsync(projectId, cancellationToken);

        if (project is null)
            return Result<DailyProjectDto>.Failure("پروژه پیدا نشد.");

        return Result<DailyProjectDto>.Success(DailyProjectMapper.ToDto(project));
    }

    public async Task<Result<IReadOnlyList<DailyProjectDto>>> GetByEmployerAsync(int employerId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetByEmployerIdAsync(employerId, cancellationToken);
        return Result<IReadOnlyList<DailyProjectDto>>.Success(projects.Select(DailyProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DailyProjectDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetActiveForDevelopersAsync(cancellationToken);
        return Result<IReadOnlyList<DailyProjectDto>>.Success(projects.Select(DailyProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DailyProjectDto>>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetPendingAsync(cancellationToken);
        return Result<IReadOnlyList<DailyProjectDto>>.Success(projects.Select(DailyProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DailyProjectDto>>> GetApprovedAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetApprovedAsync(cancellationToken);
        return Result<IReadOnlyList<DailyProjectDto>>.Success(projects.Select(DailyProjectMapper.ToDto).ToList());
    }

    public async Task<Result> ApproveAsync(int projectId, int adminId, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.Status != DailyProjectStatus.Pending)
            return Result.Failure("فقط پروژه‌های در انتظار تایید قابل تایید هستند.");

        project.Approve(adminId);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(adminId, "DailyProjectApproved", nameof(DailyProject), projectId, null, null, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(int projectId, int adminId, string note, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        if (string.IsNullOrWhiteSpace(note))
            return Result.Failure("دلیل رد الزامی است.");

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.Status != DailyProjectStatus.Pending)
            return Result.Failure("فقط پروژه‌های در انتظار تایید قابل رد هستند.");

        project.Reject(adminId, note);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(adminId, "DailyProjectRejected", nameof(DailyProject), projectId, note, null, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int projectId, int adminId, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.Status != DailyProjectStatus.Approved)
            return Result.Failure("فقط پروژه‌های تاییدشده قابل فعال‌سازی هستند.");

        if (!project.IsPaid)
            return Result.Failure("پروژه باید کاملاً پرداخت شده باشد.");

        try
        {
            project.Activate();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی.</summary>
    public async Task<Result> DepositFundsAsync(int projectId, int employerId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.EmployerId != employerId)
            return Result.Failure("فقط کارفرمای این پروژه می‌تواند واریز انجام دهد.");

        if (project.Status != DailyProjectStatus.Approved)
            return Result.Failure("فقط پروژه‌های تاییدشده قابل واریز هستند.");

        // بررسی موجودی کیف پول کارفرما
        var walletResult = await _walletService.GetByUserIdAsync(employerId, cancellationToken);

        if (!walletResult.IsSuccess)
            return Result.Failure("کیف پول کارفرما یافت نشد.");

        if (walletResult.Data!.Balance < project.TotalBudget)
            return Result.Failure("موجودی کیف پول کافی نیست. لطفاً ابتدا کیف پول خود را شارژ کنید.");

        // واریز از کیف پول کارفرما به حساب امانی
        var depositResult = await _walletService.EmployerDepositToEscrowAsync(
            employerId,
            project.TotalBudget,
            projectId,
            $"واریز بودجه‌ی پروژه روزمره: {project.Title}",
            cancellationToken);

        if (!depositResult.IsSuccess)
            return Result.Failure("واریز ناموفق بود: " + depositResult.Message);

        // ثبت پرداخت در پروژه
        project.RegisterPayment(project.TotalBudget);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            employerId,
            "DailyProjectDepositFunds",
            nameof(DailyProject),
            projectId,
            $"Amount={project.TotalBudget}",
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<DailyApplicationDto>> ApplyAsync(int developerId, int projectId, CancellationToken cancellationToken = default)
    {
        var developer = await _userRepository.GetByIdAsync(developerId, cancellationToken);
        if (developer is null)
            return Result<DailyApplicationDto>.Failure("توسعه‌دهنده پیدا نشد.");

        // کارجویان همان برنامه‌نویسان پلتفرم هستند
        if (developer.UserRole != UserRole.JobSeeker)
            return Result<DailyApplicationDto>.Failure("فقط برنامه‌نویس می‌تواند درخواست دهد.");

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<DailyApplicationDto>.Failure("پروژه پیدا نشد.");

        if (project.Status != DailyProjectStatus.Active)
            return Result<DailyApplicationDto>.Failure("فقط پروژه‌های فعال قابل درخواست هستند.");

        if (!project.HasCapacity())
            return Result<DailyApplicationDto>.Failure("ظرفیت پروژه تکمیل شده است.");

        var existing = await _applicationRepository.GetByProjectAndDeveloperAsync(projectId, developerId, cancellationToken);

        if (existing is not null)
            return Result<DailyApplicationDto>.Failure("شما قبلاً در این پروژه درخواست داده‌اید.");

        var application = new DailyProjectApplication(projectId, developerId);
        await _applicationRepository.AddAsync(application, cancellationToken);

        project.IncrementFilledSlots();
        _projectRepository.Update(project);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            developerId,
            "DailyProjectApplied",
            nameof(DailyProjectApplication),
            application.Id,
            $"ProjectId={projectId}",
            null,
            cancellationToken);

        // اعلان درخواست جدید برای کارفرمای پروژه
        await _notificationService.SendAsync(
            project.EmployerId,
            "درخواست جدید برای تسک روزمره",
            $"یک کاربر برای تسک «{project.Title}» درخواست داد.",
            NotificationType.Project,
            "/employer/daily-projects",
            cancellationToken);

        return Result<DailyApplicationDto>.Success(DailyProjectMapper.ToApplicationDto(application));
    }

    public async Task<Result> StartTaskAsync(int developerId, StartTaskDto dto, CancellationToken cancellationToken = default)
    {
        var application = await _applicationRepository.GetByProjectAndDeveloperAsync(dto.DailyProjectId, developerId, cancellationToken);
        if (application is null)
            return Result.Failure("درخواستی برای این پروژه پیدا نشد.");

        if (application.Status != DailyApplicationStatus.Pending)
            return Result.Failure("این درخواست در وضعیت قابل شروع نیست.");

        var project = await _projectRepository.GetByIdAsync(dto.DailyProjectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        application.Start(project.DeadlineSeconds);
        _applicationRepository.Update(application);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SubmitTaskAsync(int developerId, SubmitTaskDto dto, CancellationToken cancellationToken = default)
    {
        var application = await _applicationRepository.GetByProjectAndDeveloperAsync(dto.DailyProjectId, developerId, cancellationToken);
        if (application is null)
            return Result.Failure("درخواستی برای این پروژه پیدا نشد.");

        if (application.Status != DailyApplicationStatus.InProgress)
            return Result.Failure("این درخواست باید در وضعیت در حال انجام باشد.");

        application.Submit(dto.ProofAttachmentId);
        _applicationRepository.Update(application);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveTaskAsync(int applicationId, int adminId, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        var app = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (app is null)
            return Result.Failure("درخواست پیدا نشد.");

        if (app.Status != DailyApplicationStatus.Submitted)
            return Result.Failure("فقط درخواست‌های ارسال‌شده قابل تایید هستند.");

        app.Approve(adminId);
        _applicationRepository.Update(app);

        // پرداخت به توسعه‌دهنده از حساب امانی
        var project = await _projectRepository.GetByIdAsync(app.DailyProjectId, cancellationToken)
            ?? throw new InvalidOperationException("پروژه پیدا نشد.");

        var paymentResult = await _walletService.PayDeveloperFromEscrowAsync(
            adminId,
            app.DeveloperId,
            project.PricePerUnit,
            WalletTransactionType.DailyTaskPayment,
            $"پرداخت تسک روزمره: {project.Title}",
            project.Id,
            cancellationToken);

        if (!paymentResult.IsSuccess)
            return Result.Failure("پرداخت به توسعه‌دهنده ناموفق بود: " + paymentResult.Message);

        // بررسی تکمیل ظرفیت
        var approvedCount = await _applicationRepository.CountApprovedForProjectAsync(app.DailyProjectId, cancellationToken);
        var fullProject = await _projectRepository.GetByIdAsync(app.DailyProjectId, cancellationToken);

        if (fullProject is not null && approvedCount >= fullProject.Capacity)
        {
            fullProject.Complete();
            _projectRepository.Update(fullProject);
        }

        await _applicationRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "DailyTaskApproved",
            nameof(DailyProjectApplication),
            applicationId,
            $"DeveloperId={app.DeveloperId};Amount={project.PricePerUnit}",
            null,
            cancellationToken);

        // اعلان تایید تسک و واریز دستمزد
        var developerUser = await _userRepository.GetByIdAsync(app.DeveloperId, cancellationToken);

        await _notificationService.SendAsync(
            app.DeveloperId,
            "تسک روزمره تایید شد",
            $"تسک «{project.Title}» تایید شد و {project.PricePerUnit:N0} تومان به کیف پول شما واریز شد.",
            NotificationType.Wallet,
            DailyTasksLink(developerUser?.UserRole ?? UserRole.JobSeeker),
            cancellationToken);

        // دستاوردهای وابسته به درآمد (مثل کسب‌کار برتر) بررسی می‌شوند
        await _achievementService.CheckAndAwardAchievementsAsync(
            app.DeveloperId,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectTaskAsync(int applicationId, int adminId, string note, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        if (string.IsNullOrWhiteSpace(note))
            return Result.Failure("دلیل رد الزامی است.");

        var app = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (app is null)
            return Result.Failure("درخواست پیدا نشد.");

        if (app.Status != DailyApplicationStatus.Submitted)
            return Result.Failure("فقط درخواست‌های ارسال‌شده قابل رد هستند.");

        app.Reject(adminId, note);
        _applicationRepository.Update(app);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "DailyTaskRejected",
            nameof(DailyProjectApplication),
            applicationId,
            note,
            null,
            cancellationToken);

        // اعلان رد تسک همراه با دلیل
        var developerUser = await _userRepository.GetByIdAsync(app.DeveloperId, cancellationToken);

        await _notificationService.SendAsync(
            app.DeveloperId,
            "تسک روزمره رد شد",
            $"اثبات ارسالی شما رد شد. دلیل: {note}",
            NotificationType.Project,
            DailyTasksLink(developerUser?.UserRole ?? UserRole.JobSeeker),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ExpireTaskAsync(int applicationId, int adminId, CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        var app = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (app is null)
            return Result.Failure("درخواست پیدا نشد.");

        if (app.Status != DailyApplicationStatus.InProgress)
            return Result.Failure("فقط درخواست‌های در حال انجام قابل منقضی کردن هستند.");

        if (!app.IsExpired())
            return Result.Failure("این درخواست هنوز منقضی نشده است.");

        app.Expire();
        _applicationRepository.Update(app);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>منقضی‌کردن خودکار درخواست‌های مهلت‌گذشته.</summary>
    public async Task<Result> ExpireStaleApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var staleApplications = await _applicationRepository.GetExpiredAsync(cancellationToken);

        foreach (var application in staleApplications)
        {
            application.Expire();
            _applicationRepository.Update(application);
        }

        if (staleApplications.Count > 0)
            await _applicationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<DailyApplicationDto>>> GetApplicationsByProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var apps = await _applicationRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<DailyApplicationDto>>.Success(apps.Select(DailyProjectMapper.ToApplicationDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DailyApplicationDto>>> GetApplicationsByDeveloperAsync(int developerId, CancellationToken cancellationToken = default)
    {
        var apps = await _applicationRepository.GetByDeveloperIdAsync(developerId, cancellationToken);
        return Result<IReadOnlyList<DailyApplicationDto>>.Success(apps.Select(DailyProjectMapper.ToApplicationDto).ToList());
    }

    private async Task<Result> ValidateAdminAsync(int adminId, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);
        if (admin is null) return Result.Failure("کاربر مورد نظر پیدا نشد.");
        if (admin.UserRole != UserRole.Admin) return Result.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");
        return Result.Success();
    }
}