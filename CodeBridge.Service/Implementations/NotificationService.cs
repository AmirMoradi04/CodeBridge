using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Notification;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>سرویس اعلان‌ها: ارسال، خواندن و علامت‌گذاری خوانده‌شده.</summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository
            ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task SendAsync(
        int userId,
        string title,
        string message,
        NotificationType type,
        string? link = null,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            return;

        try
        {
            var notification = new Notification(userId, title, message, type, link);

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _notificationRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // اعلان جانبی است؛ خطای آن نباید عملیات اصلی (پرداخت/تخصیص) را نیمه‌کاره کند.
        }
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> GetUserNotificationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);

        IReadOnlyList<NotificationDto> dtos = notifications
            .Select(NotificationMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<NotificationDto>>.Success(dtos);
    }

    public async Task<Result<int>> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);

        return Result<int>.Success(count);
    }

    public async Task<Result> MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);

        return Result.Success();
    }
}
