using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Resume;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>مدیریت رزومه‌های کارجویان و بررسی ادمین.</summary>
public sealed class ResumeService : IResumeService
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;

    public ResumeService(
        IResumeRepository resumeRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService)
    {
        _resumeRepository = resumeRepository ?? throw new ArgumentNullException(nameof(resumeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Result<ResumeDto>> CreateOrUpdateAsync(
        int userId,
        CreateResumeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<ResumeDto>.Failure("اطلاعات رزومه الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Summary))
            return Result<ResumeDto>.Failure("عنوان و خلاصه‌ی رزومه الزامی است.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result<ResumeDto>.Failure("کاربر مورد نظر پیدا نشد.");

        // فقط کارجو مجاز به ثبت رزومه است
        if (user.UserRole != UserRole.JobSeeker)
            return Result<ResumeDto>.Failure("فقط کارجو می‌تواند رزومه ثبت کند.");

        var existing = await _resumeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existing is not null)
        {
            var trackedResume = await _resumeRepository.GetByIdAsync(existing.Id, cancellationToken)
                ?? existing;

            trackedResume.UpdateDetails(dto.Title, dto.Summary, dto.Skills, dto.AttachmentId);

            _resumeRepository.Update(trackedResume);
            await _resumeRepository.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                userId,
                "ResumeUpdated",
                nameof(Resume),
                trackedResume.Id,
                null,
                null,
                cancellationToken);

            return Result<ResumeDto>.Success(ResumeMapper.ToDto(trackedResume));
        }

        var resume = new Resume(userId, dto.Title, dto.Summary, dto.Skills, dto.AttachmentId);

        await _resumeRepository.AddAsync(resume, cancellationToken);
        await _resumeRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId,
            "ResumeCreated",
            nameof(Resume),
            resume.Id,
            null,
            null,
            cancellationToken);

        return Result<ResumeDto>.Success(ResumeMapper.ToDto(resume));
    }

    public async Task<Result<ResumeDto>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var resume = await _resumeRepository.GetByUserIdAsync(userId, cancellationToken);

        return resume is null
            ? Result<ResumeDto>.Failure("رزومه‌ای برای این کاربر ثبت نشده است.")
            : Result<ResumeDto>.Success(ResumeMapper.ToDto(resume));
    }

    public async Task<Result<IReadOnlyList<ResumeDto>>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var resumes = await _resumeRepository.GetPendingAsync(cancellationToken);

        return Result<IReadOnlyList<ResumeDto>>.Success(resumes.Select(ResumeMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<ResumeDto>>> GetApprovedAsync(
        CancellationToken cancellationToken = default)
    {
        var resumes = await _resumeRepository.GetApprovedAsync(cancellationToken);

        return Result<IReadOnlyList<ResumeDto>>.Success(resumes.Select(ResumeMapper.ToDto).ToList());
    }

    public async Task<Result> ApproveAsync(
        int resumeId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var resume = await _resumeRepository.GetByIdAsync(resumeId, cancellationToken);

        if (resume is null)
            return Result.Failure("رزومه‌ی مورد نظر پیدا نشد.");

        if (resume.Status != ResumeStatus.Pending)
            return Result.Failure("این رزومه قبلاً بررسی شده است.");

        await _resumeRepository.ApproveResumeAsync(resumeId, adminId, cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ResumeApproved",
            nameof(Resume),
            resumeId,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        int resumeId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return adminCheck;

        var resume = await _resumeRepository.GetByIdAsync(resumeId, cancellationToken);

        if (resume is null)
            return Result.Failure("رزومه‌ی مورد نظر پیدا نشد.");

        if (resume.Status != ResumeStatus.Pending)
            return Result.Failure("این رزومه قبلاً بررسی شده است.");

        await _resumeRepository.RejectResumeAsync(resumeId, adminId, cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ResumeRejected",
            nameof(Resume),
            resumeId,
            null,
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
            return Result.Failure("فقط ادمین می‌تواند رزومه را بررسی کند.");

        return Result.Success();
    }
}
