using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای شرکت‌کنندگان جلسه‌ی زنده.</summary>
public interface ILiveSessionParticipantRepository : IGenericRepository<LiveSessionParticipant>
{
    Task<LiveSessionParticipant?> GetBySessionAndJuniorAsync(
        int sessionId,
        int juniorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LiveSessionParticipant>> GetBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);
}
