using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_DataAccessLayer.Helpers;
using ASINToplama_EntityLayer.Concrete;
using ASINToplama_EntityLayer.Dtos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userDal;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userDal, IUnitOfWork uow, IMapper mapper)
        {
            _userDal = userDal;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _userDal.GetByIdAsync(id, ct);
            return entity is null ? null : _mapper.Map<UserDto>(entity);
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var entity = await _userDal.Query().FirstOrDefaultAsync(u => u.Email == email, ct);
            return entity is null ? null : _mapper.Map<UserDto>(entity);
        }

        public async Task<PagedResult<UserDto>> SearchAsync(
    string? keyword, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var q = _userDal.Query(); // AsNoTracking default

            // Soft-delete/aktiflik filtreleri (entity’nizde bu alanlar varsa açın)
            if (typeof(User).GetProperty(nameof(User.IsDeleted)) != null)
                q = q.Where(u => !u.IsDeleted);
            if (typeof(User).GetProperty(nameof(User.IsActive)) != null)
                q = q.Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Case-insensitive collation (SQL Server)
                var pattern = $"%{keyword.Trim()}%";
                q = q.Where(u =>
                    (u.Email != null && EF.Functions.Like(
                        EF.Functions.Collate(u.Email, "SQL_Latin1_General_CP1_CI_AS"), pattern)) ||
                    (u.FullName != null && EF.Functions.Like(
                        EF.Functions.Collate(u.FullName, "SQL_Latin1_General_CP1_CI_AS"), pattern))
                );
            }

            var total = await q.CountAsync(ct);

            var list = await q
                .OrderByDescending(u => u.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<UserDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Items = _mapper.Map<List<UserDto>>(list)
            };
        }



        public async Task<UserDto> CreateAsync(UserCreateRequest request, CancellationToken ct = default)
        {
            // İş kuralı: email benzersiz
            if (await _userDal.AnyAsync(u => u.Email == request.Email, ct))
                throw new InvalidOperationException("Email zaten kayıtlı.");

            var entity = _mapper.Map<User>(request);
            entity.CreatedAtUtc = DateTime.UtcNow;
            entity.IsActive = true;

            // Şifre hash
            if (!string.IsNullOrWhiteSpace(entity.PasswordHash))
                entity.PasswordHash = PasswordEncryption.Hash(entity.PasswordHash);

            await _userDal.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<UserDto>(entity);
        }

        public async Task<UserDto?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken ct = default)
        {
            var entity = await _userDal.GetByIdAsync(id, ct);
            if (entity is null) return null;

            _mapper.Map(request, entity); // Password -> PasswordHash aktarımı

            if (!string.IsNullOrWhiteSpace(entity.PasswordHash))
                entity.PasswordHash = PasswordEncryption.Hash(entity.PasswordHash);

            // BaseEntity var ise UpdatedAtUtc repo.Remove’da/set’te ayarlanıyordu; burada da set edebiliriz:
            if (entity is BaseEntity be) be.UpdatedAtUtc = DateTime.UtcNow;

            _userDal.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<UserDto>(entity);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _userDal.GetByIdAsync(id, ct);
            if (entity is null) return false;

            _userDal.Remove(entity); // Soft delete ise GenericRepository içinde işaretleniyor
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
