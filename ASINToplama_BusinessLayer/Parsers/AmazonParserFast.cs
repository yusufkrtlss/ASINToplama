using System.Text.RegularExpressions;

namespace ASINToplama_BusinessLayer.Parsers
{
    public static class AmazonParserFast
    {
        // ASIN çoğunlukla 10 karakter: A-Z + 0-9
        private static readonly Regex AsinRegex =
            new Regex("data-asin=\"([A-Z0-9]{10})\"", RegexOptions.Compiled);

        public static IReadOnlyList<string> ExtractAsinsFast(string html)
        {
            if (string.IsNullOrEmpty(html)) return Array.Empty<string>();

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in AsinRegex.Matches(html))
            {
                var val = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(val))
                    set.Add(val);
            }
            return set.ToList();
        }
    }
}
