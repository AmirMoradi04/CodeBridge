using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Messaging;

/// <summary>
/// شروع مکالمه: برای AdminUser باید UserId و برای ProjectGroup باید ProjectId مقدار داشته باشد.
/// </summary>
public sealed class CreateConversationDto
{
    public ConversationType Type { get; init; }

    public int? UserId { get; init; }

    public int? ProjectId { get; init; }
}