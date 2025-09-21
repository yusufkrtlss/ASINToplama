using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;
using System.Linq.Expressions;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repo;
        private readonly IUnitOfWork _uow;

        public GenericService(IGenericRepository<T> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

        public virtual Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate,
            int page, int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
            => _repo.GetPagedAsync(predicate, page, pageSize, orderBy, include, asNoTracking, ct);

        public virtual async Task<T> CreateAsync(T entity, CancellationToken ct = default)
        {
            // Basit kurallar: CreatedAt/UpdatedAt ayarı (varsa)
            if (entity is BaseEntity be)
            {
                be.CreatedAtUtc = DateTime.UtcNow;
                be.UpdatedAtUtc = null;
                be.IsDeleted = false;
            }
            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity;
        }

        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
        {
            if (entity is BaseEntity be)
                be.UpdatedAtUtc = DateTime.UtcNow;

            _repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null) return false;

            _repo.Remove(existing);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
