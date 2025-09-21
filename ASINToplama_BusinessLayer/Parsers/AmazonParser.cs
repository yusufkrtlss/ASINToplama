using HtmlAgilityPack;

namespace ASINToplama_BusinessLayer.Parsers
{
    public static class AmazonParser
    {
        public static IReadOnlyList<string> ExtractAsins(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes(
                "//div[@data-component-type='s-search-result' and @data-asin and string-length(@data-asin)>0]"
            );

            if (nodes is null) return Array.Empty<string>();

            return nodes
                .Select(n => n.GetAttributeValue("data-asin", "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public static bool HasNextPage(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // "Sonraki" butonu var VE disabled değilse → true
            var next = doc.DocumentNode.SelectSingleNode(
                "//a[contains(@class,'s-pagination-next') and not(@aria-disabled='true')]"
            );
            return next != null;
        }
    }
}
