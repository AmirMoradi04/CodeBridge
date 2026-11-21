namespace CodeBridge.Service.DTOs.User;

public sealed class UpdateProfileDto
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? Bio { get; init; }

    /// <summary>شناسه‌ی فایل پیوست آواتار کاربر.</summary>
    public int? ProfilePictureAttachmentId { get; init; }

    /// <summary>شناسه‌ی فایل پیوست لوگوی شرکت.</summary>
    public int? CompanyLogoAttachmentId { get; init; }
}
