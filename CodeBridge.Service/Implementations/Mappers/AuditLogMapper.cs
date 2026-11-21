using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.AuditLog;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class AuditLogMapper
{
    public static AuditLogDto ToDto(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Details = log.Details,
        IpAddress = log.IpAddress,
        CreatedAt = log.CreatedAt
    };
}
