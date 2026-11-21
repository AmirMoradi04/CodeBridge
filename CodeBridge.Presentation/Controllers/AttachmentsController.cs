using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Attachment;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// آپلود و مدیریت متادیتای فایل؛ خود فایل روی Storage ذخیره می‌شود.
/// مالک فایل از توکن خوانده می‌شود.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : BaseApiController
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>آپلود فایل؛ relatedEntityType مثلاً Resume، CompanyLogo یا PostImage.</summary>
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<AttachmentDto>> Upload(
        IFormFile file,
        [FromQuery] string relatedEntityType,
        [FromQuery] int? relatedEntityId,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { isSuccess = false, message = "فایلی برای آپلود ارسال نشده است." });

        if (string.IsNullOrWhiteSpace(relatedEntityType))
            return BadRequest(new { isSuccess = false, message = "نوع موجودیت مرتبط الزامی است." });

        await using var stream = file.OpenReadStream();

        return Handle(await _attachmentService.UploadAsync(
            CurrentUserId(),
            stream,
            file.FileName,
            file.ContentType,
            relatedEntityType,
            relatedEntityId,
            cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AttachmentDto>> GetById(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _attachmentService.GetByIdAsync(
            id,
            CurrentUserId(),
            CurrentUserRole() == UserRole.Admin,
            cancellationToken));

    /// <summary>دانلود فایل پیوست؛ فقط صاحب فایل یا ادمین مجاز است.</summary>
    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> GetFile(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetFileAsync(
            id,
            CurrentUserId(),
            CurrentUserRole() == UserRole.Admin,
            cancellationToken);

        // دسترسی غیرمجاز و فایل ناموجود هر دو ۴۰۳ هستند تا وجود فایل افشا نشود
        if (!result.IsSuccess || result.Data is null)
            return StatusCode(StatusCodes.Status403Forbidden, result);

        var file = result.Data;

        // نام اصلی فایل در هدر می‌آید تا مرورگر با همان نام باز/ذخیره کند
        Response.Headers.ContentDisposition =
            $"inline; filename*=UTF-8''{Uri.EscapeDataString(file.FileName)}";

        return File(file.Content, file.ContentType, enableRangeProcessing: true);
    }

    /// <summary>فایل‌های کاربر جاری.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> GetMine(
        CancellationToken cancellationToken)
        => Handle(await _attachmentService.GetByOwnerAsync(CurrentUserId(), cancellationToken));

    /// <summary>حذف فایل؛ صاحب فایل یا ادمین.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _attachmentService.DeleteAsync(
            id,
            CurrentUserId(),
            CurrentUserRole() == UserRole.Admin,
            cancellationToken));
}
