namespace CodeBridge.Service.DTOs.Messaging;

public sealed class SendMessageDto
{
    public int ConversationId { get; init; }

    public string Content { get; init; } = string.Empty;
}