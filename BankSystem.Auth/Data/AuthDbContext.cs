using Microsoft.EntityFrameworkCore;
using BankSystem.Auth.Models;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.Extensions.Options;

namespace BankSystem.Auth.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthUser> AuthUsers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public DbSet<Key> Keys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            modelBuilder.ConfigureClientContext(new ConfigurationStoreOptions());
            modelBuilder.ConfigureResourcesContext(new ConfigurationStoreOptions());
            modelBuilder.ConfigurePersistedGrantContext(new OperationalStoreOptions());
        }
    }
}