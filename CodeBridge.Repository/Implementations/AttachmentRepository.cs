using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری فایل‌های پیوست؛ فقط دسترسی به داده.</summary>
public sealed class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Attachment?> GetByRelatedEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Where(attachment => attachment.RelatedEntityType == entityType
                && attachment.RelatedEntityId == entityId)
            .OrderByDescending(attachment => attachment.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Attachment>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(attachment => attachment.OwnerId == ownerId)
            .OrderByDescending(attachment => attachment.CreatedAt)
            .ToListAsync(cancellationToken);
}
