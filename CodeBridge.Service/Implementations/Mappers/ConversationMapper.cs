using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Messaging;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ConversationMapper
{
    public static ConversationDto ToDto(Conversation conversation) => new()
    {
        Id = conversation.Id,
        Type = conversation.Type,
        Title = conversation.Title,
        ProjectId = conversation.ProjectId,
        Participants = (conversation.Participants ?? new List<ConversationParticipant>())
            .Select(ParticipantToDto)
            .ToList(),
        CreatedAt = conversation.CreatedAt
    };

    public static ConversationParticipantDto ParticipantToDto(ConversationParticipant participant) => new()
    {
        UserId = participant.UserId,
        DisplayName = UserMapper.DisplayName(participant.User),
        Email = participant.User?.Email ?? string.Empty,
        JoinedAt = participant.JoinedAt
    };
}