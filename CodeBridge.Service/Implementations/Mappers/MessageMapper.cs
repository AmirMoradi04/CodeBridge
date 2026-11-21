using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Messaging;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class MessageMapper
{
    public static MessageDto ToDto(Message message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        SenderId = message.SenderId,
        SenderName = UserMapper.DisplayName(message.Sender),
        Content = message.Content,
        CreatedAt = message.CreatedAt
    };
}