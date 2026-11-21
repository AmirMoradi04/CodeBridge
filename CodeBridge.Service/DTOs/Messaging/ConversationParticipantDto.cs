namespace CodeBridge.Service.DTOs.Messaging;

public sealed class ConversationParticipantDto
{
    public int UserId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime JoinedAt { get; init; }
}