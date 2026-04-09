using BankSystem.Credit.Data;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Credit.Services
{
    /// Каждую минуту проверяет кредиты и ставит статус "Overdue",
    /// если дата платежа прошла, а кредит ещё не закрыт.
    public class CreditStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreditStatusBackgroundService> _logger;

        public CreditStatusBackgroundService(IServiceScopeFactory scopeFactory,
                                             ILogger<CreditStatusBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckOverdueCreditsAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckOverdueCreditsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreditDbContext>();

            var now = DateTime.UtcNow;
            var overdueCredits = await db.Credits
                .Where(c => c.Status == "active" && c.PaymentDueDate < now)
                .ToListAsync();

            if (overdueCredits.Count == 0) return;

            foreach (var credit in overdueCredits)
                credit.Status = "Overdue";

            await db.SaveChangesAsync();
            _logger.LogInformation("Marked {Count} credit(s) as Overdue", overdueCredits.Count);
        }
    }
}
