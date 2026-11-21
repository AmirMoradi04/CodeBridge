using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Middlewares;

/// <summary>
/// تبدیل استثناهای لایه‌ی سرویس به پاسخ‌های استاندارد HTTP
/// تا کنترلرها فقط مسئول مسیریابی باشند.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "موردی یافت نشد", exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "درخواست نامعتبر", exception.Message);
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "درخواست نامعتبر", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطای پیش‌بینی‌نشده در پردازش درخواست");

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "خطای داخلی سرور",
                "در پردازش درخواست خطایی رخ داد.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        });
    }
}
