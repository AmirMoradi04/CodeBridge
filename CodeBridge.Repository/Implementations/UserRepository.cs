using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری کاربران؛ فقط دسترسی به داده.</summary>
public sealed class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetWithResumeAndLevelAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(user => user.Resume)
                .ThenInclude(resume => resume!.Attachment)
            .Include(user => user.DeveloperLevel)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetWithRefreshTokensAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(user => user.DeveloperLevel)
            .Where(user => user.UserRole == role)
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(user => user.DeveloperLevel)
            .Where(user => !user.IsApproved)
            .OrderBy(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllWithLevelsAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(user => user.DeveloperLevel)
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetJobSeekersByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(user => user.DeveloperLevel)
            .Where(user => user.UserRole == UserRole.JobSeeker
                && user.DeveloperLevel != null
                && user.DeveloperLevel.Level == level)
            .ToListAsync(cancellationToken);

    public Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        => DbSet.CountAsync(user => user.UserRole == role, cancellationToken);

    /// <summary>تازه‌ترین کاربران (به‌جز ادمین‌ها) برای فید فعالیت.</summary>
    public async Task<IReadOnlyList<User>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 10 : count;

        return await DbSet
            .AsNoTracking()
            .Where(user => user.UserRole != UserRole.Admin)
            .OrderByDescending(user => user.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>تایید حساب کاربر و ثبت تغییر.</summary>
    public async Task ApproveUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await DbSet.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("کاربر مورد نظر پیدا نشد.");

        user.Approve();
        Update(user);

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>رد حساب کاربر و ثبت تغییر.</summary>
    public async Task RejectUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await DbSet.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("کاربر مورد نظر پیدا نشد.");

        user.Reject();
        Update(user);

        await SaveChangesAsync(cancellationToken);
    }
}
