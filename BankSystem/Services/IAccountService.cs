using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.DTOs;

namespace BankSystem.Services
{
    public interface IAccountService
    {
        Task<AccountDto> GetAccountByIdAsync(Guid id);
        Task<IEnumerable<AccountDto>> GetAccountsByClientIdAsync(Guid clientId);
        Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
        Task<bool> CloseAccountAsync(Guid id);
        Task<decimal> GetBalanceAsync(Guid id);
    }
}