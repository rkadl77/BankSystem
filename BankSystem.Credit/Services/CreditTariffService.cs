using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankSystem.Credit.Data;
using BankSystem.Credit.Models;
using BankSystem.Credit.DTOs;

namespace BankSystem.Credit.Services
{
    public class CreditTariffService : ICreditTariffService
    {
        private readonly CreditDbContext _context;

        public CreditTariffService(CreditDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CreditTariffDto>> GetAllTariffsAsync()
        {
            var tariffs = await _context.CreditTariffs.ToListAsync();
            return tariffs.Select(t => new CreditTariffDto
            {
                Id = t.Id,
                Name = t.Name,
                InterestRate = t.InterestRate,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                IsActive = t.IsActive
            });
        }

        public async Task<CreditTariffDto> GetTariffByIdAsync(Guid id)
        {
            var tariff = await _context.CreditTariffs.FindAsync(id);
            if (tariff == null) return null;

            return new CreditTariffDto
            {
                Id = tariff.Id,
                Name = tariff.Name,
                InterestRate = tariff.InterestRate,
                Description = tariff.Description,
                CreatedAt = tariff.CreatedAt,
                IsActive = tariff.IsActive
            };
        }

        public async Task<CreditTariffDto> CreateTariffAsync(CreateCreditTariffRequest request)
        {
            var tariff = new CreditTariff
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                InterestRate = request.InterestRate,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.CreditTariffs.Add(tariff);
            await _context.SaveChangesAsync();

            return new CreditTariffDto
            {
                Id = tariff.Id,
                Name = tariff.Name,
                InterestRate = tariff.InterestRate,
                Description = tariff.Description,
                CreatedAt = tariff.CreatedAt,
                IsActive = tariff.IsActive
            };
        }

        public async Task<CreditTariffDto> UpdateTariffAsync(Guid id, UpdateCreditTariffRequest request)
        {
            var tariff = await _context.CreditTariffs.FindAsync(id);
            if (tariff == null) return null;

            tariff.Name = request.Name;
            tariff.InterestRate = request.InterestRate;
            tariff.Description = request.Description;
            tariff.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return new CreditTariffDto
            {
                Id = tariff.Id,
                Name = tariff.Name,
                InterestRate = tariff.InterestRate,
                Description = tariff.Description,
                CreatedAt = tariff.CreatedAt,
                IsActive = tariff.IsActive
            };
        }

        public async Task<bool> DeleteTariffAsync(Guid id)
        {
            var tariff = await _context.CreditTariffs.FindAsync(id);
            if (tariff == null) return false;

            _context.CreditTariffs.Remove(tariff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CreditTariffDto>> GetActiveTariffsAsync()
        {
            var tariffs = await _context.CreditTariffs
                .Where(t => t.IsActive)
                .ToListAsync();

            return tariffs.Select(t => new CreditTariffDto
            {
                Id = t.Id,
                Name = t.Name,
                InterestRate = t.InterestRate,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                IsActive = t.IsActive
            });
        }
    }
}