using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.Credit.DTOs;

namespace BankSystem.Credit.Services
{
    public interface ICreditService
    {
        Task<IEnumerable<CreditDto>> GetCreditsByClientIdAsync(Guid clientId);
        Task<CreditDetailsDto?> GetCreditByIdAsync(Guid id);
        Task<CreditDto> CreateCreditAsync(CreateCreditRequest request);
        Task<bool> RepayCreditAsync(RepayCreditRequest request);
        Task<IEnumerable<CreditDto>> GetActiveCreditsAsync();
        Task<decimal> GetTotalDebtByClientIdAsync(Guid clientId);
        Task<IEnumerable<CreditDto>> GetOverdueCreditsAsync(Guid clientId);
        Task<CreditRatingDto> CalculateCreditRatingAsync(Guid clientId);
        Task<bool> UpdateCreditStatusAsync(Guid creditId);
        Task<CreditDetailDto?> GetCreditDetailsAsync(Guid creditId);
    }
}