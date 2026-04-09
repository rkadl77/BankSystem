using Microsoft.EntityFrameworkCore;
using BankSystem.Credit.Models;

namespace BankSystem.Credit.Data
{
    public class CreditDbContext : DbContext
    {
        public CreditDbContext(DbContextOptions<CreditDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Credit> Credits { get; set; }
        public DbSet<CreditTariff> CreditTariffs { get; set; }
        public DbSet<CreditPayment> CreditPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CreditTariff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InterestRate).HasPrecision(5, 2);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasMany(e => e.Credits)
                    .WithOne(e => e.Tariff)
                    .HasForeignKey(e => e.TariffId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Models.Credit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(5, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Tariff)
                    .WithMany(e => e.Credits)
                    .HasForeignKey(e => e.TariffId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Payments)
                    .WithOne(e => e.Credit)
                    .HasForeignKey(e => e.CreditId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CreditPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Credit)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.CreditId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}