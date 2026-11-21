using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// متادیتای فایل ذخیره‌شده در Storage؛ هیچ فایلی داخل دیتابیس نگهداری نمی‌شود.
/// </summary>
public class Attachment : BaseEntity
{
    public string FileName { get; private set; } = string.Empty;

    /// <summary>مسیر فایل روی Storage (نه محتوای فایل).</summary>
    public string FilePath { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long FileSize { get; private set; }

    public int OwnerId { get; private set; }

    /// <summary>نوع انتیتی مرتبط؛ مثلاً Resume، CompanyLogo یا PostImage.</summary>
    public string RelatedEntityType { get; private set; } = string.Empty;

    public int? RelatedEntityId { get; private set; }

    // --- Navigation Properties ---
    public User Owner { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private Attachment()
    {
    }

    public Attachment(
        int ownerId,
        string fileName,
        string filePath,
        string contentType,
        long fileSize,
        string relatedEntityType,
        int? relatedEntityId = null)
    {
        OwnerId = ownerId;
        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(FileName))
            throw new InvalidOperationException("نام فایل الزامی است.");

        if (string.IsNullOrWhiteSpace(FilePath))
            throw new InvalidOperationException("مسیر فایل الزامی است.");

        if (string.IsNullOrWhiteSpace(ContentType))
            throw new InvalidOperationException("نوع محتوای فایل الزامی است.");

        if (FileSize < 0)
            throw new InvalidOperationException("حجم فایل نمی‌تواند منفی باشد.");

        if (string.IsNullOrWhiteSpace(RelatedEntityType))
            throw new InvalidOperationException("نوع انتیتی مرتبط الزامی است.");
    }
}
