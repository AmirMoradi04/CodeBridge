using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Achievement;

namespace CodeBridge.Service.Interfaces;

/// <summary>ارزیابی و اهدای دستاوردها (نشان‌ها) به کاربران.</summary>
public interface IAchievementService
{
    /// <summary>بررسی قواعد و اهدای دستاوردهای تازه به کاربر؛ تکرار آن بی‌خطر است.</summary>
    Task<Result> CheckAndAwardAchievementsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AchievementDto>>> GetUserAchievementsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
