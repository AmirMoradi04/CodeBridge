namespace CodeBridge.Service.Common;

/// <summary>نتیجه‌ی یک عملیات سرویس بدون پرتاب استثنا برای خطاهای کسب‌وکار.</summary>
public class Result
{
    public bool IsSuccess { get; protected set; }

    public string? Message { get; protected set; }

    public List<string>? Errors { get; protected set; }

    protected Result(bool isSuccess, string? message = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public static Result Success() => new(true);

    public static Result Failure(string message) => new(false, message);

    public static Result Failure(List<string> errors) => new(false, errors: errors);
}
