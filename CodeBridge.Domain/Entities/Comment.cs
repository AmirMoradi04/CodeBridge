using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// دیدگاه روی پست؛ با ParentCommentId امکان پاسخ تودرتو فراهم می‌شود.
/// </summary>
public class Comment : BaseEntity
{
    public string Content { get; private set; } = string.Empty;

    public int PostId { get; private set; }

    public int AuthorId { get; private set; }

    public int? ParentCommentId { get; private set; }

    // --- Navigation Properties ---
    public Post Post { get; private set; } = null!;

    public User Author { get; private set; } = null!;

    public Comment? ParentComment { get; private set; }

    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private Comment()
    {
    }

    public Comment(int postId, int authorId, string content, int? parentCommentId = null)
    {
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Content))
            throw new InvalidOperationException("متن دیدگاه الزامی است.");
    }
}
