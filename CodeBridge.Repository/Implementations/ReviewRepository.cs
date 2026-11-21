using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری امتیازها؛ جدیدترین نظرات اول برگردانده می‌شوند.</summary>
public sealed class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Review>> GetByTargetUserIdAsync(
        int targetUserId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(review => review.Reviewer)
            .Include(review => review.Project)
            .Where(review => review.TargetUserId == targetUserId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(review => review.Reviewer)
            .Include(review => review.TargetUser)
            .Include(review => review.Project)
            .Where(review => review.ProjectId == projectId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasReviewedAsync(
        int projectId,
        int reviewerId,
        int targetUserId,
        CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(
            review => review.ProjectId == projectId
                && review.ReviewerId == reviewerId
                && review.TargetUserId == targetUserId,
            cancellationToken);

    public async Task<IReadOnlyList<Review>> GetRecentFiveStarAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 10 : count;

        return await DbSet
            .AsNoTracking()
            .Include(review => review.TargetUser)
            .Include(review => review.Project)
            .Where(review => review.Rating == Review.MaxRating)
            .OrderByDescending(review => review.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(review => review.Reviewer)
            .Include(review => review.TargetUser)
            .Include(review => review.Project)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);
}
