using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>دسترسی به عضوهای تیم پروژه.</summary>
public interface IProjectMemberRepository : IGenericRepository<ProjectMember>
{
    /// <summary>اعضای یک پروژه.</summary>
    Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>پروژه‌هایی که کاربر در آنها عضو است.</summary>
    Task<IReadOnlyList<ProjectMember>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>بررسی عضویت کاربر در پروژه.</summary>
    Task<bool> IsMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);

    /// <summary>حذف عضو از پروژه؛ تغییرات باید جداگانه ذخیره شوند.</summary>
    Task RemoveAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}