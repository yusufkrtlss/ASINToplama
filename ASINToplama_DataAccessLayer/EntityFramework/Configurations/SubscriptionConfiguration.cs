using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_DataAccessLayer.EntityFramework.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> b)
        {
            b.ToTable("Subscriptions");

            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired();

            b.Property(x => x.PlanName)
             .IsRequired()
             .HasMaxLength(64);

            b.Property(x => x.DailyLimit).IsRequired();

            b.Property(x => x.CurrentPeriodStartUtc).IsRequired();
            b.Property(x => x.CurrentPeriodEndUtc).IsRequired();

            b.Property(x => x.Status)
             .IsRequired()
             .HasConversion<int>(); // enum → int

            b.Property(x => x.BillingAnchorHour).IsRequired();

            b.Property(x => x.CancelAtPeriodEnd).IsRequired();

            // Opsiyonel Trial sonu
            b.Property(x => x.TrialEndUtc);

            // İlişkiler
            b.HasOne<User>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.NoAction); // ⬅️ Cascade kapat


            // Soft delete (tablo bazlı)
            b.HasQueryFilter(x => !x.IsDeleted);

            // Sorgu performansı için indeksler
            b.HasIndex(x => new { x.UserId, x.Status });
            b.HasIndex(x => new { x.UserId, x.CurrentPeriodStartUtc, x.CurrentPeriodEndUtc });
        }
    }
}
