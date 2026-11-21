using CodeBridge.Service.DTOs.Notification;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>اعلان‌های کاربر جاری؛ گیرنده همیشه از توکن خوانده می‌شود.</summary>
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>فهرست اعلان‌های کاربر جاری (جدیدترین اول).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMine(
        CancellationToken cancellationToken)
        => Handle(await _notificationService.GetUserNotificationsAsync(
            CurrentUserId(),
            cancellationToken));

    /// <summary>تعداد اعلان‌های خوانده‌نشده؛ برای نشان‌دادن نشان قرمز روی زنگ.</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
        => Handle(await _notificationService.GetUnreadCountAsync(
            CurrentUserId(),
            cancellationToken));

    /// <summary>خوانده‌شدن همه‌ی اعلان‌های کاربر جاری.</summary>
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        => Handle(await _notificationService.MarkAllAsReadAsync(
            CurrentUserId(),
            cancellationToken));
}
