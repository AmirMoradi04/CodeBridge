using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>دسترسی به شرکت‌کنندگان مکالمه.</summary>
public interface IConversationParticipantRepository : IGenericRepository<ConversationParticipant>
{
    /// <summary>شرکت‌کنندگان یک مکالمه.</summary>
    Task<IReadOnlyList<ConversationParticipant>> GetByConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>عضویت مشخص‌شده‌ی کاربر در مکالمه.</summary>
    Task<ConversationParticipant?> GetByUserAndConversationAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>افزودن عضو به مکالمه در صورت نبودن؛ تغییرات باید جداگانه ذخیره شوند.</summary>
    Task<bool> AddIfMissingAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
}