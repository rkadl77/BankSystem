using System.Text;
using System.Text.Json;
using BankSystem.Credit.DTOs;

namespace BankSystem.Credit.Clients
{
    public class CoreServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoreServiceClient> _logger;

        public CoreServiceClient(HttpClient httpClient, ILogger<CoreServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("http://localhost:5001");
        }

        public async Task<AccountDto?> GetAccountAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AccountDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account {AccountId}", accountId);
                return null;
            }
        }

        public async Task<decimal?> GetAccountBalanceAsync(Guid accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/{accountId}/balance");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<decimal>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for account {AccountId}", accountId);
                return null;
            }
        }

        public async Task<string?> GetAccountCurrencyAsync(Guid accountId)
        {
            var account = await GetAccountAsync(accountId);
            return account?.Currency;
        }

        public async Task<bool> WithdrawFromAccountAsync(Guid accountId, decimal amount, string currency, string description)
        {
            try
            {
                var request = new
                {
                    AccountId = accountId,
                    Amount = amount,
                    Currency = currency,
                    Type = "credit_payment",
                    Description = description
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/transactions/withdraw", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Withdraw failed for account {AccountId}: {Error}", accountId, error);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing from account {AccountId}", accountId);
                return false;
            }
        }
    }
}