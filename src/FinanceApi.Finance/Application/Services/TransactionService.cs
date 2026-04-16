using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Application.Services;

public class TransactionService(FinanceDbContext db) : ITransactionService
{
    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request)
    {
        var accountExists = await db.Accounts.AnyAsync(a => a.Id == request.AccountId);
        if (!accountExists)
            throw new AccountNotFoundException(request.AccountId);

        var transaction = Transaction.Create(
            request.Amount, request.Type, request.Description,
            request.Source, request.Destination, request.TransactionDate,
            request.AccountId, request.CategoryId, request.ExternalId);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
        await RecalculateBalanceAsync(request.AccountId);
        return ToDto(transaction);
    }

    public async Task<TransactionDto> FindByIdAsync(Guid id)
    {
        var transaction = await db.Transactions.FindAsync(id)
            ?? throw new TransactionNotFoundException(id);
        return ToDto(transaction);
    }

    public async Task<TransactionListWithBalanceDto> ListByAccountAsync(Guid accountId)
    {
        var transactions = await db.Transactions
            .Where(t => t.AccountId == accountId)
            .ToListAsync();
        return BuildResult(transactions);
    }

    public async Task<TransactionListWithBalanceDto> ListByPeriodAsync(Guid accountId, DateOnly start, DateOnly end)
    {
        var transactions = await db.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionDate >= start && t.TransactionDate <= end)
            .ToListAsync();
        return BuildResult(transactions);
    }

    public async Task<TransactionListWithBalanceDto> ListByTypeAsync(Guid accountId, TransactionType type)
    {
        var transactions = await db.Transactions
            .Where(t => t.AccountId == accountId && t.Type == type)
            .ToListAsync();
        return BuildResult(transactions);
    }

    public async Task<TransactionListWithBalanceDto> ListByCategoriesAsync(Guid accountId, IList<Guid> categoryIds)
    {
        var transactions = await db.Transactions
            .Where(t => t.AccountId == accountId && t.CategoryId.HasValue && categoryIds.Contains(t.CategoryId.Value))
            .ToListAsync();
        return BuildResult(transactions);
    }

    public async Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionRequest request)
    {
        var transaction = await db.Transactions.FindAsync(id)
            ?? throw new TransactionNotFoundException(id);
        transaction.Update(
            request.Amount, request.Type, request.Description,
            request.Source, request.Destination, request.TransactionDate, request.CategoryId);
        await db.SaveChangesAsync();
        await RecalculateBalanceAsync(transaction.AccountId);
        return ToDto(transaction);
    }

    public async Task<TransactionDto> CategorizeAsync(Guid id, Guid? categoryId)
    {
        var transaction = await db.Transactions.FindAsync(id)
            ?? throw new TransactionNotFoundException(id);
        transaction.Categorize(categoryId);
        await db.SaveChangesAsync();
        return ToDto(transaction);
    }

    public async Task<bool> ExistsByExternalIdAsync(string externalId) =>
        await db.Transactions.AnyAsync(t => t.ExternalId == externalId);

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await db.Transactions.FindAsync(id)
            ?? throw new TransactionNotFoundException(id);
        var accountId = transaction.AccountId;
        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();
        await RecalculateBalanceAsync(accountId);
    }

    private async Task RecalculateBalanceAsync(Guid accountId)
    {
        var rows = await db.Transactions
            .Where(t => t.AccountId == accountId)
            .Select(t => new { t.Type, t.Amount })
            .ToListAsync();

        var balance = rows.Sum(t => t.Type.Apply(t.Amount));

        var account = await db.Accounts.FindAsync(accountId);
        if (account is not null)
        {
            account.UpdateBalance(balance);
            await db.SaveChangesAsync();
        }
    }

    private static TransactionListWithBalanceDto BuildResult(List<Transaction> transactions)
    {
        var balance = transactions.Sum(t => t.Type.Apply(t.Amount));
        return new TransactionListWithBalanceDto(balance, transactions.Select(ToDto).ToList());
    }

    private static TransactionDto ToDto(Transaction t) =>
        new(t.Id, t.Amount, t.Type, t.Description, t.Source, t.Destination, t.TransactionDate, t.AccountId, t.CategoryId, t.ExternalId);
}
