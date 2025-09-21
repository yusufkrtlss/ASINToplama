using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASINToplama_API.Controllers
{
    [ApiController]
    [Route("api/amazon")]
    [AllowAnonymous] // şimdilik açık
    public class AmazonSearchController : ControllerBase
    {
        private readonly IAmazonSearchService _svc;

        public AmazonSearchController(IAmazonSearchService svc) => _svc = svc;

        // ---- Tek sayfa (mevcut servis metodunla zaten vardıysa bırak) ----
        [HttpGet("page")]
        public async Task<IActionResult> Page(
            [FromQuery] string keyword,
            [FromQuery] AmazonDomain domain = AmazonDomain.ComMx,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("keyword zorunludur.");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var asins = await _svc.SearchAsinsAsync(keyword, domain, pageNumber, pageSize, ct);

            return Ok(new
            {
                keyword,
                domain,
                pageNumber,
                pageSize,
                count = asins.Count,
                asins
            });
        }

        // ---- Tüm sayfalar (mevcut servis metodunla) ----
        [HttpGet("all")]
        public async Task<IActionResult> All(
            [FromQuery] string keyword,
            [FromQuery] AmazonDomain domain = AmazonDomain.ComMx,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("keyword zorunludur.");

            var asins = await _svc.SearchAllAsinsAsync(keyword, domain, ct);

            return Ok(new
            {
                keyword,
                domain,
                count = asins.Count,
                asins
            });
        }

        // ---- BULK: İstemci yalnızca keywords + domain gönderir; maxConcurrency = 3 sabit ----
        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk(
            [FromBody] AmazonBulkSimpleRequest req,
            CancellationToken ct)
        {
            if (req?.Keywords is null || req.Keywords.Count == 0)
                return BadRequest("En az 1 keyword giriniz.");
            if (req.Keywords.Count > 500)
                return BadRequest("Maksimum 500 keyword desteklenir.");

            // input hijyeni (boş/aynı anahtar kelimeleri temizle)
            var cleanKeywords = req.Keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleanKeywords.Count == 0)
                return BadRequest("Geçerli keyword bulunamadı.");

            const int maxConcurrency = 3; // sabit

            var t0 = DateTime.UtcNow;
            var map = await _svc.SearchAllAsinsForKeywordsAsync(cleanKeywords, req.Domain, maxConcurrency, ct);
            var elapsed = DateTime.UtcNow - t0;

            var results = map.Select(kvp => new
            {
                keyword = kvp.Key,
                count = kvp.Value.Count,
                asins = kvp.Value
            }).ToList();

            var total = results.Sum(r => r.count);
            var aps = elapsed.TotalSeconds > 0 ? Math.Round(total / elapsed.TotalSeconds, 1) : total;

            return Ok(new
            {
                domain = req.Domain,
                totalAsinCount = total,
                elapsed,
                asinPerSecond = aps,
                results
            });
        }
    }

    // İstemci sadece keywords + domain gönderir
    public sealed class AmazonBulkSimpleRequest
    {
        public List<string> Keywords { get; set; } = new();
        public AmazonDomain Domain { get; set; } = AmazonDomain.Com;
    }

}
