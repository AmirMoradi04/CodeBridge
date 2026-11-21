using System.Linq.Expressions;
using CodeBridge.Domain.Common;

namespace CodeBridge.Domain.Interfaces.Repositories;

/// <summary>
/// قرارداد عمومی ریپازیتوری برای انتیتی‌های قابل ممیزی.
/// فقط عملیات پایه‌ی داده را توصیف می‌کند و هیچ وابستگی‌ای به EF Core ندارد.
/// </summary>
public interface IGenericRepository<T> where T : class, IAuditableEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);

    /// <summary>ثبت تغییرات انباشته‌شده؛ پیاده‌سازی در لایه‌ی Repository انجام می‌شود.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>دسترسی به کوئری برای سناریوهای پیچیده‌تر در لایه‌ی سرویس.</summary>
    IQueryable<T> Query();
}
