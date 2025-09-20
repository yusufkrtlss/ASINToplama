using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> LoginAsync(
            string email,
            string password,
            CancellationToken ct = default);
    }
}
