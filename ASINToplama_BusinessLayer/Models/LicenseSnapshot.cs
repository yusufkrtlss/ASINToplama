namespace ASINToplama_BusinessLayer.Models
{
    public sealed class LicenseSnapshot
    {
        public Guid UserId { get; init; }
        public string PlanName { get; init; } = "Default";
        public int DailyLimit { get; init; } = 500_000;
        public DateTime DayAnchorUtc { get; init; }
        public DateTime ResetAtUtc { get; init; }
    }
}
