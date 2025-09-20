using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface ISubscriptionService : IGenericService<Subscription>
    {
        Task<Subscription?> GetActiveAsync(Guid userId, DateTime nowUtc, CancellationToken ct = default);
        Task<bool> SetCancelAtPeriodEndAsync(Guid subscriptionId, bool value, CancellationToken ct = default);
    }
}
