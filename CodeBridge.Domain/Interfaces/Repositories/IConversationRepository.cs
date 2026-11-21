using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>دسترسی به مکالمه‌ها (گفت‌وگوی ادمین-کاربر و گروه پروژه).</summary>
public interface IConversationRepository : IGenericRepository<Conversation>
{
    /// <summary>مکالمه‌هایی که کاربر در آنها عضو است.</summary>
    Task<IReadOnlyList<Conversation>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>مکالمه به همراه شرکت‌کنندگان.</summary>
    Task<Conversation?> GetWithParticipantsAsync(int conversationId, CancellationToken cancellationToken = default);

    /// <summary>گروه گفت‌وگوی یک پروژه (در صورت وجود).</summary>
    Task<Conversation?> GetProjectGroupAsync(int projectId, CancellationToken cancellationToken = default);
}