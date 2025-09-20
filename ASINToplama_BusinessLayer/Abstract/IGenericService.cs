using System.Linq.Expressions;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface IGenericService<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate,
            int page, int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default);

        Task<T> CreateAsync(T entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default); 
    }
}
