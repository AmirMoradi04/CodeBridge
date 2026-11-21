using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Post;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class PostMapper
{
    public static PostDto ToDto(Post post) => new()
    {
        Id = post.Id,
        Content = post.Content,
        AuthorId = post.AuthorId,
        AuthorName = post.Author is null ? null : UserMapper.DisplayName(post.Author),
        AttachmentId = post.AttachmentId,
        IsPublished = post.IsPublished,
        LikesCount = post.Likes?.Count ?? 0,
        CommentsCount = post.Comments?.Count ?? 0,
        CreatedAt = post.CreatedAt
    };
}
