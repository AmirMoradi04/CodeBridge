using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای دستاوردها.</summary>
public interface IAchievementRepository : IGenericRepository<Achievement>
{
    /// <summary>دریافت همه‌ی دستاوردهای تعریف‌شده برای کاشت یا ارزیابی قواعد.</summary>
    Task<IReadOnlyList<Achievement>> GetAllSeededAsync(CancellationToken cancellationToken = default);
}
