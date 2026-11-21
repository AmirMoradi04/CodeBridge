using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای رزومه‌ها.</summary>
public interface IResumeRepository : IGenericRepository<Resume>
{
    Task<Resume?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Resume?> GetWithAttachmentAsync(int resumeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resume>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resume>> GetApprovedAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resume>> GetByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task ApproveResumeAsync(int resumeId, int adminId, CancellationToken cancellationToken = default);

    Task RejectResumeAsync(int resumeId, int adminId, CancellationToken cancellationToken = default);
}
