namespace CodeBridge.Service.DTOs.Post;

public sealed class CreatePostDto
{
    public string Content { get; init; } = string.Empty;

    public int? AttachmentId { get; init; }
}
