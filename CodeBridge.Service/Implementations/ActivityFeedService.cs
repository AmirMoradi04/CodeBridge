using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Activity;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>فید فعالیت: ترکیب رویدادهای اخیر کاربران، پروژه‌ها، نظرات و واریزها.</summary>
public sealed class ActivityFeedService : IActivityFeedService
{
    private const int MaxCount = 50;

    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public ActivityFeedService(
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IReviewRepository reviewRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _transactionRepository = transactionRepository
            ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public async Task<Result<IReadOnlyList<ActivityFeedDto>>> GetRecentActivitiesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count <= 0 ? 10 : Math.Min(count, MaxCount);

        // هر منبع بیش از حد نیاز خوانده می‌شود تا پس از ادغام، بیست مورد اخیر قابل انتخاب باشد
        var fetchCount = safeCount * 2;

        var activities = new List<ActivityFeedDto>(fetchCount * 2);

        var users = await _userRepository.GetRecentAsync(fetchCount, cancellationToken);

        foreach (var user in users)
        {
            activities.Add(new ActivityFeedDto
            {
                Type = "user",
                Title = "کاربر جدید",
                Description = $"{user.FirstName} {user.LastName} به CodeBridge پیوست",
                CreatedAt = user.CreatedAt
            });
        }

        var projects = await _projectRepository.GetRecentPublishedAsync(fetchCount, cancellationToken);

        foreach (var project in projects)
        {
            activities.Add(new ActivityFeedDto
            {
                Type = "project",
                Title = "پروژه‌ی جدید منتشر شد",
                Description = project.Title,
                CreatedAt = project.PublishedAt ?? project.CreatedAt
            });
        }

        var reviews = await _reviewRepository.GetRecentFiveStarAsync(fetchCount, cancellationToken);

        foreach (var review in reviews)
        {
            var target = review.TargetUser is null
                ? "کاربر"
                : $"{review.TargetUser.FirstName} {review.TargetUser.LastName}".Trim();

            activities.Add(new ActivityFeedDto
            {
                Type = "review",
                Title = "امتیاز ۵ ستاره ثبت شد",
                Description = $"«{target}» بابت پروژه‌ی «{review.Project?.Title ?? "—"}» پنج ستاره گرفت",
                CreatedAt = review.CreatedAt
            });
        }

        var deposits = await _transactionRepository.GetRecentDepositsAsync(fetchCount, cancellationToken);
        var ownerIds = deposits
            .Select(transaction => transaction.Wallet?.UserId ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var owners = ownerIds.Count > 0
            ? await _userRepository.FindAsync(user => ownerIds.Contains(user.Id), cancellationToken)
            : Array.Empty<CodeBridge.Domain.Entities.Identity.User>();

        var ownersById = owners.ToDictionary(owner => owner.Id);

        foreach (var deposit in deposits)
        {
            var ownerId = deposit.Wallet?.UserId ?? 0;
            ownersById.TryGetValue(ownerId, out var owner);

            var ownerName = owner is null
                ? "یک کاربر"
                : $"{owner.FirstName} {owner.LastName}".Trim();

            activities.Add(new ActivityFeedDto
            {
                Type = "deposit",
                Title = "واریز جدید",
                Description = $"{ownerName} مبلغ {deposit.Amount:N0} تومان شارژ کرد",
                CreatedAt = deposit.CreatedAt
            });
        }

        IReadOnlyList<ActivityFeedDto> result = activities
            .OrderByDescending(activity => activity.CreatedAt)
            .Take(safeCount)
            .ToList();

        return Result<IReadOnlyList<ActivityFeedDto>>.Success(result);
    }
}
