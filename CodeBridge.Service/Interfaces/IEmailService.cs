namespace CodeBridge.Service.Interfaces;

/// <summary>انتزاع ارسال ایمیل؛ برای توسعه پیاده‌سازی mock لاگ‌کننده استفاده می‌شود.</summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
