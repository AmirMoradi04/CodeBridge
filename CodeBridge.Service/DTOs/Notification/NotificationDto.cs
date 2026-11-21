using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Notification;

/// <summary>اطلاعات نمایشی یک اعلان.</summary>
public sealed class NotificationDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public bool IsRead { get; init; }

    public string? Link { get; init; }

    public NotificationType Type { get; init; }

    public DateTime CreatedAt { get; init; }
}
