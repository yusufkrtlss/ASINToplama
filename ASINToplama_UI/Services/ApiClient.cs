using ASINToplama_UI.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace ASINToplama_UI.Services
{
    public sealed class ApiOptions
    {
        public string BaseUrl { get; set; } = "";
    }

    public interface IApiClient
    {
        string? AccessToken { get; }
        Task<LoginResult?> LoginAsync(string email, string password, CancellationToken ct = default);
        Task<BulkResponse?> RunBulkAsync(BulkRequest req, CancellationToken ct = default);

        // Opsiyonel yardımcılar:
        Task RestoreTokenAsync(string? token);
        string? GetToken();
    }

    public sealed class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json;
        private string? _accessToken;

        public string? AccessToken => _accessToken;

        public ApiClient(HttpClient http, IOptions<ApiOptions> opt)
        {
            _http = http;
            _http.BaseAddress = new Uri(opt.Value.BaseUrl.TrimEnd('/'));

            // İstek/yanıt JSON’unu camelCase bekliyoruz (API de camelCase döndürüyor)
            _json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public async Task<LoginResult?> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            var payload = new LoginDto { Email = email, Password = password };
            var body = JsonSerializer.Serialize(payload, _json);
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync("/api/auth/login", content, ct);
            if (!resp.IsSuccessStatusCode) return null;

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<LoginResult>(stream, _json, ct);
            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken)) return null;

            _accessToken = result.AccessToken;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return result;
        }

        public async Task<BulkResponse?> RunBulkAsync(BulkRequest req, CancellationToken ct = default)
        {
            // domain int olarak serileşir; enum sıralaması API ile aynı
            var body = JsonSerializer.Serialize(req, _json);
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync("/api/amazon/bulk", content, ct);
            if (!resp.IsSuccessStatusCode) return null;

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<BulkResponse>(stream, _json, ct);
            return result;
        }

        // ---- Opsiyoneller: oturum kalıcılığı için yardımcılar ----
        public Task RestoreTokenAsync(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                _accessToken = token;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            return Task.CompletedTask;
        }

        public string? GetToken() => _accessToken;
    }
}
