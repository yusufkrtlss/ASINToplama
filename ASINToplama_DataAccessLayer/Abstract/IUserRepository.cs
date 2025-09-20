using ASINToplama_EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASINToplama_DataAccessLayer.Abstract
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
    }
}
