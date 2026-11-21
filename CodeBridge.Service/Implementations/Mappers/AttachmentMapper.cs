using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Attachment;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class AttachmentMapper
{
    public static AttachmentDto ToDto(Attachment attachment) => new()
    {
        Id = attachment.Id,
        FileName = attachment.FileName,
        FilePath = attachment.FilePath,
        ContentType = attachment.ContentType,
        FileSize = attachment.FileSize,
        OwnerId = attachment.OwnerId,
        RelatedEntityType = attachment.RelatedEntityType,
        RelatedEntityId = attachment.RelatedEntityId,
        CreatedAt = attachment.CreatedAt
    };
}
