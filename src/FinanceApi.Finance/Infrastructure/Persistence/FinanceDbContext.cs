using FinanceApi.Finance.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Infrastructure.Persistence;

public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<FinancialIntegration> FinancialIntegrations => Set<FinancialIntegration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");

        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AccountName).IsRequired().HasMaxLength(100);
            e.Property(a => a.Institution).IsRequired().HasMaxLength(100);
            e.Property(a => a.Description).HasColumnType("text");
            e.Property(a => a.Balance).HasPrecision(15, 2);
            e.HasIndex(a => a.PluggyAccountId).IsUnique().HasFilter("pluggy_account_id IS NOT NULL");
            e.HasMany(a => a.Transactions).WithOne(t => t.Account).HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => new { c.Name, c.UserId }).IsUnique();
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasPrecision(15, 2).IsRequired();
            e.Property(t => t.Type).HasConversion<string>().IsRequired();
            e.HasIndex(t => t.ExternalId).IsUnique().HasFilter("external_id IS NOT NULL");
            e.HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FinancialIntegration>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Aggregator).HasConversion<string>().IsRequired();
            e.HasIndex(f => f.LinkId).IsUnique();
            e.HasMany(f => f.Accounts).WithOne().HasForeignKey(a => a.IntegrationId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
