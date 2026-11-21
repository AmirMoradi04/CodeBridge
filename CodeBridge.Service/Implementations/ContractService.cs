using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Contract;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار قرارداد: ساخت، امضا و تکمیل.</summary>
public sealed class ContractService : IContractService
{
    // امتیاز اضافه‌شده به برنامه‌نویس پس از تکمیل قرارداد
    private const int CompletedProjectScore = 10;

    private readonly IContractRepository _contractRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperLevelRepository _developerLevelRepository;
    private readonly IAuditLogService _auditLogService;

    public ContractService(
        IContractRepository contractRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IDeveloperLevelRepository developerLevelRepository,
        IAuditLogService auditLogService)
    {
        _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _developerLevelRepository = developerLevelRepository
            ?? throw new ArgumentNullException(nameof(developerLevelRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Result<ContractDto>> CreateAsync(
        CreateContractDto dto,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<ContractDto>.Failure("اطلاعات قرارداد الزامی است.");

        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return Result<ContractDto>.Failure(adminCheck.Message ?? "ادمین معتبر نیست.");

        if (dto.TotalAmount <= 0)
            return Result<ContractDto>.Failure("مبلغ کل قرارداد باید بزرگ‌تر از صفر باشد.");

        if (dto.PlatformFee < 0 || dto.DeveloperShare < 0 || (dto.MentorShare ?? 0m) < 0)
            return Result<ContractDto>.Failure("سهم‌های قرارداد نمی‌توانند منفی باشند.");

        // مجموع سهم‌ها باید دقیقاً برابر مبلغ کل باشد
        var sharesTotal = dto.PlatformFee + dto.DeveloperShare + (dto.MentorShare ?? 0m);

        if (sharesTotal != dto.TotalAmount)
            return Result<ContractDto>.Failure("مجموع سهم‌ها با مبلغ کل قرارداد هم‌خوانی ندارد.");

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, cancellationToken);

        if (project is null)
            return Result<ContractDto>.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        // قرارداد فقط برای پروژه‌ی تاییدشده و دارای برنامه‌نویس ساخته می‌شود
        if (project.Status is not (ProjectStatus.Approved or ProjectStatus.InProgress))
            return Result<ContractDto>.Failure("قرارداد فقط برای پروژه‌ی تاییدشده قابل ساخت است.");

        if (project.AssignedDeveloperId is null)
            return Result<ContractDto>.Failure("برای این پروژه هنوز برنامه‌نویسی تخصیص نیافته است.");

        if ((dto.MentorShare ?? 0m) > 0 && !dto.MentorId.HasValue)
            return Result<ContractDto>.Failure("برای سهم منتور، انتخاب منتور الزامی است.");

        if (dto.MentorId.HasValue)
        {
            if (dto.MentorId.Value == project.AssignedDeveloperId.Value)
                return Result<ContractDto>.Failure("برنامه‌نویس پروژه نمی‌تواند هم‌زمان منتور همان قرارداد باشد.");

            var mentor = await _userRepository.GetByIdAsync(dto.MentorId.Value, cancellationToken);
            if (mentor is null || mentor.UserRole != UserRole.JobSeeker)
                return Result<ContractDto>.Failure("منتور انتخاب‌شده باید یک برنامه‌نویس معتبر باشد.");
        }

        if (await _contractRepository.GetByProjectIdAsync(project.Id, cancellationToken) is not null)
            return Result<ContractDto>.Failure("برای این پروژه قبلاً قرارداد ثبت شده است.");

        var contract = new Contract(
            project.Id,
            project.AssignedDeveloperId.Value,
            project.EmployerId,
            adminId,
            dto.TotalAmount,
            dto.PlatformFee,
            dto.DeveloperShare,
            dto.MentorShare);

        if (dto.MentorId.HasValue)
            contract.SetMentor(dto.MentorId.Value);

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ContractCreated",
            nameof(Contract),
            contract.Id,
            null,
            null,
            cancellationToken);

        return Result<ContractDto>.Success(ContractMapper.ToDto(contract));
    }

    /// <summary>خواندن قرارداد با کنترل دسترسی: ادمین یا یکی از طرفین قرارداد.</summary>
    public async Task<Result<ContractDto>> GetByIdAsync(
        int contractId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(contractId, cancellationToken);

        if (contract is null)
            return Result<ContractDto>.Failure("قرارداد مورد نظر پیدا نشد.");

        var canAccess = callerRole switch
        {
            UserRole.Admin => true,
            UserRole.Employer => contract.EmployerId == callerId,
            // کارجویان همان برنامه‌نویسان پلتفرم هستند
            UserRole.JobSeeker => contract.DeveloperId == callerId,
            _ => false // سایر کاربران به قراردادها دسترسی ندارند
        };

        if (!canAccess)
            return Result<ContractDto>.Failure("شما به این قرارداد دسترسی ندارید.");

        return Result<ContractDto>.Success(ContractMapper.ToDto(contract));
    }

    public async Task<Result<ContractDto?>> GetByProjectAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByProjectIdAsync(projectId, cancellationToken);

        if (contract is null)
            return Result<ContractDto?>.Success(null);

        var canAccess = callerRole switch
        {
            UserRole.Admin => true,
            UserRole.Employer => contract.EmployerId == callerId,
            // کارجویان همان برنامه‌نویسان پلتفرم هستند
            UserRole.JobSeeker => contract.DeveloperId == callerId,
            _ => false
        };

        if (!canAccess)
            return Result<ContractDto?>.Failure("شما به قرارداد این پروژه دسترسی ندارید.");

        return Result<ContractDto?>.Success(ContractMapper.ToDto(contract));
    }

    public async Task<Result<IReadOnlyList<ContractDto>>> GetByDeveloperAsync(
        int developerId,
        CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetByDeveloperIdAsync(developerId, cancellationToken);

        return Result<IReadOnlyList<ContractDto>>.Success(
            contracts.Select(ContractMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ContractDto>>> GetByEmployerAsync(
        int employerId,
        CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetByEmployerIdAsync(employerId, cancellationToken);

        return Result<IReadOnlyList<ContractDto>>.Success(
            contracts.Select(ContractMapper.ToDto).ToList());
    }

    public async Task<Result> SignAsync(
        int contractId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId, cancellationToken);

        if (contract is null)
            return Result.Failure("قرارداد مورد نظر پیدا نشد.");

        // فقط طرفین قرارداد (برنامه‌نویس یا کارفرما) حق امضا دارند
        if (contract.DeveloperId != userId && contract.EmployerId != userId)
            return Result.Failure("فقط برنامه‌نویس یا کارفرمای طرف قرارداد می‌تواند آن را امضا کند.");

        if (contract.Status != ContractStatus.Draft)
            return Result.Failure("فقط قرارداد پیش‌نویس قابل امضا است.");

        contract.Sign();
        _contractRepository.Update(contract);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "ContractSigned",
            nameof(Contract),
            contractId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    /// <summary>امضای NDA فقط توسط برنامه‌نویس طرف قرارداد.</summary>
    public async Task<Result> SignNdaAsync(
        int contractId,
        int developerId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId, cancellationToken);

        if (contract is null)
            return Result.Failure("قرارداد مورد نظر پیدا نشد.");

        if (contract.DeveloperId != developerId)
            return Result.Failure("فقط برنامه‌نویس طرف قرارداد می‌تواند NDA را امضا کند.");

        try
        {
            contract.SignNda();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(exception.Message);
        }

        _contractRepository.Update(contract);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            developerId,
            "ContractNdaSigned",
            nameof(Contract),
            contractId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompleteAsync(
        int contractId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var contract = await _contractRepository.GetByIdAsync(contractId, cancellationToken);

        if (contract is null)
            return Result.Failure("قرارداد مورد نظر پیدا نشد.");

        var project = await _projectRepository.GetByIdAsync(contract.ProjectId, cancellationToken);
        if (project is null || !project.IsFinalApproved)
            return Result.Failure("تکمیل قرارداد فقط پس از تایید نهایی و پرداخت از حساب امانی مجاز است.");

        // قرارداد امضاشده در جریان تکمیل، ابتدا فعال و سپس تکمیل می‌شود
        if (contract.Status is not (ContractStatus.Signed or ContractStatus.Active))
            return Result.Failure("فقط قرارداد امضاشده یا فعال قابل تکمیل است.");

        if (!contract.IsNdaSigned)
            return Result.Failure("توافق‌نامه NDA هنوز امضا نشده است.");

        if (!contract.IsFullyPaid || contract.DepositPaid < contract.TotalAmount)
            return Result.Failure("کارفرما هنوز تسویه نهایی را انجام نداده است.");

        if (contract.Status == ContractStatus.Signed)
            contract.Activate();

        contract.Complete();

        project.ChangeStatus(ProjectStatus.Completed);
        _projectRepository.Update(project);

        // امتیاز و تعداد پروژه‌های تکمیل‌شده‌ی برنامه‌نویس به‌روزرسانی می‌شود
        var developerLevel = await _developerLevelRepository.GetByUserIdAsync(
            contract.DeveloperId,
            cancellationToken);

        if (developerLevel is not null)
        {
            developerLevel.RecordCompletedProject(CompletedProjectScore);
            _developerLevelRepository.Update(developerLevel);
        }

        _contractRepository.Update(contract);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ContractCompleted",
            nameof(Contract),
            contractId,
            $"DeveloperId={contract.DeveloperId}",
            null,
            cancellationToken);

        return Result.Success();
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
}
