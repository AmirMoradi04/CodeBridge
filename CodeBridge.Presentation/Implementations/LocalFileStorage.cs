using CodeBridge.Service.Interfaces;

namespace CodeBridge.Presentation.Implementations;

/// <summary>
/// ذخیره‌ی فایل‌ها روی دیسک محلی، بیرون از wwwroot؛ بنابراین فایل‌ها فقط از طریق
/// endpoint احراز هویت‌شده‌ی دانلود قابل دسترسی‌اند و لینک عمومی ندارند.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private const string UploadFolder = "uploads";

    private readonly string _rootPath;

    public LocalFileStorage(IWebHostEnvironment environment, IConfiguration configuration)
    {
        // مسیر پیش‌فرض زیر App_Data است که ASP.NET Core آن را استاتیک سرو نمی‌کند
        var configuredRoot = configuration["Storage:RootPath"];

        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(environment.ContentRootPath, "App_Data", UploadFolder)
            : configuredRoot;

        _rootPath = Path.GetFullPath(root);
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Directory.CreateDirectory(_rootPath);

        // نام فایل یکتا می‌شود تا فایل‌ها روی هم بازنویسی نشوند
        var safeExtension = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid():N}{safeExtension}";
        var physicalPath = Path.Combine(_rootPath, storedName);

        await using (var fileStream = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{UploadFolder}/{storedName}";
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(path);

        if (string.IsNullOrWhiteSpace(fileName))
            return Task.CompletedTask;

        var physicalPath = Path.Combine(_rootPath, fileName);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<Stream?>(null);

        // فقط نام فایل استفاده می‌شود تا مسیر از پوشه‌ی ذخیره‌سازی خارج نشود
        var fileName = Path.GetFileName(path);

        if (string.IsNullOrWhiteSpace(fileName))
            return Task.FromResult<Stream?>(null);

        var physicalPath = Path.Combine(_rootPath, fileName);

        if (!File.Exists(physicalPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            physicalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }
}
