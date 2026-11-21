using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای پروژه‌ها.</summary>
public interface IProjectRepository : IGenericRepository<Project>
{
    Task<Project?> GetWithContractAsync(int projectId, CancellationToken cancellationToken = default);

    Task<Project?> GetWithPartiesAsync(int projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByEmployerIdAsync(int employerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByDeveloperIdAsync(int developerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>فهرست همه‌ی پروژه‌ها برای پنل ادمین با فیلتر اختیاری وضعیت.</summary>
    Task<IReadOnlyList<Project>> GetAllForAdminAsync(
        ProjectStatus? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByStatusAsync(
        ProjectStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByRequiredLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetUnassignedAsync(CancellationToken cancellationToken = default);

    /// <summary>پروژه‌هایی که کاربر در آنها تخصیص یافته یا عضو تیم است.</summary>
    Task<IReadOnlyList<Project>> GetAssignedToUserAsync(int userId, CancellationToken cancellationToken = default);

    Task AssignDeveloperAsync(int projectId, int developerId, CancellationToken cancellationToken = default);

    /// <summary>تازه‌ترین پروژه‌های منتشرشده برای فید فعالیت.</summary>
    Task<IReadOnlyList<Project>> GetRecentPublishedAsync(int count, CancellationToken cancellationToken = default);
}
