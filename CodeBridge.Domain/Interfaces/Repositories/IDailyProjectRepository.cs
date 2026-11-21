using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای پروژه‌های روزمره.</summary>
public interface IDailyProjectRepository : IGenericRepository<DailyProject>
{
    Task<IReadOnlyList<DailyProject>> GetByEmployerIdAsync(
        int employerId,
        CancellationToken cancellationToken = default);

    /// <summary>پروژه‌های فعال با ظرفیت باقی‌مانده برای توسعه‌دهندگان.</summary>
    Task<IReadOnlyList<DailyProject>> GetActiveForDevelopersAsync(CancellationToken cancellationToken = default);    Task<IReadOnlyList<DailyProject>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyProject>> GetApprovedAsync(
        CancellationToken cancellationToken = default);

    Task<DailyProject?> GetWithApplicationsAsync(int projectId, CancellationToken cancellationToken = default);
}
