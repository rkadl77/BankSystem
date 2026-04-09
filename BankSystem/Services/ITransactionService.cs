using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.DTOs;

namespace BankSystem.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsByAccountIdAsync(Guid accountId);
        Task<TransactionDto> DepositAsync(CreateTransactionRequest request);
        Task<TransactionDto> WithdrawAsync(CreateTransactionRequest request);
        Task<bool> TransferAsync(TransferRequest request);
        Task<IEnumerable<TransactionDto>> TransferBetweenAccountsAsync(TransferBetweenAccountsRequest request);
    }
}