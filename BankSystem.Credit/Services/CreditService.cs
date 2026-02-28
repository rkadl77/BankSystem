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
    public class CreditService : ICreditService
    {
        private readonly CreditDbContext _context;

        public CreditService(CreditDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CreditDto>> GetCreditsByClientIdAsync(Guid clientId)
        {
            var credits = await _context.Credits
                .Include(c => c.Tariff)
                .Where(c => c.ClientId == clientId)
                .ToListAsync();

            return credits.Select(c => new CreditDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                AccountId = c.AccountId,
                TariffName = c.Tariff != null ? c.Tariff.Name : "",
                Amount = c.Amount,
                RemainingAmount = c.RemainingAmount,
                InterestRate = c.InterestRate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status
            });
        }

        public async Task<CreditDetailsDto?> GetCreditByIdAsync(Guid id)
        {
            var credit = await _context.Credits
                .Include(c => c.Tariff)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credit == null) return null;

            return new CreditDetailsDto
            {
                Id = credit.Id,
                ClientId = credit.ClientId,
                AccountId = credit.AccountId,
                TariffName = credit.Tariff != null ? credit.Tariff.Name : "",
                Amount = credit.Amount,
                RemainingAmount = credit.RemainingAmount,
                InterestRate = credit.InterestRate,
                StartDate = credit.StartDate,
                EndDate = credit.EndDate,
                Status = credit.Status,
                Payments = credit.Payments != null
                    ? credit.Payments.Select(p => new CreditPaymentDto
                    {
                        Id = p.Id,
                        CreditId = p.CreditId,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status
                    }).ToList()
                    : new List<CreditPaymentDto>()
            };
        }

        public async Task<CreditDto> CreateCreditAsync(CreateCreditRequest request)
        {
            var tariff = await _context.CreditTariffs.FindAsync(request.TariffId);
            if (tariff == null)
                throw new InvalidOperationException("Tariff not found");

            var credit = new Models.Credit 
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                AccountId = request.AccountId,
                TariffId = request.TariffId,
                Amount = request.Amount,
                RemainingAmount = request.Amount,
                InterestRate = tariff.InterestRate,
                StartDate = DateTime.UtcNow,
                Status = "active"
            };

            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();

            return new CreditDto
            {
                Id = credit.Id,
                ClientId = credit.ClientId,
                AccountId = credit.AccountId,
                TariffName = tariff.Name,
                Amount = credit.Amount,
                RemainingAmount = credit.RemainingAmount,
                InterestRate = credit.InterestRate,
                StartDate = credit.StartDate,
                EndDate = credit.EndDate,
                Status = credit.Status
            };
        }

        public async Task<bool> RepayCreditAsync(RepayCreditRequest request)
        {
            var credit = await _context.Credits.FindAsync(request.CreditId);
            if (credit == null) return false;
            if (credit.Status != "active") return false;
            if (request.Amount <= 0) return false;
            if (request.Amount > credit.RemainingAmount) return false;

            credit.RemainingAmount -= request.Amount;

            if (credit.RemainingAmount == 0)
            {
                credit.Status = "closed";
                credit.EndDate = DateTime.UtcNow;
            }

            var payment = new CreditPayment
            {
                Id = Guid.NewGuid(),
                CreditId = credit.Id,
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                Status = "completed"
            };

            _context.CreditPayments.Add(payment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CreditDto>> GetActiveCreditsAsync()
        {
            var credits = await _context.Credits
                .Include(c => c.Tariff)
                .Where(c => c.Status == "active")
                .ToListAsync();

            return credits.Select(c => new CreditDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                AccountId = c.AccountId,
                TariffName = c.Tariff != null ? c.Tariff.Name : "",
                Amount = c.Amount,
                RemainingAmount = c.RemainingAmount,
                InterestRate = c.InterestRate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status
            });
        }

        public async Task<decimal> GetTotalDebtByClientIdAsync(Guid clientId)
        {
            var credits = await _context.Credits
                .Where(c => c.ClientId == clientId && c.Status == "active")
                .ToListAsync();

            return credits.Sum(c => c.RemainingAmount);
        }
    }
}