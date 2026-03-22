using Microsoft.EntityFrameworkCore;
using BankSystem.Models;

namespace BankSystem.Data
{
    public class BankSystemContext : DbContext
    {
        public BankSystemContext(DbContextOptions<BankSystemContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.HasIndex(e => e.AccountNumber).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.RelatedTransactionId);

                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}