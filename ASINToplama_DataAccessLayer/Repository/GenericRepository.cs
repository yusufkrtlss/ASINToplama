using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_DataAccessLayer.EntityFramework.Context;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASINToplama_DataAccessLayer.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _set;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public IQueryable<T> Query(bool asNoTracking = true)
            => asNoTracking ? _set.AsNoTracking() : _set;

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _set.FindAsync([id], ct);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.AnyAsync(predicate, ct);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? await _set.CountAsync(ct) : await _set.CountAsync(predicate, ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await _set.AddRangeAsync(entities, ct);

        public void Update(T entity) => _set.Update(entity);

        public void Remove(T entity)
        {
            // Soft delete desteği: BaseEntity varsa IsDeleted işaretle
            if (entity is BaseEntity be)
            {
                be.IsDeleted = true;
                be.UpdatedAtUtc = DateTime.UtcNow;
                _set.Update(entity);
            }
            else
            {
                _set.Remove(entity);
            }
        }

        public async Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate,
            int page, int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            IQueryable<T> q = _set;

            if (asNoTracking) q = q.AsNoTracking();
            if (predicate is not null) q = q.Where(predicate);
            if (!string.IsNullOrWhiteSpace(include))
            {
                foreach (var path in include.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    q = q.Include(path);
            }
            if (orderBy is not null) q = orderBy(q);

            return await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        }
    }
}
