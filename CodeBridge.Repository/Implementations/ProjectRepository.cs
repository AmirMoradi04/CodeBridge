using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری پروژه‌ها؛ فقط دسترسی به داده.</summary>
public sealed class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Project?> GetWithContractAsync(int projectId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(project => project.Contract)
            .FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

    public Task<Project?> GetWithPartiesAsync(int projectId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(project => project.Employer)
            .Include(project => project.AssignedDeveloper)
            .Include(project => project.ApprovedByAdmin)
            .FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

    public async Task<IReadOnlyList<Project>> GetByEmployerIdAsync(
        int employerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.EmployerId == employerId)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Project>> GetByDeveloperIdAsync(
        int developerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.AssignedDeveloperId == developerId)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Project>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.Status == ProjectStatus.Pending)
            .OrderBy(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <summary>همه‌ی پروژه‌ها برای ادمین؛ نام طرفین هم بارگذاری می‌شود.</summary>
    public async Task<IReadOnlyList<Project>> GetAllForAdminAsync(
        ProjectStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(project => project.Employer)
            .Include(project => project.AssignedDeveloper)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(project => project.Status == status.Value);

        return await query
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByStatusAsync(
        ProjectStatus status,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.Status == status)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Project>> GetByRequiredLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.RequiredLevel == level)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Project>> GetUnassignedAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.AssignedDeveloperId == null
                && project.Status == ProjectStatus.Approved)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <summary>پروژه‌هایی که کاربر در آنها تخصیص یافته یا عضو تیم است.</summary>
    public async Task<IReadOnlyList<Project>> GetAssignedToUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(project => project.AssignedDeveloperId == userId
                || project.Members.Any(member => member.UserId == userId))
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <summary>تخصیص برنامه‌نویس و تغییر وضعیت به در حال اجرا.</summary>
    public async Task AssignDeveloperAsync(
        int projectId,
        int developerId,
        CancellationToken cancellationToken = default)
    {
        var project = await DbSet.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            ?? throw new KeyNotFoundException("پروژه‌ی مورد نظر پیدا نشد.");

        project.AssignDeveloper(developerId);
        Update(project);

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>تازه‌ترین پروژه‌های منتشرشده برای فید فعالیت.</summary>
    public async Task<IReadOnlyList<Project>> GetRecentPublishedAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var safeCount = count < 1 ? 10 : count;

        return await DbSet
            .AsNoTracking()
            .Where(project => project.IsPublic && project.PublishedAt != null)
            .OrderByDescending(project => project.PublishedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }
}
