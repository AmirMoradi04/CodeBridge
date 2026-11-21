namespace CodeBridge.Service.DTOs.AuditLog;

public sealed class AuditLogDto
{
    public int Id { get; init; }

    public int? UserId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string EntityType { get; init; } = string.Empty;

    public int? EntityId { get; init; }

    public string? Details { get; init; }

    public string? IpAddress { get; init; }

    public DateTime CreatedAt { get; init; }
}
