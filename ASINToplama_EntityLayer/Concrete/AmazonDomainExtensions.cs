namespace ASINToplama_EntityLayer.Concrete
{
    public static class AmazonDomainExtensions
    {
        public static string ToBaseUrl(this AmazonDomain domain) =>
            domain switch
            {
                AmazonDomain.Com => "https://www.amazon.com/",
                AmazonDomain.ComMx => "https://www.amazon.com.mx/",
                AmazonDomain.Sa => "https://www.amazon.sa/",
                AmazonDomain.De => "https://www.amazon.de/",
                AmazonDomain.CoUk => "https://www.amazon.co.uk/",
                AmazonDomain.Ca => "https://www.amazon.ca/",
                AmazonDomain.Jp => "https://www.amazon.co.jp/",
                AmazonDomain.In => "https://www.amazon.in/",
                _ => "https://www.amazon.com/"
            };
    }

}
