using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_DataAccessLayer.Helpers;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class UserService : GenericService<User>, IUserService
    {
        private readonly IGenericRepository<User> _repo;

        public UserService(IGenericRepository<User> repo, IUnitOfWork uow) : base(repo, uow)
        {
            _repo = repo;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _repo.Query().FirstOrDefaultAsync(u => u.Email == email, ct);

        // Parola hash’leme ile Create
        public override async Task<User> CreateAsync(User entity, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(entity.PasswordHash))
                entity.PasswordHash = PasswordEncryption.Hash(entity.PasswordHash);

            return await base.CreateAsync(entity, ct);
        }

        // Parola güncellendiyse hash’le
        public override async Task<bool> UpdateAsync(User entity, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(entity.PasswordHash))
            {
                // Dışarıdan plain geldiyse hash’le; aynı hash tekrar gelmişse istersen kontrol ekleyebilirsin
                entity.PasswordHash = PasswordEncryption.Hash(entity.PasswordHash);
            }
            return await base.UpdateAsync(entity, ct);
        }
    }
}
