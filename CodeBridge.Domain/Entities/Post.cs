using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// پست شبکه‌ی اجتماعی؛ فایل پیوست آن از طریق Attachment مدیریت می‌شود.
/// </summary>
public class Post : BaseEntity
{
    public string Content { get; private set; } = string.Empty;

    public int AuthorId { get; private set; }

    public int? AttachmentId { get; private set; }

    public bool IsPublished { get; private set; }

    // --- Navigation Properties ---
    public User Author { get; private set; } = null!;

    public Attachment? Attachment { get; private set; }

    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    public ICollection<Like> Likes { get; private set; } = new List<Like>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private Post()
    {
    }

    public Post(int authorId, string content, int? attachmentId = null)
    {
        AuthorId = authorId;
        Content = content;
        AttachmentId = attachmentId;
        IsPublished = false;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Content))
            throw new InvalidOperationException("متن پست الزامی است.");
    }

    public void UpdateContent(string content, int? attachmentId = null)
    {
        Content = content;
        AttachmentId = attachmentId;

        Validate();
        SetUpdatedAt();
    }

    /// <summary>انتشار پست برای مشاهده‌ی عمومی.</summary>
    public void Publish()
    {
        IsPublished = true;
        SetUpdatedAt();
    }
}
