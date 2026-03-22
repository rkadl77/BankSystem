using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace BankSystem.Services
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyExchangeService> _logger;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

        public CurrencyExchangeService(HttpClient httpClient, IMemoryCache cache, ILogger<CurrencyExchangeService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRateAsync(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Currency codes cannot be empty");

            from = from.ToUpper();
            to = to.ToUpper();

            if (from == to)
                return 1m;

            var cacheKey = $"exchange_rate_{from}_{to}";

            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation("Cache hit for exchange rate {From} to {To}", from, to);
                return cachedRate;
            }

            _logger.LogInformation("Cache miss for exchange rate {From} to {To}, calling API", from, to);

            try
            {
                var response = await _httpClient.GetAsync($"https://api.exchangerate-api.com/v4/latest/{from}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(content, options);

                if (data?.Rates == null || !data.Rates.ContainsKey(to))
                    throw new InvalidOperationException($"Exchange rate for {to} not found in API response");

                var rate = data.Rates[to];

                _cache.Set(cacheKey, rate, CacheTtl);
                _logger.LogInformation("Cached exchange rate {From} to {To}: {Rate}", from, to, rate);

                return rate;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to get exchange rate from {From} to {To}", from, to);
                throw new InvalidOperationException($"Failed to get exchange rate from {from} to {to}", ex);
            }
        }

        public async Task<decimal> ConvertAsync(decimal amount, string from, string to)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");

            var rate = await GetExchangeRateAsync(from, to);
            return amount * rate;
        }
    }

    public class ExchangeRateResponse
    {
        public string Base { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
