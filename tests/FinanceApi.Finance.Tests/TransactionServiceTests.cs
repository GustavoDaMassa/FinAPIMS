using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Domain.Models;

namespace FinanceApi.Finance.Tests;

public class TransactionServiceTests
{
    private static ITransactionService CreateService(out FinanceDbContext db)
    {
        db = TestHelpers.CreateDb();
        return new TransactionService(db);
    }

    private static Account SeedAccount(FinanceDbContext db, Guid? userId = null)
    {
        var account = Account.Create("Conta", "Banco", null, userId ?? TestHelpers.UserId);
        db.Accounts.Add(account);
        db.SaveChanges();
        return account;
    }

    private static CreateTransactionRequest InflowRequest(Guid accountId, decimal amount = 100m, DateOnly? date = null) =>
        new(amount, TransactionType.Inflow, "Salário", null, null, date ?? DateOnly.FromDateTime(DateTime.Today), accountId);

    private static CreateTransactionRequest OutflowRequest(Guid accountId, decimal amount = 50m, DateOnly? date = null) =>
        new(amount, TransactionType.Outflow, "Mercado", null, null, date ?? DateOnly.FromDateTime(DateTime.Today), accountId);

    [Fact]
    public async Task Create_ShouldPersistTransactionAndReturnDto()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);

        var result = await service.CreateAsync(InflowRequest(account.Id));

        Assert.Equal(100m, result.Amount);
        Assert.Equal(TransactionType.Inflow, result.Type);
        Assert.Equal(account.Id, result.AccountId);
        Assert.Equal(1, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Create_WhenAccountNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.CreateAsync(InflowRequest(Guid.NewGuid())));
    }

    [Fact]
    public async Task FindById_WhenExists_ShouldReturnDto()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var tx = await service.CreateAsync(InflowRequest(account.Id));

        var result = await service.FindByIdAsync(tx.Id);

        Assert.Equal(tx.Id, result.Id);
    }

    [Fact]
    public async Task FindById_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            service.FindByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListByAccount_ShouldReturnTransactionsWithCorrectBalance()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        await service.CreateAsync(InflowRequest(account.Id, 200m));
        await service.CreateAsync(OutflowRequest(account.Id, 80m));

        var result = await service.ListByAccountAsync(account.Id);

        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(120m, result.Balance); // 200 - 80
    }

    [Fact]
    public async Task ListByAccount_ShouldReturnOnlyTransactionsForThatAccount()
    {
        var service = CreateService(out var db);
        var account1 = SeedAccount(db);
        var account2 = SeedAccount(db);
        await service.CreateAsync(InflowRequest(account1.Id));
        await service.CreateAsync(InflowRequest(account2.Id));

        var result = await service.ListByAccountAsync(account1.Id);

        Assert.Single(result.Transactions);
        Assert.All(result.Transactions, t => Assert.Equal(account1.Id, t.AccountId));
    }

    [Fact]
    public async Task ListByPeriod_ShouldFilterByDateRange()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var jan = new DateOnly(2024, 1, 15);
        var mar = new DateOnly(2024, 3, 10);
        await service.CreateAsync(InflowRequest(account.Id, date: jan));
        await service.CreateAsync(InflowRequest(account.Id, date: mar));

        var result = await service.ListByPeriodAsync(account.Id,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));

        Assert.Single(result.Transactions);
        Assert.Equal(jan, result.Transactions[0].TransactionDate);
    }

    [Fact]
    public async Task ListByType_ShouldFilterByTransactionType()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        await service.CreateAsync(InflowRequest(account.Id));
        await service.CreateAsync(OutflowRequest(account.Id));

        var result = await service.ListByTypeAsync(account.Id, TransactionType.Inflow);

        Assert.Single(result.Transactions);
        Assert.Equal(TransactionType.Inflow, result.Transactions[0].Type);
    }

    [Fact]
    public async Task Categorize_ShouldSetCategoryOnTransaction()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var tx = await service.CreateAsync(InflowRequest(account.Id));
        var category = Category.Create("Salário", TestHelpers.UserId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var result = await service.CategorizeAsync(tx.Id, category.Id);

        Assert.Equal(category.Id, result.CategoryId);
    }

    [Fact]
    public async Task Categorize_WithNullCategoryId_ShouldClearCategory()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var category = Category.Create("Salário", TestHelpers.UserId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        var tx = await service.CreateAsync(InflowRequest(account.Id) with { CategoryId = category.Id });

        var result = await service.CategorizeAsync(tx.Id, null);

        Assert.Null(result.CategoryId);
    }

    [Fact]
    public async Task ExistsByExternalId_WhenExists_ShouldReturnTrue()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        await service.CreateAsync(InflowRequest(account.Id) with { ExternalId = "ext-123" });

        var result = await service.ExistsByExternalIdAsync("ext-123");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByExternalId_WhenNotExists_ShouldReturnFalse()
    {
        var service = CreateService(out _);

        var result = await service.ExistsByExternalIdAsync("inexistente");

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_ShouldRemoveTransaction()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var tx = await service.CreateAsync(InflowRequest(account.Id));

        await service.DeleteAsync(tx.Id);

        Assert.Equal(0, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_ShouldModifyFieldsAndReturnDto()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var tx = await service.CreateAsync(InflowRequest(account.Id, 100m));
        var newDate = new DateOnly(2024, 6, 1);
        var request = new UpdateTransactionRequest(200m, TransactionType.Outflow, "Aluguel", "Conta", null, newDate, null);

        var result = await service.UpdateAsync(tx.Id, request);

        Assert.Equal(tx.Id, result.Id);
        Assert.Equal(200m, result.Amount);
        Assert.Equal(TransactionType.Outflow, result.Type);
        Assert.Equal("Aluguel", result.Description);
        Assert.Equal("Conta", result.Source);
        Assert.Null(result.Destination);
        Assert.Equal(newDate, result.TransactionDate);
        Assert.Null(result.CategoryId);
        Assert.Equal(account.Id, result.AccountId); // AccountId nunca muda
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);
        var request = new UpdateTransactionRequest(50m, TransactionType.Inflow, null, null, null, DateOnly.FromDateTime(DateTime.Today), null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task ListByCategories_ShouldFilterByCategoryIds()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var cat = Category.Create("Alimentação", TestHelpers.UserId);
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        await service.CreateAsync(InflowRequest(account.Id) with { CategoryId = cat.Id });
        await service.CreateAsync(InflowRequest(account.Id));

        var result = await service.ListByCategoriesAsync(account.Id, [cat.Id]);

        Assert.Single(result.Transactions);
        Assert.Equal(cat.Id, result.Transactions[0].CategoryId);
    }

    [Fact]
    public async Task Create_ShouldUpdateAccountBalance()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);

        await service.CreateAsync(InflowRequest(account.Id, 200m));
        await service.CreateAsync(OutflowRequest(account.Id, 50m));

        var updated = await db.Accounts.FindAsync(account.Id);
        Assert.Equal(150m, updated!.Balance);
    }

    [Fact]
    public async Task Update_ShouldUpdateAccountBalance()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        var tx = await service.CreateAsync(InflowRequest(account.Id, 100m));

        await service.UpdateAsync(tx.Id, new UpdateTransactionRequest(
            300m, TransactionType.Inflow, null, null, null,
            DateOnly.FromDateTime(DateTime.Today), null));

        var updated = await db.Accounts.FindAsync(account.Id);
        Assert.Equal(300m, updated!.Balance);
    }

    [Fact]
    public async Task Delete_ShouldUpdateAccountBalance()
    {
        var service = CreateService(out var db);
        var account = SeedAccount(db);
        await service.CreateAsync(InflowRequest(account.Id, 100m));
        var tx2 = await service.CreateAsync(InflowRequest(account.Id, 50m));

        await service.DeleteAsync(tx2.Id);

        var updated = await db.Accounts.FindAsync(account.Id);
        Assert.Equal(100m, updated!.Balance);
    }
}
