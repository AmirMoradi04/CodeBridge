using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.AuditLog;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>ثبت و بازیابی گزارش‌های ممیزی.</summary>
public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    public async Task<Result> LogAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure("نوع عملیات برای ثبت در AuditLog الزامی است.");

        var log = new AuditLog(action, entityType, userId, entityId, details, ipAddress);

        await _auditLogRepository.AddAsync(log, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(userId, cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(logs.Select(AuditLogMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return Result<IReadOnlyList<AuditLogDto>>.Failure("نوع انتیتی الزامی است.");

        var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(logs.Select(AuditLogMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            return Result<IReadOnlyList<AuditLogDto>>.Failure("تعداد باید بزرگ‌تر از صفر باشد.");

        var logs = await _auditLogRepository.GetRecentAsync(count, cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(logs.Select(AuditLogMapper.ToDto).ToList());
    }
}
