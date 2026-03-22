using System.Text.Json;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Clients
{
    public class CreditServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CreditServiceClient> _logger;

        public CreditServiceClient(HttpClient httpClient, ILogger<CreditServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("http://localhost:5003");
        }

        public async Task<List<CreditDto>?> GetCreditsByClientIdAsync(Guid clientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/credits/client/{clientId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CreditDto>>();
                }
                return new List<CreditDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credits for client {ClientId}", clientId);
                return new List<CreditDto>();
            }
        }
    }
}