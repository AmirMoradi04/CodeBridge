using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Attachment;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>آپلود و مدیریت متادیتای فایل؛ خود فایل در دیتابیس ذخیره نمی‌شود.</summary>
public sealed class AttachmentService : IAttachmentService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    // انواع محتوای مجاز برای پیوست
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/png",
        "image/jpeg"
    };

    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IAuditLogRepository _auditLogRepository;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IFileStorage fileStorage,
        IAuditLogRepository auditLogRepository)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    public async Task<Result<AttachmentDto>> UploadAsync(
        int ownerId,
        Stream stream,
        string fileName,
        string contentType,
        string relatedEntityType,
        int? relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        if (stream is null || !stream.CanRead)
            return Result<AttachmentDto>.Failure("جریان فایل معتبر نیست.");

        if (string.IsNullOrWhiteSpace(fileName))
            return Result<AttachmentDto>.Failure("نام فایل الزامی است.");

        var safeContentType = contentType ?? string.Empty;

        if (!AllowedContentTypes.Contains(safeContentType))
            return Result<AttachmentDto>.Failure(
                "نوع فایل مجاز نیست؛ فقط pdf، docx، png و jpg پذیرفته می‌شود.");

        if (string.IsNullOrWhiteSpace(relatedEntityType))
            return Result<AttachmentDto>.Failure("نوع انتیتی مرتبط الزامی است.");

        MemoryStream? buffer = null;

        try
        {
            // برای جریان‌های غیرقابل‌جستجو، محتوا در حافظه بافر می‌شود تا حجم قابل بررسی باشد
            if (!stream.CanSeek)
            {
                buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, cancellationToken);
                buffer.Position = 0;
            }

            var content = buffer ?? stream;

            if (content.Length == 0)
                return Result<AttachmentDto>.Failure("فایل خالی است.");

            if (content.Length > MaxFileSize)
                return Result<AttachmentDto>.Failure("حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.");

            var path = await _fileStorage.SaveAsync(content, fileName, safeContentType, cancellationToken);

            var attachment = new Attachment(
                ownerId,
                fileName,
                path,
                safeContentType,
                content.Length,
                relatedEntityType,
                relatedEntityId);

            await _attachmentRepository.AddAsync(attachment, cancellationToken);
            await _attachmentRepository.SaveChangesAsync(cancellationToken);

            await AddAuditLogAsync(ownerId, "AttachmentUploaded", attachment.Id, cancellationToken);

            return Result<AttachmentDto>.Success(AttachmentMapper.ToDto(attachment));
        }
        finally
        {
            buffer?.Dispose();
        }
    }

    public async Task<Result<AttachmentDto>> GetByIdAsync(
        int attachmentId,
        int requesterId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment is null)
            return Result<AttachmentDto>.Failure("فایل پیوست پیدا نشد.");

        // متادیتای فایل هم مثل خود فایل فقط برای صاحب آن و ادمین قابل مشاهده است
        if (!HasAccess(attachment, requesterId, isAdmin))
            return Result<AttachmentDto>.Failure("دسترسی به این فایل پیوست مجاز نیست.");

        return Result<AttachmentDto>.Success(AttachmentMapper.ToDto(attachment));
    }

    public async Task<Result<AttachmentFileDto>> GetFileAsync(
        int attachmentId,
        int requesterId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment is null)
            return Result<AttachmentFileDto>.Failure("فایل پیوست پیدا نشد.");

        // فقط صاحب رزومه یا ادمین می‌تواند فایل را ببیند
        if (!HasAccess(attachment, requesterId, isAdmin))
            return Result<AttachmentFileDto>.Failure("دسترسی به این فایل پیوست مجاز نیست.");

        var stream = await _fileStorage.OpenReadAsync(attachment.FilePath, cancellationToken);

        if (stream is null)
            return Result<AttachmentFileDto>.Failure("فایل روی سرور موجود نیست.");

        return Result<AttachmentFileDto>.Success(new AttachmentFileDto
        {
            Content = stream,
            FileName = attachment.FileName,
            ContentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                ? "application/octet-stream"
                : attachment.ContentType,
            FileSize = attachment.FileSize
        });
    }

    private static bool HasAccess(Attachment attachment, int requesterId, bool isAdmin)
        => isAdmin || attachment.OwnerId == requesterId;

    public async Task<Result<IReadOnlyList<AttachmentDto>>> GetByOwnerAsync(
        int ownerId,
        CancellationToken cancellationToken = default)
    {
        var attachments = await _attachmentRepository.GetByOwnerIdAsync(ownerId, cancellationToken);

        return Result<IReadOnlyList<AttachmentDto>>.Success(
            attachments.Select(AttachmentMapper.ToDto).ToList());
    }

    public async Task<Result> DeleteAsync(
        int attachmentId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment is null)
            return Result.Failure("فایل پیوست پیدا نشد.");

        // فقط صاحب فایل یا ادمین مجاز به حذف است
        if (!HasAccess(attachment, userId, isAdmin))
            return Result.Failure("فقط صاحب فایل یا ادمین می‌تواند آن را حذف کند.");

        await _fileStorage.DeleteAsync(attachment.FilePath, cancellationToken);

        _attachmentRepository.Remove(attachment);
        await _attachmentRepository.SaveChangesAsync(cancellationToken);

        await AddAuditLogAsync(userId, "AttachmentDeleted", attachment.Id, cancellationToken);

        return Result.Success();
    }

    private async Task AddAuditLogAsync(
        int userId,
        string action,
        int attachmentId,
        CancellationToken cancellationToken)
    {
        var log = new AuditLog(action, nameof(Attachment), userId, attachmentId);

        await _auditLogRepository.AddAsync(log, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);
    }
}
