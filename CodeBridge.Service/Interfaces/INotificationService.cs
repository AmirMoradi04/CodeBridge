using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Notification;

namespace CodeBridge.Service.Interfaces;

/// <summary>ارسال و مدیریت اعلان‌های کاربران.</summary>
public interface INotificationService
{
    /// <summary>ثبت یک اعلان برای کاربر؛ خطای اعلان نباید جریان اصلی را متوقف کند.</summary>
    Task SendAsync(
        int userId,
        string title,
        string message,
        NotificationType type,
        string? link = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NotificationDto>>> GetUserNotificationsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<int>> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result> MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
