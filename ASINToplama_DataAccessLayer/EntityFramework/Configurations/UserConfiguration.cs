using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_DataAccessLayer.EntityFramework.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users");

            b.HasKey(x => x.Id);

            b.Property(x => x.Email)
             .IsRequired()
             .HasMaxLength(256);

            b.Property(x => x.FullName)
             .IsRequired()
             .HasMaxLength(128);

            b.Property(x => x.PasswordHash)
             .IsRequired()
             .HasMaxLength(512);

            b.Property(x => x.IsAdmin).IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            // Soft delete (tablo bazlı)
            b.HasQueryFilter(x => !x.IsDeleted);

            // Sık kullanılan alanlar için indeks
            b.HasIndex(x => x.Email).IsUnique(); // sistem genelinde e-posta tekilse
            b.HasIndex(x => x.IsActive);
        }
    }
}
