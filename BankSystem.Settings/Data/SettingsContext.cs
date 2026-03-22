using Microsoft.EntityFrameworkCore;
using BankSystem.Settings.Models;

namespace BankSystem.Settings.Data
{
    public class SettingsContext : DbContext
    {
        public SettingsContext(DbContextOptions<SettingsContext> options)
            : base(options)
        {
        }

        public DbSet<UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.Theme).IsRequired().HasMaxLength(20);
                entity.Property(e => e.HiddenAccountIds).IsRequired();
            });
        }
    }
}
