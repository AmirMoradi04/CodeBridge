using CodeBridge.Domain.Common;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// اعلان درون‌برنامه‌ای برای یک کاربر؛ هر اعلان در نهایت با MarkAsRead خوانده می‌شود.
/// </summary>
public class Notification : BaseEntity
{
    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public bool IsRead { get; private set; }

    /// <summary>مسیر مقصد در فرانت برای کلیک روی اعلان.</summary>
    public string? Link { get; private set; }

    public NotificationType Type { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private Notification()
    {
    }

    public Notification(
        int userId,
        string title,
        string message,
        NotificationType type,
        string? link = null)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        Link = link;
        IsRead = false;

        Validate();
    }

    /// <summary>علامت‌گذاری به‌عنوان خوانده‌شده؛ تکرار عملیات بی‌اثر است.</summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        SetUpdatedAt();
    }

    public override void Validate()
    {
        if (UserId <= 0)
            throw new InvalidOperationException("گیرنده‌ی اعلان معتبر نیست.");

        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان اعلان الزامی است.");

        if (string.IsNullOrWhiteSpace(Message))
            throw new InvalidOperationException("متن اعلان الزامی است.");
    }
}
