using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Application.Services;

public class AccountService(FinanceDbContext db) : IAccountService
{
    public async Task<AccountDto> CreateAsync(CreateAccountRequest request)
    {
        var account = Account.Create(request.AccountName, request.Institution, request.Description, request.UserId, request.IntegrationId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return ToDto(account);
    }

    public async Task<AccountDto> FindByIdAsync(Guid id)
    {
        var account = await db.Accounts.FindAsync(id)
            ?? throw new AccountNotFoundException(id);
        return ToDto(account);
    }

    public async Task<IReadOnlyList<AccountDto>> ListByUserAsync(Guid userId)
    {
        var accounts = await db.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();
        return accounts.Select(ToDto).ToList();
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request)
    {
        var account = await db.Accounts.FindAsync(id)
            ?? throw new AccountNotFoundException(id);
        account.Update(request.AccountName, request.Institution, request.Description);
        await db.SaveChangesAsync();
        return ToDto(account);
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await db.Accounts.FindAsync(id)
            ?? throw new AccountNotFoundException(id);
        db.Accounts.Remove(account);
        await db.SaveChangesAsync();
    }

    public async Task<AccountDto> LinkToPluggyAsync(LinkAccountRequest request)
    {
        var integration = await db.FinancialIntegrations.FindAsync(request.IntegrationId)
            ?? throw new FinancialIntegrationNotFoundException(request.IntegrationId);

        var account = Account.Create(request.AccountName, request.Institution, request.Description, integration.UserId, request.IntegrationId);
        account.LinkToPluggy(request.PluggyAccountId, request.IntegrationId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return ToDto(account);
    }

    private static AccountDto ToDto(Account a) =>
        new(a.Id, a.AccountName, a.Institution, a.Description, a.Balance, a.UserId, a.IntegrationId, a.PluggyAccountId);
}
