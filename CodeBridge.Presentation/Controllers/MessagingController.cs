using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Messaging;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>
/// گفت‌وگوی ادمین با کاربر و گروه‌های گفت‌وگوی پروژه؛ همه‌ی منطق در IMessagingService است.
/// </summary>
[Route("api/[controller]")]
public class MessagingController : BaseApiController
{
    private readonly IMessagingService _messagingService;

    public MessagingController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    /// <summary>مکالمه‌های کاربر جاری (هر دو نوع AdminUser و ProjectGroup).</summary>
    [HttpGet("conversations")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations(
        CancellationToken cancellationToken)
        => Handle(await _messagingService.GetMyConversationsAsync(CurrentUserId(), cancellationToken));

    /// <summary>شروع مکالمه؛ فقط ادمین.</summary>
    [HttpPost("conversations")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ConversationDto>> StartConversation(
        [FromBody] CreateConversationDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest(new { isSuccess = false, message = "اطلاعات مکالمه الزامی است." });

        // هر نوع مکالمه ورودی مخصوص به خود را می‌گیرد
        var result = dto.Type switch
        {
            ConversationType.AdminUser => await _messagingService.CreateDirectAsync(
                CurrentUserId(),
                dto.UserId ?? 0,
                cancellationToken),

            ConversationType.ProjectGroup => await _messagingService.CreateProjectGroupAsync(
                dto.ProjectId ?? 0,
                CurrentUserId(),
                cancellationToken),

            _ => Result<ConversationDto>.Failure("نوع مکالمه نامعتبر است.")
        };

        return Handle(result);
    }

    [HttpGet("conversations/{id:int}")]
    [Authorize]
    public async Task<ActionResult<ConversationDto>> GetConversation(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _messagingService.GetByIdAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>پیام‌های یک مکالمه؛ فقط برای شرکت‌کنندگان.</summary>
    /// <summary>علامت‌گذاری مکالمه به‌عنوان خوانده‌شده برای کاربر جاری.</summary>
    [HttpPost("conversations/{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _messagingService.MarkAsReadAsync(
            id,
            CurrentUserId(),
            cancellationToken));

    [HttpGet("conversations/{id:int}/messages")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessages(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
        => Handle(await _messagingService.GetMessagesAsync(
            id,
            CurrentUserId(),
            page,
            pageSize,
            cancellationToken));

    /// <summary>ارسال پیام؛ فقط برای شرکت‌کنندگان مکالمه.</summary>
    [HttpPost("messages")]
    [Authorize]
    public async Task<ActionResult<MessageDto>> SendMessage(
        [FromBody] SendMessageDto dto,
        CancellationToken cancellationToken)
        => Handle(await _messagingService.SendMessageAsync(CurrentUserId(), dto, cancellationToken));

    /// <summary>افزودن کاربر به گروه پروژه؛ فقط ادمین.</summary>
    [HttpPost("conversations/{id:int}/participants")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ConversationDto>> AddParticipant(
        int id,
        [FromQuery] int userId,
        CancellationToken cancellationToken)
        => Handle(await _messagingService.AddUserToProjectGroupAsync(
            id,
            userId,
            CurrentUserId(),
            cancellationToken));
}