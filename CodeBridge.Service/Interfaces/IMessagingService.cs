using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Messaging;

namespace CodeBridge.Service.Interfaces;

/// <summary>گفت‌وگوی ادمین با کاربر و گروه‌های گفت‌وگوی پروژه.</summary>
public interface IMessagingService
{
    /// <summary>شروع گفت‌وگوی مستقیم بین ادمین و یک کاربر.</summary>
    Task<Result<ConversationDto>> CreateDirectAsync(
        int adminId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>ساخت گروه گفت‌وگوی پروژه با اعضای تیم.</summary>
    Task<Result<ConversationDto>> CreateProjectGroupAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>افزودن کاربر به گروه گفت‌وگوی پروژه.</summary>
    Task<Result<ConversationDto>> AddUserToProjectGroupAsync(
        int conversationId,
        int userId,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>مکالمه‌هایی که کاربر در آنها عضو است (هر دو نوع).</summary>
    Task<Result<IReadOnlyList<ConversationDto>>> GetMyConversationsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<ConversationDto>> GetByIdAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>پیام‌های یک مکالمه؛ فقط برای شرکت‌کنندگان.</summary>
    Task<Result<IReadOnlyList<MessageDto>>> GetMessagesAsync(
        int conversationId,
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>علامت‌گذاری مکالمه به‌عنوان خوانده‌شده برای کاربر جاری.</summary>
    Task<Result> MarkAsReadAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>ارسال پیام در مکالمه‌ای که فرستنده در آن عضو است.</summary>
    Task<Result<MessageDto>> SendMessageAsync(
        int senderId,
        SendMessageDto dto,
        CancellationToken cancellationToken = default);
}