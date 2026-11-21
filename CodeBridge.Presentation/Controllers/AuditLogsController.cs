using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.AuditLog;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>گزارش‌های ممیزی؛ مشاهده فقط برای ادمین.</summary>
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetRecent(
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
        => Handle(await _auditLogService.GetRecentAsync(count, cancellationToken));

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetByUser(
        int userId,
        CancellationToken cancellationToken)
        => Handle(await _auditLogService.GetByUserAsync(userId, cancellationToken));

    [HttpGet("entity")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetByEntity(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken cancellationToken)
        => Handle(await _auditLogService.GetByEntityAsync(entityType, entityId, cancellationToken));
}
