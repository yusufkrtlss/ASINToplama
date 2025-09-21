using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_BusinessLayer.Parsers;
using ASINToplama_EntityLayer.Concrete;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class AmazonSearchService : IAmazonSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AmazonSearchService> _logger;

        // Sonsuz döngü emniyeti (gerekirse appsettings'e taşınır)
        private const int MaxPagesSafety = 50;

        // Adaptif bekleme (anti-bot'a takılmadan hızlanmak için)
        private volatile int _adaptiveDelayMs = 400; // başlangıç
        private const int MinDelayMs = 200;
        private const int MaxDelayMs = 1500;

        // Çok kelimelik çalışmada ani patlamayı yumuşat (global QPS sınırlayıcı)
        private readonly SemaphoreSlim _globalQps = new(3, 3); // aynı anda 3 GET

        public AmazonSearchService(HttpClient httpClient, ILogger<AmazonSearchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                //    "User-Agent",
                //    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                //);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            }
            //if (!_httpClient.DefaultRequestHeaders.AcceptEncoding.Any())
            //{
            //    _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            //}

            _httpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        public async Task<IReadOnlyList<string>> SearchAsinsAsync(
            string keyword, AmazonDomain domain, int pageNumber, int pageSize, CancellationToken ct)
        {
            ApplyAcceptLanguage(domain);

            var baseUrl = domain.ToBaseUrl();
            var url = $"{baseUrl}s?k={Uri.EscapeDataString(keyword)}&page={pageNumber}";

            _logger.LogInformation("Search page Domain={Domain} Keyword={Keyword} Page={Page}", domain, keyword, pageNumber);

            await _globalQps.WaitAsync(ct);
            try
            {
                var resp = await _httpClient.GetAsync(url, ct);

                if ((int)resp.StatusCode == 503 || (int)resp.StatusCode == 429)
                {
                    AdaptDelay(false);
                    await Task.Delay(_adaptiveDelayMs, ct);
                    resp = await _httpClient.GetAsync(url, ct);
                }
                resp.EnsureSuccessStatusCode();

                var html = await resp.Content.ReadAsStringAsync(ct);
                var asins = AmazonParserFast.ExtractAsinsFast(html); // hızlı regex parser
                return asins.Take(pageSize).ToList();
            }
            finally
            {
                _globalQps.Release();
            }
        }

        public async Task<IReadOnlyList<string>> SearchAllAsinsAsync(
            string keyword, AmazonDomain domain, CancellationToken ct)
        {
            ApplyAcceptLanguage(domain);

            var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var page = 1;

            // Prefetch: bir sonraki sayfayı önceden başlat (temkinli)
            Task<HttpResponseMessage>? nextTask = null;

            while (page <= MaxPagesSafety && !ct.IsCancellationRequested)
            {
                var baseUrl = domain.ToBaseUrl();
                var url = $"{baseUrl}s?k={Uri.EscapeDataString(keyword)}&page={page}";

                _logger.LogInformation("Search all Domain={Domain} Keyword={Keyword} Page={Page}", domain, keyword, page);

                HttpResponseMessage resp;

                await _globalQps.WaitAsync(ct);
                try
                {
                    // Şu anki sayfa isteği
                    var currentTask = nextTask ?? _httpClient.GetAsync(url, ct);

                    // Bir sonrakini (page+1) önden başlat
                    var nextUrl = $"{baseUrl}s?k={Uri.EscapeDataString(keyword)}&page={page + 1}";
                    nextTask = _httpClient.GetAsync(nextUrl, ct);

                    resp = await currentTask;

                    if ((int)resp.StatusCode == 503 || (int)resp.StatusCode == 429)
                    {
                        AdaptDelay(false);
                        await Task.Delay(_adaptiveDelayMs, ct);
                        resp = await _httpClient.GetAsync(url, ct);
                    }
                }
                finally
                {
                    _globalQps.Release();
                }

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Page {Page} failed with {Status}. Stopping.", page, resp.StatusCode);
                    break;
                }

                var html = await resp.Content.ReadAsStringAsync(ct);
                var pageAsins = AmazonParserFast.ExtractAsinsFast(html);
                if (pageAsins.Count == 0)
                {
                    _logger.LogInformation("No ASINs on page {Page}. Stopping.", page);
                    break;
                }

                var before = all.Count;
                foreach (var a in pageAsins) all.Add(a);
                var added = all.Count - before;
                if (added == 0)
                {
                    _logger.LogInformation("No new ASINs on page {Page}. Stopping.", page);
                    break;
                }

                // Sonraki sayfa var mı? (basit kontrol: link analizi yerine "prefetch cevap + added" kombinasyonu)
                // Eğer prefetch yaptığımız sonraki sayfa başarısız görünürse ya da added çok küçükse adaptif bekleme
                AdaptDelay(true);
                await Task.Delay(Math.Max(100, _adaptiveDelayMs / 2), ct);

                page++;
            }

            _logger.LogInformation("Search all finished. Total={Count}", all.Count);
            return all.ToList();
        }

        public async Task<Dictionary<string, IReadOnlyList<string>>> SearchAllAsinsForKeywordsAsync(
            IEnumerable<string> keywords, AmazonDomain domain, int maxConcurrency, CancellationToken ct)
        {
            ApplyAcceptLanguage(domain);

            var degree = Math.Clamp(maxConcurrency, 1, 4); // güvenli 1..4
            var sem = new SemaphoreSlim(degree, degree);
            var bag = new ConcurrentDictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            var tasks = new List<Task>();

            foreach (var kw in keywords)
            {
                await sem.WaitAsync(ct);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var list = await SearchAllAsinsAsync(kw, domain, ct);
                        bag[kw] = list;
                        await Task.Delay(300 + Random.Shared.Next(0, 250), ct); // kelimeler arası minik gecikme
                    }
                    catch
                    {
                        bag[kw] = Array.Empty<string>();
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks);
            return new Dictionary<string, IReadOnlyList<string>>(bag, StringComparer.OrdinalIgnoreCase);
        }

        private void ApplyAcceptLanguage(AmazonDomain domain)
        {
            _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept-Language",
                domain switch
                {
                    AmazonDomain.ComMx => "es-MX,es;q=0.9,en;q=0.8",
                    AmazonDomain.Sa => "ar-SA,ar;q=0.9,en;q=0.8",
                    AmazonDomain.De => "de-DE,de;q=0.9,en;q=0.8",
                    AmazonDomain.CoUk => "en-GB,en;q=0.9",
                    AmazonDomain.Ca => "en-CA,en;q=0.9",
                    AmazonDomain.Jp => "ja-JP,ja;q=0.9,en;q=0.8",
                    AmazonDomain.In => "en-IN,en;q=0.9",
                    _ => "en-US,en;q=0.9"
                });
        }

        private void AdaptDelay(bool success)
        {
            if (success)
            {
                var v = _adaptiveDelayMs - 50;
                _adaptiveDelayMs = v < MinDelayMs ? MinDelayMs : v;
            }
            else
            {
                var v = _adaptiveDelayMs + 250;
                _adaptiveDelayMs = v > MaxDelayMs ? MaxDelayMs : v;
            }
        }
    }
}
