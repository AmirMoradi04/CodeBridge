using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Messaging;

/// <summary>مکالمه به همراه شرکت‌کنندگان.</summary>
public sealed class ConversationDto
{
    public int Id { get; init; }

    public ConversationType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public int? ProjectId { get; init; }

    public IReadOnlyList<ConversationParticipantDto> Participants { get; init; }
        = Array.Empty<ConversationParticipantDto>();

    /// <summary>تعداد پیام‌های خوانده‌نشده برای کاربر جاری.</summary>
    public int UnreadCount { get; set; }

    public DateTime CreatedAt { get; init; }
}