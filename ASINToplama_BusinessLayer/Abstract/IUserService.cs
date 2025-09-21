using ASINToplama_EntityLayer.Dtos;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default);

        Task<PagedResult<UserDto>> SearchAsync(
            string? keyword,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<UserDto> CreateAsync(UserCreateRequest request, CancellationToken ct = default);
        Task<UserDto?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
