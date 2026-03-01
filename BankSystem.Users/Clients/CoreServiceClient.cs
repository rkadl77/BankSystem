using System.Text.Json;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Clients
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

        public async Task<List<AccountDto>?> GetAccountsByClientIdAsync(Guid clientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/accounts/client/{clientId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<AccountDto>>();
                }
                return new List<AccountDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts for client {ClientId}", clientId);
                return new List<AccountDto>();
            }
        }
    }
}