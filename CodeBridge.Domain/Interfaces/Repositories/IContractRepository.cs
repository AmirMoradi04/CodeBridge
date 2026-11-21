using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای قراردادها.</summary>
public interface IContractRepository : IGenericRepository<Contract>
{
    Task<Contract?> GetWithDetailsAsync(int contractId, CancellationToken cancellationToken = default);

    Task<Contract?> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contract>> GetByDeveloperIdAsync(
        int developerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contract>> GetByEmployerIdAsync(
        int employerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contract>> GetActiveAsync(CancellationToken cancellationToken = default);
}
