namespace BankSystem.Services
{
    public interface ICurrencyExchangeService
    {
        Task<decimal> GetExchangeRateAsync(string from, string to);
        Task<decimal> ConvertAsync(decimal amount, string from, string to);
    }
}
