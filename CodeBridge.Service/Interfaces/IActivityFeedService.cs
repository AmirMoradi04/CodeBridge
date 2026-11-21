using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Activity;

namespace CodeBridge.Service.Interfaces;

/// <summary>فید فعالیت زنده‌ی سایت: رویدادهای اخیر از چند بخش مختلف.</summary>
public interface IActivityFeedService
{
    Task<Result<IReadOnlyList<ActivityFeedDto>>> GetRecentActivitiesAsync(
        int count,
        CancellationToken cancellationToken = default);
}
