using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_DataAccessLayer.EntityFramework.Context;
using ASINToplama_DataAccessLayer.Helpers;
using ASINToplama_DataAccessLayer.Repository;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASINToplama_DataAccessLayer.EntityFramework.Concrete
{
    public class EFUserRepository : GenericRepository<User>, IUserRepository
    {
        public EFUserRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
                return (false, "Kullanıcı bulunamadı.", null);

            if (!user.IsActive)
                return (false, "Kullanıcı pasif.", null);

            // Şifre doğrulama: entity’de alan adı PasswordHash
            var ok = PasswordEncryption.CheckHashed(user.PasswordHash, password);
            if (!ok)
                return (false, "Geçersiz şifre.", null);

            return (true, "Giriş başarılı.", user);
        }
    }
}
