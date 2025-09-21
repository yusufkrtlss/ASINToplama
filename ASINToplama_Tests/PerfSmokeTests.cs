using ASINToplama_BusinessLayer.Concrete;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
namespace ASINToplama_Tests
{
    public class PerfSmokeTests
    {
        [Fact]
        public async Task Quick_Perf_Smoke()
        {
            var http = CreateFastClient();
            var svc = new AmazonSearchService(http, NullLogger<AmazonSearchService>.Instance);

            var keywords = new[] { "mavi", "klavye", "mavi klavye" };
            var sw = Stopwatch.StartNew();
            var dict = await svc.SearchAllAsinsForKeywordsAsync(keywords, AmazonDomain.ComMx, maxConcurrency: 3, ct: default);
            sw.Stop();

            var total = dict.Values.Sum(v => v.Count);
            Console.WriteLine($"Toplam ASIN: {total} | Süre: {sw.Elapsed.TotalSeconds:n1} sn | ASIN/sn: {(total / Math.Max(1, sw.Elapsed.TotalSeconds)):n1}");
        }

        private static HttpClient CreateFastClient()
        {
            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                MaxConnectionsPerServer = 8,
                EnableMultipleHttp2Connections = true
            };

            var http = new HttpClient(handler)
            {
                DefaultRequestVersion = System.Net.HttpVersion.Version20,
                //DefaultVersionPolicy = System.Net.HttpVersionPolicy.RequestVersionOrHigher,
                Timeout = TimeSpan.FromSeconds(20)
            };
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            http.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            return http;
        }
    }
}
