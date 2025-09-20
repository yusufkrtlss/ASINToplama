using System.ComponentModel.DataAnnotations;

namespace ASINToplama_EntityLayer.Concrete
{
    /// <summary> Aylık lisans/abonelik kaydı. Kota hesapları bu kayda bağlıdır. </summary>
    public class Subscription : BaseEntity
    {
        public Guid UserId { get; set; }

        public string PlanName { get; set; } = "Default";
        public int DailyLimit { get; set; } = 500_000;

        public DateTime CurrentPeriodStartUtc { get; set; }
        public DateTime CurrentPeriodEndUtc { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
        public int BillingAnchorHour { get; set; } = 2;
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? TrialEndUtc { get; set; }
    }
}
