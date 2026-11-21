using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.AuditLog;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ثبت و مشاهده‌ی گزارش‌های ممیزی.</summary>
public interface IAuditLogService
{
    Task<Result> LogAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AuditLogDto>>> GetByUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AuditLogDto>>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);
}
