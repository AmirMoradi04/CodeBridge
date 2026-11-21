using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// امتیاز و نظر کارفرما درباره‌ی برنامه‌نویس یا منتور یک پروژه‌ی تکمیل‌شده.
/// </summary>
public class Review : BaseEntity
{
    public const int MinRating = 1;
    public const int MaxRating = 5;

    public int ProjectId { get; private set; }

    public int ContractId { get; private set; }

    /// <summary>کارفرمایی که امتیاز می‌دهد.</summary>
    public int ReviewerId { get; private set; }

    /// <summary>برنامه‌نویس یا منتوری که امتیاز می‌گیرد.</summary>
    public int TargetUserId { get; private set; }

    public int Rating { get; private set; }

    public string? Comment { get; private set; }

    // --- Navigation Properties ---
    public Project Project { get; private set; } = null!;

    public Contract Contract { get; private set; } = null!;

    public User Reviewer { get; private set; } = null!;

    public User TargetUser { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private Review()
    {
    }

    public Review(
        int projectId,
        int contractId,
        int reviewerId,
        int targetUserId,
        int rating,
        string? comment)
    {
        ProjectId = projectId;
        ContractId = contractId;
        ReviewerId = reviewerId;
        TargetUserId = targetUserId;
        Rating = rating;
        // متن خالی معادل «بدون نظر» است
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        Validate();
    }

    public override void Validate()
    {
        if (Rating is < MinRating or > MaxRating)
            throw new InvalidOperationException("امتیاز باید عددی بین ۱ تا ۵ باشد.");

        if (TargetUserId == ReviewerId)
            throw new InvalidOperationException("کاربر نمی‌تواند به خودش امتیاز بدهد.");
    }
}
