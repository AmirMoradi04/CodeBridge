using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Attachment;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد آپلود و مدیریت متادیتای فایل.</summary>
public interface IAttachmentService
{
    Task<Result<AttachmentDto>> UploadAsync(
        int ownerId,
        Stream stream,
        string fileName,
        string contentType,
        string relatedEntityType,
        int? relatedEntityId,
        CancellationToken cancellationToken = default);

    Task<Result<AttachmentDto>> GetByIdAsync(
        int attachmentId,
        int requesterId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>محتوای فایل برای دانلود؛ فقط صاحب فایل یا ادمین مجاز است.</summary>
    Task<Result<AttachmentFileDto>> GetFileAsync(
        int attachmentId,
        int requesterId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AttachmentDto>>> GetByOwnerAsync(
        int ownerId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        int attachmentId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
