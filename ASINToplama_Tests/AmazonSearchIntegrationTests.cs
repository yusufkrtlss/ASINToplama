using ASINToplama_BusinessLayer.Concrete;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.Extensions.Logging.Abstractions;

namespace ASINToplama_Tests
{
    public class AmazonSearchIntegrationTests
    {
        //[Fact(Skip = "Gerçek Amazon isteği atar, manuel çalıştırılmalı.")]
        [Fact]
        public async Task SearchAllAsins_Should_Return_Many_Asins_From_AmazonMx()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            var svc = new AmazonSearchService(http, Microsoft.Extensions.Logging.Abstractions.NullLogger<AmazonSearchService>.Instance);

            var list = await svc.SearchAllAsinsAsync("keyboard", AmazonDomain.ComMx, default);

            Assert.NotEmpty(list);
            Assert.True(list.Count >= 100, $"Beklenen >=100, gelen {list.Count}");
            Console.WriteLine($"Toplam ASIN: {list.Count}");
            foreach (var asin in list)
            {
                Console.WriteLine(asin);
            }
        }

    }
}
