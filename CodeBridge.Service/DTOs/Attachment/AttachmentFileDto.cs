namespace CodeBridge.Service.DTOs.Attachment;

/// <summary>محتوای فایل برای دانلود؛ همراه نام و نوع محتوا.</summary>
public sealed class AttachmentFileDto
{
    public Stream Content { get; init; } = Stream.Null;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/octet-stream";

    public long FileSize { get; init; }
}
