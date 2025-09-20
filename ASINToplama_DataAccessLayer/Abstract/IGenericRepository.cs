using System.Linq.Expressions;

namespace ASINToplama_DataAccessLayer.Abstract
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        void Update(T entity);
        void Remove(T entity); // Soft delete ise burada işaretleyeceğiz

        // Esnek sorgular için güvenli giriş noktası:
        IQueryable<T> Query(bool asNoTracking = true);

        // Basit sayfalama yardımcıları
        Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate,
            int page, int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? include = null, // "Nav1,Nav2.SubNav" biçiminde
            bool asNoTracking = true,
            CancellationToken ct = default);
    }
}
