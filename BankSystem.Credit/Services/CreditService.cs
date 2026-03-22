using BankSystem.Credit.Clients;
using BankSystem.Credit.Data;
using BankSystem.Credit.DTOs;
using BankSystem.Credit.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.Credit.Services
{
    public class CreditService : ICreditService
    {
        private readonly CreditDbContext _context;
        private readonly CoreServiceClient _coreClient;

        public CreditService(CreditDbContext context, CoreServiceClient coreClient)
        {
            _context = context;
            _coreClient = coreClient;
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

            var account = await _coreClient.GetAccountAsync(request.AccountId);
            if (account == null)
                throw new InvalidOperationException("Account not found in Core service");

            if (!account.IsActive)
                throw new InvalidOperationException("Account is not active");

            var hasFunds = await _coreClient.HasSufficientFundsAsync(request.Amount);
            if (!hasFunds)
                throw new InvalidOperationException("Master account has insufficient funds");

            var transferSuccess = await _coreClient.TransferFromMasterAsync(request.AccountId, request.Amount);
            if (!transferSuccess)
                throw new InvalidOperationException("Failed to transfer funds from master account");

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

            var accountCurrency = await _coreClient.GetAccountCurrencyAsync(credit.AccountId);
            if (string.IsNullOrEmpty(accountCurrency))
            {
                return false;
            }

            var withdrawSuccess = await _coreClient.WithdrawFromAccountAsync(
                credit.AccountId,
                request.Amount,
                accountCurrency,
                $"Credit repayment for credit {credit.Id}"
            );

            if (!withdrawSuccess)
                return false;

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
                Status = "completed",
                TransactionId = "pending"
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

        public async Task<IEnumerable<CreditDto>> GetOverdueCreditsAsync(Guid clientId)
        {
            var credits = await _context.Credits
                .Include(c => c.Tariff)
                .Where(c => c.ClientId == clientId)
                .Where(c => c.Status == "Overdue" || (c.Status != "Paid" && c.Status != "Defaulted" && c.PaymentDueDate < DateTime.UtcNow))
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

        public async Task<CreditRatingDto> CalculateCreditRatingAsync(Guid clientId)
        {
            var credits = await _context.Credits
                .Include(c => c.Payments)
                .Where(c => c.ClientId == clientId)
                .ToListAsync();

            var totalCount = credits.Count;
            var overdueCount = credits.Count(c => c.Status == "Overdue" || c.DaysOverdue > 0);
            var defaultedCount = credits.Count(c => c.Status == "Defaulted");

            var totalOverdueDays = credits.Sum(c => c.DaysOverdue);
            var onTimePayments = credits
                .SelectMany(c => c.Payments ?? new List<CreditPayment>())
                .Count(p => p.Status == "completed" && p.PaymentDate <= p.PaymentDate.AddDays(-1));

            var rating = CreditRatingCalculator.CalculateRating(totalOverdueDays, defaultedCount, onTimePayments);
            var description = CreditRatingCalculator.GetRatingDescription(rating);

            var onTimePercentage = totalCount > 0
                ? (decimal)(totalCount - overdueCount - defaultedCount) / totalCount * 100
                : 100m;

            return new CreditRatingDto
            {
                ClientId = clientId,
                Rating = rating,
                Description = description,
                OverdueCount = overdueCount,
                TotalCount = totalCount,
                OnTimePercentage = onTimePercentage
            };
        }

        public async Task<bool> UpdateCreditStatusAsync(Guid creditId)
        {
            var credit = await _context.Credits.FindAsync(creditId);
            if (credit == null)
                return false;

            var now = DateTime.UtcNow;

            // If current date is past due date and not paid, mark as Overdue
            if (now > credit.PaymentDueDate && credit.Status != "Paid" && credit.Status != "Defaulted")
            {
                credit.Status = "Overdue";
            }

            // If was Overdue but now has payment after due date, mark as Paid
            if (credit.Status == "Overdue" && credit.LastPaymentDate.HasValue && credit.LastPaymentDate.Value > credit.PaymentDueDate)
            {
                credit.Status = "Paid";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CreditDetailDto?> GetCreditDetailsAsync(Guid creditId)
        {
            var credit = await _context.Credits
                .Include(c => c.Tariff)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == creditId);

            if (credit == null) return null;

            var rating = await CalculateCreditRatingAsync(credit.ClientId);

            return new CreditDetailDto
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
                PaymentDueDate = credit.PaymentDueDate,
                LastPaymentDate = credit.LastPaymentDate,
                TermMonths = credit.TermMonths,
                DaysOverdue = credit.DaysOverdue,
                RatingDescription = rating.Description,
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
    }
}