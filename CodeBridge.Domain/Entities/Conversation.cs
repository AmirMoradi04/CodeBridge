using CodeBridge.Domain.Common;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>
/// مکالمه: یا گفت‌وگوی مستقیم ادمین با کاربر، یا گروه گفت‌وگوی پروژه.
/// </summary>
public class Conversation : BaseEntity
{
    public ConversationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    /// <summary>فقط برای نوع ProjectGroup مقدار دارد.</summary>
    public int? ProjectId { get; private set; }

    // --- Navigation Properties ---
    public Project? Project { get; private set; }

    public ICollection<ConversationParticipant> Participants { get; private set; }
        = new List<ConversationParticipant>();

    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    // سازنده‌ی بدون پارامتر برای EF Core
    private Conversation()
    {
    }

    private Conversation(ConversationType type, string title, int? projectId)
    {
        Type = type;
        Title = title;
        ProjectId = projectId;

        Validate();
    }

    /// <summary>گفت‌وگوی مستقیم بین ادمین و یک کاربر.</summary>
    public static Conversation CreateAdminUser(string userDisplayName)
        => new(ConversationType.AdminUser, userDisplayName, null);

    /// <summary>گروه گفت‌وگوی یک پروژه.</summary>
    public static Conversation CreateProjectGroup(int projectId, string projectTitle)
        => new(ConversationType.ProjectGroup, projectTitle, projectId);

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان مکالمه الزامی است.");

        if (Type == ConversationType.ProjectGroup && ProjectId is null or <= 0)
            throw new InvalidOperationException("گروه پروژه باید به شناسه‌ی پروژه متصل باشد.");

        if (Type == ConversationType.AdminUser && ProjectId is not null)
            throw new InvalidOperationException("گفت‌وگوی ادمین-کاربر نمی‌تواند به پروژه متصل باشد.");
    }
}