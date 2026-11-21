using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری عضوهای پروژه؛ فقط دسترسی به داده.</summary>
public sealed class ProjectMemberRepository : GenericRepository<ProjectMember>, IProjectMemberRepository
{
    public ProjectMemberRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(member => member.User)
            .Where(member => member.ProjectId == projectId)
            .OrderBy(member => member.JoinedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProjectMember>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(member => member.Project)
            .Where(member => member.UserId == userId)
            .OrderByDescending(member => member.JoinedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> IsMemberAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken);

    public async Task RemoveAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var member = await DbSet
            .FirstOrDefaultAsync(
                item => item.ProjectId == projectId && item.UserId == userId,
                cancellationToken);

        if (member is not null)
            Remove(member);
    }
}