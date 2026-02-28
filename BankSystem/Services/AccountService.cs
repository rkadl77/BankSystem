using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankSystem.Data;
using BankSystem.Models;
using BankSystem.DTOs;

namespace BankSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankSystemContext _context;

        public AccountService(BankSystemContext context)
        {
            _context = context;
        }

        public async Task<AccountDto> GetAccountByIdAsync(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return null;

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                IsActive = account.IsActive
            };
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsByClientIdAsync(Guid clientId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.ClientId == clientId)
                .ToListAsync();

            return accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
            });
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
        {
            if (string.IsNullOrEmpty(request.Currency))
                throw new InvalidOperationException("Currency is required");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = GenerateAccountNumber(),
                ClientId = request.ClientId,
                Balance = 0,
                Currency = request.Currency.ToUpper(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Currency = account.Currency,
                CreatedAt = account.CreatedAt,
                IsActive = account.IsActive
            };
        }

        public async Task<bool> CloseAccountAsync(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null || !account.IsActive || account.Balance > 0)
                return false;

            account.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetBalanceAsync(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);
            return account?.Balance ?? 0;
        }

        private string GenerateAccountNumber()
        {
            return $"ACC{DateTime.UtcNow.Ticks.ToString().Substring(0, 12)}";
        }
    }
}