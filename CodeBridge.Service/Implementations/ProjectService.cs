using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Project;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>مدیریت چرخه‌ی عمر پروژه و تخصیص برنامه‌نویس.</summary>
public sealed class ProjectService : IProjectService
{
    // امتیاز اضافه‌شده به برنامه‌نویس پس از تکمیل قرارداد (همان مقدار ContractService)
    private const int CompletedProjectScore = 10;

    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperLevelRepository _developerLevelRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IWalletService _walletService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IAchievementService _achievementService;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IDeveloperLevelRepository developerLevelRepository,
        IProjectMemberRepository projectMemberRepository,
        IContractRepository contractRepository,
        IWalletService walletService,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IAchievementService achievementService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _developerLevelRepository = developerLevelRepository
            ?? throw new ArgumentNullException(nameof(developerLevelRepository));
        _projectMemberRepository = projectMemberRepository
            ?? throw new ArgumentNullException(nameof(projectMemberRepository));
        _contractRepository = contractRepository
            ?? throw new ArgumentNullException(nameof(contractRepository));
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _achievementService = achievementService ?? throw new ArgumentNullException(nameof(achievementService));
    }

    public async Task<Result<ProjectDto>> CreateAsync(
        int employerId,
        CreateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<ProjectDto>.Failure("اطلاعات پروژه الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<ProjectDto>.Failure("عنوان پروژه الزامی است.");

        if (dto.Budget <= 0)
            return Result<ProjectDto>.Failure("بودجه‌ی پروژه باید بزرگ‌تر از صفر باشد.");

        var employer = await _userRepository.GetByIdAsync(employerId, cancellationToken);

        if (employer is null)
            return Result<ProjectDto>.Failure("کارفرمای مورد نظر پیدا نشد.");

        // فقط کارفرما مجاز به ثبت پروژه است
        if (employer.UserRole != UserRole.Employer)
            return Result<ProjectDto>.Failure("فقط کارفرما می‌تواند پروژه ثبت کند.");

        var project = new Project(
            employerId,
            dto.Title,
            dto.Description,
            dto.Budget,
            dto.RequiredLevel);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            employerId,
            "ProjectCreated",
            nameof(Project),
            project.Id,
            null,
            null,
            cancellationToken);

        return Result<ProjectDto>.Success(ProjectMapper.ToDto(project));
    }

    /// <summary>خواندن پروژه با کنترل دسترسی: ادمین، کارفرما، برنامه‌نویس تخصیص‌یافته یا عضو تیم.</summary>
    public async Task<Result<ProjectDto>> GetByIdAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithPartiesAsync(projectId, cancellationToken);

        if (project is null)
            return Result<ProjectDto>.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        var isMember = await _projectMemberRepository.IsMemberAsync(
            project.Id,
            callerId,
            cancellationToken);

        var canAccess = callerRole switch
        {
            UserRole.Admin => true,
            UserRole.Employer => project.EmployerId == callerId,
            // کارجویان همان برنامه‌نویسان پلتفرم هستند
            UserRole.JobSeeker => project.AssignedDeveloperId == callerId || isMember,
            _ => isMember
        };

        if (!canAccess)
            return Result<ProjectDto>.Failure("شما به این پروژه دسترسی ندارید.");

        return Result<ProjectDto>.Success(ProjectMapper.ToDto(project));
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> GetByEmployerAsync(
        int employerId,
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetByEmployerIdAsync(employerId, cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> GetByDeveloperAsync(
        int developerId,
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetByDeveloperIdAsync(developerId, cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> GetAssignedAsync(
        int developerId,
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAssignedToUserAsync(developerId, cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    /// <summary>فهرست کامل پروژه‌ها مخصوص ادمین؛ فیلتر وضعیت اختیاری است.</summary>
    public async Task<Result<IReadOnlyList<ProjectDto>>> GetAllAsync(
        ProjectStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllForAdminAsync(status, cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetPendingAsync(cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> GetUnassignedAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetUnassignedAsync(cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(ProjectMapper.ToDto).ToList());
    }

    public async Task<Result> ApproveAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.Status != ProjectStatus.Pending)
            return Result.Failure("فقط پروژه‌های در انتظار تایید قابل تایید هستند.");

        project.Approve(adminId);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectApproved",
            nameof(Project),
            projectId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        int projectId,
        int adminId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("دلیل رد پروژه الزامی است.");

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.Status != ProjectStatus.Pending)
            return Result.Failure("فقط پروژه‌های در انتظار تایید قابل رد هستند.");

        project.Reject(adminId, reason);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectRejected",
            nameof(Project),
            projectId,
            reason,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignToDeveloperAsync(
        int projectId,
        AssignProjectDto dto,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || dto.DeveloperId <= 0)
            return Result.Failure("شناسه‌ی برنامه‌نویس معتبر نیست.");

        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.Status != ProjectStatus.Approved)
            return Result.Failure("فقط پروژه‌ی تاییدشده قابل تخصیص است.");

        var developer = await _userRepository.GetByIdAsync(dto.DeveloperId, cancellationToken);

        if (developer is null)
            return Result.Failure("برنامه‌نویس مورد نظر پیدا نشد.");

        // کارجویان نقش برنامه‌نویس را دارند؛ نقش جداگانه‌ای برای برنامه‌نویس وجود ندارد
        if (developer.UserRole != UserRole.JobSeeker)
            return Result.Failure("کاربر انتخاب‌شده برنامه‌نویس نیست.");

        var developerLevel = await _developerLevelRepository.GetByUserIdAsync(
            developer.Id,
            cancellationToken);

        if (developerLevel is null)
            return Result.Failure("سطح برنامه‌نویس مشخص نشده است.");

        // قانون سطح حداقلی: پروژه‌های Junior هر سطحی، Mid فقط Mid/Senior، Senior فقط Senior می‌پذیرند
        if (!IsLevelSufficient(project.RequiredLevel, developerLevel.Level))
            return Result.Failure("سطح برنامه‌نویس برای این پروژه کافی نیست.");

        project.AssignDeveloper(developer.Id);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectAssigned",
            nameof(Project),
            projectId,
            $"DeveloperId={developer.Id}",
            null,
            cancellationToken);

        // اعلان تخصیص پروژه به برنامه‌نویس
        await _notificationService.SendAsync(
            developer.Id,
            "پروژه‌ی جدید به شما تخصیص یافت",
            $"پروژه‌ی «{project.Title}» برای شما ثبت شد.",
            NotificationType.Project,
            "/jobseeker/projects",
            cancellationToken);

        return Result.Success();
    }

    /// <summary>انتشار عمومی پروژه‌ی تاییدشده توسط ادمین. نیاز به تسویه‌ی کامل قرارداد و امضای NDA دارد.</summary>
    public async Task<Result> PublishAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        var contract = await _contractRepository.GetByProjectIdAsync(projectId, cancellationToken);

        if (contract is null)
            return Result.Failure("قراردادی برای این پروژه یافت نشد.");

        if (!contract.IsNdaSigned)
            return Result.Failure("قرارداد NDA هنوز امضا نشده و پروژه قابل انتشار نیست.");

        if (!contract.IsFullyPaid)
            return Result.Failure("قراراد کامل تسویه نشده و پروژه قابل انتشار نیست.");

        try
        {
            project.Publish();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(exception.Message);
        }

        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectPublished",
            nameof(Project),
            projectId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>افزودن عضو به تیم پروژه؛ ProjectId باید با شناسه‌ی مسیر یکی باشد.</summary>
    public async Task<Result<ProjectMemberDto>> AddMemberAsync(
        AddProjectMemberDto dto,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        if (dto is null || dto.ProjectId <= 0)
            return Result<ProjectMemberDto>.Failure("شناسه‌ی پروژه معتبر نیست.");

        if (dto.UserId <= 0)
            return Result<ProjectMemberDto>.Failure("شناسه‌ی کاربر معتبر نیست.");

        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return Result<ProjectMemberDto>.Failure(adminCheck.Message ?? "ادمین معتبر نیست.");

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, cancellationToken);

        if (project is null)
            return Result<ProjectMemberDto>.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        var user = await _userRepository.GetByIdAsync(dto.UserId, cancellationToken);

        if (user is null)
            return Result<ProjectMemberDto>.Failure("کاربر مورد نظر پیدا نشد.");

        if (await _projectMemberRepository.AnyAsync(
                member => member.ProjectId == dto.ProjectId && member.UserId == dto.UserId,
                cancellationToken))
            return Result<ProjectMemberDto>.Failure("کاربر قبلاً در این پروژه عضو است.");

        var projectMember = new ProjectMember(dto.ProjectId, dto.UserId, adminId);

        await _projectMemberRepository.AddAsync(projectMember, cancellationToken);
        await _projectMemberRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectMemberAdded",
            nameof(ProjectMember),
            projectMember.Id,
            $"ProjectId={dto.ProjectId};UserId={dto.UserId}",
            null,
            cancellationToken);

        var created = await _projectMemberRepository.GetByIdAsync(projectMember.Id, cancellationToken);

        return created is null
            ? Result<ProjectMemberDto>.Success(ProjectMemberMapper.ToDto(projectMember))
            : Result<ProjectMemberDto>.Success(ProjectMemberMapper.ToDto(created));
    }

    /// <summary>فهرست اعضای پروژه؛ فقط ادمین یا اعضای خود پروژه مجازند.</summary>
    public async Task<Result<IReadOnlyList<ProjectMemberDto>>> GetMembersAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result<IReadOnlyList<ProjectMemberDto>>.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        var isMember = await _projectMemberRepository.IsMemberAsync(projectId, callerId, cancellationToken);

        if (callerRole != UserRole.Admin && !isMember)
            return Result<IReadOnlyList<ProjectMemberDto>>.Failure("فقط ادمین یا اعضای پروژه می‌توانند فهرست اعضا را ببینند.");

        var members = await _projectMemberRepository.GetByProjectIdAsync(projectId, cancellationToken);

        return Result<IReadOnlyList<ProjectMemberDto>>.Success(
            members.Select(ProjectMemberMapper.ToDto).ToList());
    }

    /// <summary>حذف عضو از تیم پروژه توسط ادمین.</summary>
    public async Task<Result> RemoveMemberAsync(
        int projectId,
        int userId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.AssignedDeveloperId == userId)
            return Result.Failure("برنامه‌نویس تخصیص‌یافته قابل حذف از پروژه نیست.");

        if (!await _projectMemberRepository.AnyAsync(
                member => member.ProjectId == projectId && member.UserId == userId,
                cancellationToken))
            return Result.Failure("کاربر مورد نظر عضو این پروژه نیست.");

        await _projectMemberRepository.RemoveAsync(projectId, userId, cancellationToken);
        await _projectMemberRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectMemberRemoved",
            nameof(ProjectMember),
            null,
            $"ProjectId={projectId};UserId={userId}",
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateStatusAsync(
        int projectId,
        ProjectStatus status,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.Status is ProjectStatus.Completed or ProjectStatus.Rejected)
            return Result.Failure("وضعیت نهایی پروژه قابل تغییر نیست.");

        if (status == ProjectStatus.InProgress && project.AssignedDeveloperId is null)
            return Result.Failure("ابتدا باید برنامه‌نویس به پروژه تخصیص یابد.");

        project.ChangeStatus(status);
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "ProjectStatusChanged",
            nameof(Project),
            projectId,
            status.ToString(),
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی (ESCROW).</summary>
    public async Task<Result> EmployerDepositAsync(
        int projectId,
        int employerId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure("مبلغ واریز باید بزرگ‌تر از صفر باشد.");

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.EmployerId != employerId)
            return Result.Failure("فقط کارفرمای پروژه می‌تواند واریز انجام دهد.");

        var contract = await _contractRepository.GetByProjectIdAsync(projectId, cancellationToken);
        if (contract is null)
            return Result.Failure("قراردادی برای این پروژه یافت نشد.");

        if (contract.Status is not (ContractStatus.Signed or ContractStatus.Active))
            return Result.Failure("ابتدا باید قرارداد امضا شود.");

        var remainingAmount = contract.TotalAmount - contract.DepositPaid;

        if (amount > remainingAmount)
            return Result.Failure($"مبلغ بیش از مانده‌ی تسویه ({remainingAmount:N0} تومان) است.");

        var depositResult = await _walletService.EmployerDepositToEscrowAsync(
            employerId,
            amount,
            projectId,
            $"واریز بودجه‌ی پروژه شماره {projectId}",
            cancellationToken);

        if (!depositResult.IsSuccess)
            return Result.Failure("واریز ناموفق بود: " + depositResult.Message);

        contract.RegisterDeposit(amount);
        _contractRepository.Update(contract);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            employerId,
            "ProjectDeposit",
            nameof(Project),
            projectId,
            $"Amount={amount};TotalDeposit={contract.DepositPaid}",
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>تایید نهایی و پرداخت به برنامه‌نویس/منتور. فقط ادمین.</summary>
    public async Task<Result> FinalApproveAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);
        if (!adminCheck.IsSuccess) return adminCheck;

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result.Failure("پروژه پیدا نشد.");

        if (project.Status != ProjectStatus.InProgress)
            return Result.Failure("فقط پروژه‌ی در حال اجرا قابل تایید نهایی است.");

        if (project.IsFinalApproved)
            return Result.Failure("این پروژه قبلاً تأیید نهایی شده است.");

        var contract = await _contractRepository.GetByProjectIdAsync(projectId, cancellationToken);
        if (contract is null)
            return Result.Failure("قراردادی برای این پروژه یافت نشد.");

        if (!contract.IsNdaSigned)
            return Result.Failure("توافق‌نامه NDA هنوز امضا نشده است.");

        if (!contract.IsFullyPaid || contract.DepositPaid < contract.TotalAmount)
            return Result.Failure("کارفرما هنوز تسویه نهایی را انجام نداده است.");

        // تکمیل نیازمند وضعیت فعال است؛ امضای معتبر قرارداد آن را فعال می‌کند.
        if (contract.Status == ContractStatus.Draft)
            return Result.Failure("قرارداد هنوز توسط طرفین امضا نشده است.");

        if (contract.Status == ContractStatus.Signed)
            contract.Activate();

        // پرداخت به برنامه‌نویس از حساب امانی
        if (contract.DeveloperId > 0 && contract.DeveloperShare > 0)
        {
            var payDevResult = await _walletService.PayDeveloperFromEscrowAsync(
                adminId,
                contract.DeveloperId,
                contract.DeveloperShare,
                WalletTransactionType.ProjectPayment,
                $"تسویه نهایی پروژه: {project.Title}",
                projectId,
                cancellationToken);

            if (!payDevResult.IsSuccess)
                return Result.Failure("پرداخت به برنامه‌نویس ناموفق بود: " + payDevResult.Message);
        }

        // پرداخت به منتور (اگر وجود دارد)
        if (contract.MentorId.HasValue && contract.MentorShare > 0)
            {
                var payMentorResult = await _walletService.PayDeveloperFromEscrowAsync(
                    adminId,
                    contract.MentorId.Value,
                    contract.MentorShare.Value,
                    WalletTransactionType.ProjectPayment,
                    $"سهم منتور از پروژه: {project.Title}",
                    projectId,
                    cancellationToken);

            if (!payMentorResult.IsSuccess)
                return Result.Failure("پرداخت به منتور ناموفق بود: " + payMentorResult.Message);
        }

        contract.Complete();
        _contractRepository.Update(contract);

        try
        {
            project.FinalApprove();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        // پس از پرداخت از حساب امانی، چرخه‌ی پروژه هم بسته می‌شود.
        project.ChangeStatus(ProjectStatus.Completed);
        _projectRepository.Update(project);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        // شمارش پروژه‌ی تکمیل‌شده‌ی برنامه‌نویس و منتور (این مسیر، مسیر اصلی بستن پروژه است)
        await RecordCompletedProjectsAsync(contract, cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ProjectFinalApproved",
            nameof(Project),
            projectId,
            $"DeveloperShare={contract.DeveloperShare};MentorShare={contract.MentorShare}",
            null,
            cancellationToken);

        // اعلان بسته‌شدن پروژه و پرداخت نهایی به کارفرما
        await _notificationService.SendAsync(
            project.EmployerId,
            "پرداخت نهایی پروژه انجام شد",
            $"پروژه‌ی «{project.Title}» تایید نهایی و تسویه شد.",
            NotificationType.Project,
            "/employer",
            cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// به‌روزرسانی سطح برنامه‌نویس/منتور پس از تسویه و ارزیابی دستاوردها.
    /// قرارداد در این مسیر هرگز قبلاً Completed نشده، پس شمارش تکراری رخ نمی‌دهد.
    /// </summary>
    private async Task RecordCompletedProjectsAsync(Contract contract, CancellationToken cancellationToken)
    {
        var recipients = new List<int> { contract.DeveloperId };

        if (contract.MentorId.HasValue)
            recipients.Add(contract.MentorId.Value);

        foreach (var userId in recipients.Distinct())
        {
            var level = await _developerLevelRepository.GetByUserIdAsync(userId, cancellationToken);

            if (level is not null)
            {
                level.RecordCompletedProject(CompletedProjectScore);
                _developerLevelRepository.Update(level);
                await _developerLevelRepository.SaveChangesAsync(cancellationToken);
            }

            await _achievementService.CheckAndAwardAchievementsAsync(userId, cancellationToken);
        }
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

    /// <summary>
    /// قانون سطح حداقلی تخصیص:
    /// پروژه‌های Junior (JuniorA/B/C) هر سطحی را می‌پذیرند؛
    /// پروژه‌های Mid (MidA/B/C) فقط Mid یا Senior را می‌پذیرند؛
    /// پروژه‌ی Senior فقط برنامه‌نویس Senior را می‌پذیرد.
    /// </summary>
    private static bool IsLevelSufficient(DeveloperLevelType projectLevel, DeveloperLevelType devLevel)
    {
        if (projectLevel <= DeveloperLevelType.JuniorC)
            return true;

        if (projectLevel <= DeveloperLevelType.MidC)
            return devLevel >= DeveloperLevelType.MidA;

        return devLevel == DeveloperLevelType.Senior;
    }
}
