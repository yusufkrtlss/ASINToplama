using System.ComponentModel.DataAnnotations;

namespace ASINToplama_EntityLayer.Concrete
{
    /// <summary> Bir abonelik/tenant için gerçekleşen ödeme özet kaydı. </summary>
    public class Payment : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? SubscriptionId { get; set; }

        public string Provider { get; set; } = "Manual";
        public string ProviderPaymentId { get; set; } = string.Empty;

        public Currency Currency { get; set; } = Currency.TRY;
        public long AmountMinor { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime? PaidAtUtc { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }

        public bool IsRefunded { get; set; }
        public DateTime? RefundedAtUtc { get; set; }

        public string? Description { get; set; }
        public string? MetadataJson { get; set; }
    }
}
