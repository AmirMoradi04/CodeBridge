namespace CodeBridge.Service.DTOs.Post;

public sealed class PostDto
{
    public int Id { get; init; }

    public string Content { get; init; } = string.Empty;

    public int AuthorId { get; init; }

    public string? AuthorName { get; init; }

    public int? AttachmentId { get; init; }

    public bool IsPublished { get; init; }

    public int LikesCount { get; init; }

    public int CommentsCount { get; init; }

    public DateTime CreatedAt { get; init; }
}
