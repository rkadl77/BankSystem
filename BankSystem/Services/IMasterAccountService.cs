using System;
using System.Threading.Tasks;

namespace BankSystem.Services
{
    public interface IMasterAccountService
    {
        Task<Guid> GetMasterAccountIdAsync();
        Task<decimal> GetBalanceAsync();
        Task<bool> HasSufficientFundsAsync(decimal amount);
        Task TransferToClientAsync(Guid clientAccountId, decimal amount);
        Task TransferFromClientAsync(Guid clientAccountId, decimal amount);
    }
}