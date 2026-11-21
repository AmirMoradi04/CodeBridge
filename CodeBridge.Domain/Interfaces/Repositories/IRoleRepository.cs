using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای نقش‌ها.</summary>
public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);
}
