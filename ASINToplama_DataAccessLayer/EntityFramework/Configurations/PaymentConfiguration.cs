using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_DataAccessLayer.EntityFramework.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> b)
        {
            b.ToTable("Payments");

            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired();

            b.Property(x => x.Provider)
             .IsRequired()
             .HasMaxLength(64);

            b.Property(x => x.ProviderPaymentId)
             .HasMaxLength(128);

            b.Property(x => x.Currency)
             .IsRequired()
             .HasConversion<int>(); // enum → int

            b.Property(x => x.AmountMinor).IsRequired();

            b.Property(x => x.Status)
             .IsRequired()
             .HasConversion<int>();

            b.Property(x => x.FailureCode)
             .HasMaxLength(64);

            b.Property(x => x.FailureMessage)
             .HasMaxLength(512);

            b.Property(x => x.Description)
             .HasMaxLength(256);

            // İlişkiler
            b.HasOne<User>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.NoAction); 

            b.HasOne<Subscription>()
             .WithMany()
             .HasForeignKey(x => x.SubscriptionId)
             .OnDelete(DeleteBehavior.SetNull);

            // Soft delete (tablo bazlı)
            b.HasQueryFilter(x => !x.IsDeleted);

            // Sorgu performansı için indeksler
            b.HasIndex(x => new { x.UserId, x.Status });
            b.HasIndex(x => x.PaidAtUtc);
        }
    }
}
