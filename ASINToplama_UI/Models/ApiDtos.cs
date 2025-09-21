namespace ASINToplama_UI.Models
{
    public enum AmazonDomain
    {
        Com = 0,
        ComMx = 1,
        Sa = 2,
        De = 3,
        CoUk = 4,
        Ca = 5,
        Jp = 6,
        In = 7
    }

    public sealed class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public sealed class LoginResult
    {
        public string AccessToken { get; set; } = "";
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserInfo User { get; set; } = new();
    }

    public sealed class UserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public bool IsAdmin { get; set; }
    }

    // BULK istek/yanıt (API sözleşmesine göre)
    public sealed class BulkRequest
    {
        public List<string> Keywords { get; set; } = new();
        public AmazonDomain Domain { get; set; } = AmazonDomain.ComMx; // 1 = Meksika
    }

    public sealed class BulkKeywordResult
    {
        public string Keyword { get; set; } = "";
        public int Count { get; set; }
        public List<string> Asins { get; set; } = new();
    }

    public sealed class BulkResponse
    {
        public int Domain { get; set; } // API integer döndürüyor
        public int TotalAsinCount { get; set; }
        public string Elapsed { get; set; } = "";
        public double AsinPerSecond { get; set; }
        public List<BulkKeywordResult> Results { get; set; } = new();
    }
}
