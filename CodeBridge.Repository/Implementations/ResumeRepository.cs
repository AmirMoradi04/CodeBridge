using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری رزومه‌ها؛ فقط دسترسی به داده.</summary>
public sealed class ResumeRepository : GenericRepository<Resume>, IResumeRepository
{
    public ResumeRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Resume?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(resume => resume.UserId == userId, cancellationToken);

    public Task<Resume?> GetWithAttachmentAsync(int resumeId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(resume => resume.Attachment)
            .Include(resume => resume.ReviewedByAdmin)
            .FirstOrDefaultAsync(resume => resume.Id == resumeId, cancellationToken);

    public async Task<IReadOnlyList<Resume>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(resume => resume.Status == ResumeStatus.Pending)
            .OrderBy(resume => resume.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Resume>> GetApprovedAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(resume => resume.Status == ResumeStatus.Approved)
            .OrderByDescending(resume => resume.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Resume>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(resume => resume.User.DeveloperLevel != null
                && resume.User.DeveloperLevel.Level == level)
            .OrderByDescending(resume => resume.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task ApproveResumeAsync(int resumeId, int adminId, CancellationToken cancellationToken = default)
    {
        var resume = await DbSet.FirstOrDefaultAsync(r => r.Id == resumeId, cancellationToken)
            ?? throw new KeyNotFoundException("رزومه‌ی مورد نظر پیدا نشد.");

        resume.Approve(adminId);
        Update(resume);

        await SaveChangesAsync(cancellationToken);
    }

    public async Task RejectResumeAsync(int resumeId, int adminId, CancellationToken cancellationToken = default)
    {
        var resume = await DbSet.FirstOrDefaultAsync(r => r.Id == resumeId, cancellationToken)
            ?? throw new KeyNotFoundException("رزومه‌ی مورد نظر پیدا نشد.");

        resume.Reject(adminId);
        Update(resume);

        await SaveChangesAsync(cancellationToken);
    }
}
