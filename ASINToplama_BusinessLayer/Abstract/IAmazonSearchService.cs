using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Abstract
{
    public interface IAmazonSearchService
    {
        Task<IReadOnlyList<string>> SearchAsinsAsync(
            string keyword,
            AmazonDomain domain,
            int pageNumber,
            int pageSize,
            CancellationToken ct);

        // Maksimum vermeden "tüm sayfaları tara".
        Task<IReadOnlyList<string>> SearchAllAsinsAsync(
            string keyword,
            AmazonDomain domain,
            CancellationToken ct);

        // çoklu kelime (her biri bağımsız, eşzamanlılık kontrollü)
        Task<Dictionary<string, IReadOnlyList<string>>> SearchAllAsinsForKeywordsAsync(
            IEnumerable<string> keywords, AmazonDomain domain, int maxConcurrency, CancellationToken ct);
    }
}
