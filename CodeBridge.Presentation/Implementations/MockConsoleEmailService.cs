using CodeBridge.Service.Interfaces;

namespace CodeBridge.Presentation.Implementations;

/// <summary>
/// پیاده‌سازی موقت ارسال ایمیل؛ به‌جای ارسال واقعی، ایمیل در لاگ ثبت می‌شود.
/// برای اتصال به سرویس ایمیل واقعی کافی است این کلاس جایگزین شود.
/// </summary>
public sealed class MockConsoleEmailService : IEmailService
{
    private readonly ILogger<MockConsoleEmailService> _logger;

    public MockConsoleEmailService(ILogger<MockConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MOCK EMAIL] To={To} | Subject={Subject} | Body={Body}",
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}
