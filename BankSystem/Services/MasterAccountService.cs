using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankSystem.Data;
using BankSystem.Models;
using BankSystem.DTOs;
using Microsoft.Extensions.Configuration;

namespace BankSystem.Services
{
    public class MasterAccountService : IMasterAccountService
    {
        private readonly BankSystemContext _context;
        private readonly ITransactionService _transactionService;
        private readonly Guid _masterAccountId;

        public MasterAccountService(BankSystemContext context, IConfiguration config, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
            _masterAccountId = Guid.Parse(config["MasterAccount:Id"]);
        }

        public async Task<Guid> GetMasterAccountIdAsync()
        {
            return _masterAccountId;
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var account = await _context.Accounts.FindAsync(_masterAccountId);
            return account?.Balance ?? 0;
        }

        public async Task<bool> HasSufficientFundsAsync(decimal amount)
        {
            var balance = await GetBalanceAsync();
            return balance >= amount;
        }

        public async Task TransferToClientAsync(Guid clientAccountId, decimal amount)
        {
            var transferRequest = new TransferRequest
            {
                FromAccountId = _masterAccountId,
                ToAccountId = clientAccountId,
                Amount = amount,
                Currency = await GetAccountCurrencyAsync(clientAccountId),
                Description = "Выдача кредита"
            };

            var result = await _transactionService.TransferAsync(transferRequest);
            if (!result)
                throw new InvalidOperationException("Failed to transfer from master account");
        }

        public async Task TransferFromClientAsync(Guid clientAccountId, decimal amount)
        {
            var transferRequest = new TransferRequest
            {
                FromAccountId = clientAccountId,
                ToAccountId = _masterAccountId,
                Amount = amount,
                Currency = await GetAccountCurrencyAsync(clientAccountId),
                Description = "Погашение кредита"
            };

            var result = await _transactionService.TransferAsync(transferRequest);
            if (!result)
                throw new InvalidOperationException("Failed to transfer to master account");
        }

        private async Task<string> GetAccountCurrencyAsync(Guid accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            return account?.Currency ?? "RUB";
        }
    }
}