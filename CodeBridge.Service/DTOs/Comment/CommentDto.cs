namespace CodeBridge.Service.DTOs.Comment;

public sealed class CommentDto
{
    public int Id { get; init; }

    public string Content { get; init; } = string.Empty;

    public int PostId { get; init; }

    public int AuthorId { get; init; }

    public string? AuthorName { get; init; }

    public int? ParentCommentId { get; init; }

    public DateTime CreatedAt { get; init; }
}
