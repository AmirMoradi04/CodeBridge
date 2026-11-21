using System.Linq.Expressions;
using CodeBridge.Domain.Common;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>
/// پیاده‌سازی پایه‌ی ریپازیتوری با EF Core.
/// متدهای نوشتن تغییرات را ذخیره نمی‌کنند؛ SaveChangesAsync جداگانه فراخوانی می‌شود.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class, IAuditableEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(AppDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(predicate, cancellationToken);

    public virtual Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        => predicate is null
            ? DbSet.CountAsync(cancellationToken)
            : DbSet.CountAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = Context.Entry(entity);

        if (entry.State != EntityState.Detached)
        {
            entry.State = EntityState.Modified;
            return;
        }

        // اگر نسخه‌ی دیگری از همین رکورد در ChangeTracker باشد، مقادیر روی آن کپی می‌شود
        // تا خطای ردیابی هم‌زمان دو نمونه رخ ندهد.
        var tracked = DbSet.Local.FirstOrDefault(item => item.Id == entity.Id);

        if (tracked is not null)
        {
            Context.Entry(tracked).CurrentValues.SetValues(entity);
            return;
        }

        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);

    public virtual IQueryable<T> Query() => DbSet.AsQueryable();
}
