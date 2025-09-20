using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;

        public AuthService(IUserRepository users)
        {
            _users = users;
        }

        public Task<(bool Success, string Message, User? User)> LoginAsync(
            string email,
            string password,
            CancellationToken ct = default)
            => _users.LoginAsync(email, password);
    }
}
