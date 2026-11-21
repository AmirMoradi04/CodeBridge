using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای جلسه‌های زنده.</summary>
public interface ILiveSessionRepository : IGenericRepository<LiveSession>
{
    /// <summary>جلسه همراه پروژه، برگزارکننده و شرکت‌کننده‌ها.</summary>
    Task<LiveSession?> GetWithDetailsAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>جلسه‌های یک برنامه‌نویس ارشد.</summary>
    Task<IReadOnlyList<LiveSession>> GetBySeniorIdAsync(
        int seniorId,
        CancellationToken cancellationToken = default);

    /// <summary>جلسه‌های در دسترس کارجویان (زمان‌بندی‌شده و در حال برگزاری).</summary>
    Task<IReadOnlyList<LiveSession>> GetAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>همه‌ی جلسه‌ها برای پایش ادمین.</summary>
    Task<IReadOnlyList<LiveSession>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
}
