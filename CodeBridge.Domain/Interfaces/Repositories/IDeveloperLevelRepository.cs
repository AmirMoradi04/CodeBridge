using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای سطح برنامه‌نویسان.</summary>
public interface IDeveloperLevelRepository : IGenericRepository<DeveloperLevel>
{
    Task<DeveloperLevel?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeveloperLevel>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeveloperLevel>> GetTopDevelopersAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task UpdateScoreAsync(int userId, int scoreDelta, CancellationToken cancellationToken = default);
}
