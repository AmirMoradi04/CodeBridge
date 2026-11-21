namespace CodeBridge.Service.DTOs.Attachment;

public sealed class AttachmentDto
{
    public int Id { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSize { get; init; }

    public int OwnerId { get; init; }

    public string RelatedEntityType { get; init; } = string.Empty;

    public int? RelatedEntityId { get; init; }

    public DateTime CreatedAt { get; init; }
}
