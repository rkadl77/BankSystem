using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.Credit.DTOs;

namespace BankSystem.Credit.Services
{
    public interface ICreditTariffService
    {
        Task<IEnumerable<CreditTariffDto>> GetAllTariffsAsync();
        Task<CreditTariffDto> GetTariffByIdAsync(Guid id);
        Task<CreditTariffDto> CreateTariffAsync(CreateCreditTariffRequest request);
        Task<CreditTariffDto> UpdateTariffAsync(Guid id, UpdateCreditTariffRequest request);
        Task<bool> DeleteTariffAsync(Guid id);
        Task<IEnumerable<CreditTariffDto>> GetActiveTariffsAsync();
    }
}