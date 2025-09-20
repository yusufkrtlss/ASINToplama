using ASINToplama_BusinessLayer.Models;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface ILicenseGuard
    {
        Task<LicenseSnapshot> EnsureActiveLicenseAsync(Guid userId, DateTime nowUtc, CancellationToken ct = default);
    }
}
