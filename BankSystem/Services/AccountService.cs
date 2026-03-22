using BankSystem.Clients;
using BankSystem.Data;
using BankSystem.DTOs;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankSystemContext _context;
        private readonly IUserServiceClient _userClient;

        public AccountService(BankSystemContext context, IUserServiceClient userClient)
        {
            _context = context;
            _userClient = userClient;
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
                Currency = account.Currency,
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
                Currency = a.Currency,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
            });
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
        {
            if (string.IsNullOrEmpty(request.Currency))
                throw new InvalidOperationException("Currency is required");

            var userExists = await _userClient.UserExistsAsync(request.ClientId);
            if (!userExists)
                throw new InvalidOperationException($"Client with id {request.ClientId} does not exist");

            var masterExists = await _context.Accounts.AnyAsync(a => a.IsMasterAccount);
            if (masterExists)
                throw new InvalidOperationException("Master account already exists");

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

            if (account.IsMasterAccount)
                throw new InvalidOperationException("Cannot close the master account");

            account.IsActive = false;
            account.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetBalanceAsync(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);
            return account?.Balance ?? 0;
        }

        public async Task<Account> GetMasterAccountAsync()
        {
            var masterAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.IsMasterAccount);

            if (masterAccount == null)
                throw new InvalidOperationException("Master account not found");

            return masterAccount;
        }

        private string GenerateAccountNumber()
        {
            return $"ACC{DateTime.UtcNow.Ticks.ToString().Substring(0, 12)}";
        }
    }
}