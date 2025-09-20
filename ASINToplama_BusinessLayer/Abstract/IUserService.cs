using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface IUserService : IGenericService<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    }
}
