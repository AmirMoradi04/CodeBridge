namespace CodeBridge.Service.Common;

/// <summary>نتیجه‌ی دارای داده.</summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(
        bool isSuccess,
        T? data = default,
        string? message = null,
        List<string>? errors = null) : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data);

    public static new Result<T> Failure(string message) => new(false, message: message);

    public static new Result<T> Failure(List<string> errors) => new(false, errors: errors);
}
