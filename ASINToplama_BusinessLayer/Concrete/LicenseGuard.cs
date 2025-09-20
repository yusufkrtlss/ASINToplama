using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_BusinessLayer.Models;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class LicenseGuard : ILicenseGuard
    {
        private readonly IGenericRepository<User> _users;
        private readonly ISubscriptionService _subs;
        private static readonly TimeZoneInfo IstanbulTz =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

        public LicenseGuard(IGenericRepository<User> users, ISubscriptionService subs)
        {
            _users = users;
            _subs = subs;
        }

        public async Task<LicenseSnapshot> EnsureActiveLicenseAsync(Guid userId, DateTime nowUtc, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId, ct)
                       ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
            if (!user.IsActive) throw new UnauthorizedAccessException("Kullanıcı pasif.");

            var active = await _subs.GetActiveAsync(userId, nowUtc, ct)
                        ?? throw new InvalidOperationException("Aktif abonelik bulunamadı.");

            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, IstanbulTz);
            var anchorIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, 2, 0, 0, nowIst.Kind);
            if (nowIst < anchorIst) anchorIst = anchorIst.AddDays(-1);
            var resetIst = anchorIst.AddDays(1);

            var anchorUtc = TimeZoneInfo.ConvertTimeToUtc(anchorIst, IstanbulTz);
            var resetUtc = TimeZoneInfo.ConvertTimeToUtc(resetIst, IstanbulTz);

            return new LicenseSnapshot
            {
                UserId = user.Id,
                PlanName = active.PlanName,
                DailyLimit = active.DailyLimit,
                DayAnchorUtc = anchorUtc,
                ResetAtUtc = resetUtc
            };
        }
    }
}
