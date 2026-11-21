using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Notification;

namespace CodeBridge.Service.Implementations.Mappers;

/// <summary>تبدیل انتیتی اعلان به DTO.</summary>
public static class NotificationMapper
{
    public static NotificationDto ToDto(Notification notification)
        => new()
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            Link = notification.Link,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt
        };
}
