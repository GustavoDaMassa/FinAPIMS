using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Models;

namespace FinanceApi.Finance.Tests;

public class AccountServiceTests
{
    private static IAccountService CreateService(out FinanceDbContext db)
    {
        db = TestHelpers.CreateDb();
        return new AccountService(db);
    }

    [Fact]
    public async Task Create_ShouldPersistAccountAndReturnDto()
    {
        var service = CreateService(out var db);
        var request = new CreateAccountRequest("Conta Corrente", "Banco X", "descrição", TestHelpers.UserId);

        var result = await service.CreateAsync(request);

        Assert.Equal("Conta Corrente", result.AccountName);
        Assert.Equal("Banco X", result.Institution);
        Assert.Equal(0m, result.Balance);
        Assert.Equal(TestHelpers.UserId, result.UserId);
        Assert.Equal(1, await db.Accounts.CountAsync());
    }

    [Fact]
    public async Task FindById_WhenExists_ShouldReturnDto()
    {
        var service = CreateService(out var db);
        var account = Account.Create("Poupança", "Banco Y", null, TestHelpers.UserId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var result = await service.FindByIdAsync(account.Id);

        Assert.Equal(account.Id, result.Id);
        Assert.Equal("Poupança", result.AccountName);
    }

    [Fact]
    public async Task FindById_WhenNotFound_ShouldThrowAccountNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.FindByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListByUser_ShouldReturnOnlyAccountsForThatUser()
    {
        var service = CreateService(out var db);
        db.Accounts.Add(Account.Create("A1", "Banco", null, TestHelpers.UserId));
        db.Accounts.Add(Account.Create("A2", "Banco", null, TestHelpers.UserId));
        db.Accounts.Add(Account.Create("A3", "Banco", null, TestHelpers.OtherUserId));
        await db.SaveChangesAsync();

        var result = await service.ListByUserAsync(TestHelpers.UserId);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(TestHelpers.UserId, a.UserId));
    }

    [Fact]
    public async Task Update_ShouldModifyAccountFields()
    {
        var service = CreateService(out var db);
        var account = Account.Create("Antiga", "Banco", null, TestHelpers.UserId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(account.Id, new UpdateAccountRequest("Nova", "Outro Banco", "desc"));

        Assert.Equal("Nova", result.AccountName);
        Assert.Equal("Outro Banco", result.Institution);
        Assert.Equal("desc", result.Description);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateAccountRequest("X", "Y", null)));
    }

    [Fact]
    public async Task Delete_ShouldRemoveAccount()
    {
        var service = CreateService(out var db);
        var account = Account.Create("Del", "Banco", null, TestHelpers.UserId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        await service.DeleteAsync(account.Id);

        Assert.Equal(0, await db.Accounts.CountAsync());
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }
}
