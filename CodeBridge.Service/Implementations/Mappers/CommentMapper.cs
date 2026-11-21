using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Comment;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class CommentMapper
{
    public static CommentDto ToDto(Comment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        PostId = comment.PostId,
        AuthorId = comment.AuthorId,
        AuthorName = comment.Author is null ? null : UserMapper.DisplayName(comment.Author),
        ParentCommentId = comment.ParentCommentId,
        CreatedAt = comment.CreatedAt
    };
}
