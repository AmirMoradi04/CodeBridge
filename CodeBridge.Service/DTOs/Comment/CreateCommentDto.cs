namespace CodeBridge.Service.DTOs.Comment;

public sealed class CreateCommentDto
{
    public int PostId { get; init; }

    public string Content { get; init; } = string.Empty;

    public int? ParentCommentId { get; init; }
}
