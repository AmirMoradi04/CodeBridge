using CodeBridge.Domain.Entities;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>قرارداد عملیات داده‌ای فایل‌های پیوست.</summary>
public interface IAttachmentRepository : IGenericRepository<Attachment>
{
    Task<Attachment?> GetByRelatedEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attachment>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default);
}
