using System.ComponentModel.DataAnnotations;

namespace ASINToplama_EntityLayer.Concrete
{
    /// <summary> Tenant içindeki uygulama kullanıcısı. </summary>
    public class User : BaseEntity
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public bool IsAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
