using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities.Identity;

namespace CodeBridge.Domain.Entities;

/// <summary>عضویت یک کاربر در یک مکالمه؛ هر کاربر فقط یک بار در هر مکالمه ثبت می‌شود.</summary>
public class ConversationParticipant : BaseEntity
{
    public int ConversationId { get; private set; }

    public int UserId { get; private set; }

    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>زمان آخرین بازدید کاربر از مکالمه.</summary>
    public DateTime? LastReadAt { get; private set; }

    // --- Navigation Properties ---
    public Conversation Conversation { get; private set; } = null!;

    public User User { get; private set; } = null!;

    // سازنده‌ی بدون پارامتر برای EF Core
    private ConversationParticipant()
    {
    }

    public ConversationParticipant(int conversationId, int userId)
    {
        ConversationId = conversationId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;

        Validate();
    }

    public override void Validate()
    {
        if (ConversationId <= 0)
            throw new InvalidOperationException("شناسه‌ی مکالمه معتبر نیست.");

        if (UserId <= 0)
            throw new InvalidOperationException("شناسه‌ی کاربر معتبر نیست.");
    }

    /// <summary>به‌روزرسانی زمان آخرین بازدید.</summary>
    public void MarkAsRead()
    {
        LastReadAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}