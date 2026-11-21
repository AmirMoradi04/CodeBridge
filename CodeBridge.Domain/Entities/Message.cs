using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>پیام ارسال‌شده در یک مکالمه؛ فقط متن و متادیتا ذخیره می‌شود.</summary>
public class Message : BaseEntity
{
    public int ConversationId { get; private set; }

    public int SenderId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    // --- Navigation Properties ---
    public Conversation Conversation { get; private set; } = null!;

    public User Sender { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private Message()
    {
    }

    public Message(int conversationId, int senderId, string content)
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;

        Validate();
    }

    public override void Validate()
    {
        if (ConversationId <= 0)
            throw new InvalidOperationException("شناسه‌ی مکالمه معتبر نیست.");

        if (SenderId <= 0)
            throw new InvalidOperationException("شناسه‌ی فرستنده معتبر نیست.");

        if (string.IsNullOrWhiteSpace(Content))
            throw new InvalidOperationException("متن پیام الزامی است.");
    }
}