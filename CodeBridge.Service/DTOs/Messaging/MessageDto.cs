namespace CodeBridge.Service.DTOs.Messaging;

public sealed class MessageDto
{
    public int Id { get; init; }

    public int ConversationId { get; init; }

    public int SenderId { get; init; }

    public string SenderName { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}