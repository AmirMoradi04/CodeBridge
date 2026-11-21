using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Review;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار امتیازدهی: فقط کارفرمای پروژه‌ی تکمیل‌شده امتیاز ثبت می‌کند.</summary>
public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IDeveloperLevelService _developerLevelService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAchievementService _achievementService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IProjectRepository projectRepository,
        IContractRepository contractRepository,
        IDeveloperLevelService developerLevelService,
        IAuditLogService auditLogService,
        IAchievementService achievementService)
    {
        _reviewRepository = reviewRepository
            ?? throw new ArgumentNullException(nameof(reviewRepository));
        _projectRepository = projectRepository
            ?? throw new ArgumentNullException(nameof(projectRepository));
        _contractRepository = contractRepository
            ?? throw new ArgumentNullException(nameof(contractRepository));
        _developerLevelService = developerLevelService
            ?? throw new ArgumentNullException(nameof(developerLevelService));
        _auditLogService = auditLogService
            ?? throw new ArgumentNullException(nameof(auditLogService));
        _achievementService = achievementService
            ?? throw new ArgumentNullException(nameof(achievementService));
    }

    public async Task<Result> SubmitReviewAsync(
        int reviewerId,
        CreateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result.Failure("اطلاعات امتیاز الزامی است.");

        if (dto.Rating is < Review.MinRating or > Review.MaxRating)
            return Result.Failure("امتیاز باید عددی بین ۱ تا ۵ باشد.");

        if (dto.TargetUserId == reviewerId)
            return Result.Failure("کاربر نمی‌تواند به خودش امتیاز بدهد.");

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, cancellationToken);

        if (project is null)
            return Result.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (project.EmployerId != reviewerId)
            return Result.Failure("فقط کارفرمای پروژه می‌تواند امتیاز ثبت کند.");

        if (project.Status != ProjectStatus.Completed)
            return Result.Failure("ثبت امتیاز فقط پس از تکمیل پروژه امکان‌پذیر است.");

        var contract = await _contractRepository.GetByProjectIdAsync(project.Id, cancellationToken);

        if (contract is null)
            return Result.Failure("برای این پروژه قراردادی ثبت نشده است.");

        // هدف باید برنامه‌نویس یا منتور همین قرارداد باشد
        var isProjectMemberOfContract = dto.TargetUserId == contract.DeveloperId
            || (contract.MentorId.HasValue && dto.TargetUserId == contract.MentorId.Value);

        if (!isProjectMemberOfContract)
            return Result.Failure("کاربر هدف باید برنامه‌نویس یا منتور همین پروژه باشد.");

        if (await _reviewRepository.HasReviewedAsync(
                project.Id,
                reviewerId,
                dto.TargetUserId,
                cancellationToken))
            return Result.Failure("برای این پروژه قبلاً به این کاربر امتیاز داده‌اید.");

        Review review;

        try
        {
            review = new Review(
                project.Id,
                contract.Id,
                reviewerId,
                dto.TargetUserId,
                dto.Rating,
                dto.Comment);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(exception.Message);
        }

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        // میانگین امتیاز سطح برنامه‌نویس بازمحاسبه می‌شود
        await _developerLevelService.RecalculateRatingAsync(dto.TargetUserId, cancellationToken);

        // بازخورد مثبت امتیاز اضافه می‌کند؛ اگر سطحی وجود نداشته باشد نادیده گرفته می‌شود
        var scoreDelta = Math.Max(0, dto.Rating - 3);

        if (scoreDelta > 0)
            await _developerLevelService.UpdateScoreAsync(
                dto.TargetUserId,
                scoreDelta,
                cancellationToken);

        // دستاوردهای وابسته به نظر (مثل ستاره‌ی پنج‌ستاره) بررسی می‌شوند
        await _achievementService.CheckAndAwardAchievementsAsync(
            dto.TargetUserId,
            cancellationToken);

        await _auditLogService.LogAsync(
            reviewerId,
            "ReviewSubmitted",
            nameof(Review),
            review.Id,
            $"ProjectId={project.Id}; TargetUserId={dto.TargetUserId}; Rating={dto.Rating}",
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetUserReviewsAsync(
        int targetUserId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByTargetUserIdAsync(targetUserId, cancellationToken);

        IReadOnlyList<ReviewDto> dtos = reviews.Select(ReviewMapper.ToDto).ToList();

        return Result<IReadOnlyList<ReviewDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetProjectReviewsAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByProjectIdAsync(projectId, cancellationToken);

        IReadOnlyList<ReviewDto> dtos = reviews.Select(ReviewMapper.ToDto).ToList();

        return Result<IReadOnlyList<ReviewDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetAllReviewsAsync(
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetAllWithDetailsAsync(cancellationToken);

        IReadOnlyList<ReviewDto> dtos = reviews.Select(ReviewMapper.ToDto).ToList();

        return Result<IReadOnlyList<ReviewDto>>.Success(dtos);
    }
}
