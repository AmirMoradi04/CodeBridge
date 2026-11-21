namespace CodeBridge.Service.Interfaces;

/// <summary>انتزاع ذخیره‌سازی فایل؛ خود فایل هرگز در دیتابیس ذخیره نمی‌شود.</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string path, CancellationToken cancellationToken);

    /// <summary>باز کردن فایل برای دانلود؛ مسیر ذخیره‌سازی از بیرون وب سرو نمی‌شود.</summary>
    Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken);
}
