using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای درخواست‌های پروژه‌ی روزمره.</summary>
public interface IDailyProjectApplicationRepository : IGenericRepository<DailyProjectApplication>
{
    Task<DailyProjectApplication?> GetByProjectAndDeveloperAsync(
        int projectId,
        int developerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyProjectApplication>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyProjectApplication>> GetByDeveloperIdAsync(
        int developerId,
        CancellationToken cancellationToken = default);

    /// <summary>درخواست‌های منقضی‌شده (برای پاک‌سازی دستی توسط ادمین).</summary>
    Task<IReadOnlyList<DailyProjectApplication>> GetExpiredAsync(CancellationToken cancellationToken = default);

    Task<int> CountApprovedForProjectAsync(int projectId, CancellationToken cancellationToken = default);
}
