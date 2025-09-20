using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class SubscriptionService : GenericService<Subscription>, ISubscriptionService
    {
        private readonly IGenericRepository<Subscription> _subs;
        private readonly IUnitOfWork _uow;

        public SubscriptionService(IGenericRepository<Subscription> subs, IUnitOfWork uow) : base(subs, uow)
        {
            _subs = subs;
            _uow = uow;
        }

        public Task<Subscription?> GetActiveAsync(Guid userId, DateTime nowUtc, CancellationToken ct = default)
            => _subs.Query()
                    .Where(s => s.UserId == userId
                                && s.Status == SubscriptionStatus.Active
                                && s.CurrentPeriodStartUtc <= nowUtc
                                && nowUtc < s.CurrentPeriodEndUtc)
                    .FirstOrDefaultAsync(ct);

        public async Task<bool> SetCancelAtPeriodEndAsync(Guid subscriptionId, bool value, CancellationToken ct = default)
        {
            var s = await _subs.GetByIdAsync(subscriptionId, ct);
            if (s is null) return false;
            s.CancelAtPeriodEnd = value;
            _subs.Update(s);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
