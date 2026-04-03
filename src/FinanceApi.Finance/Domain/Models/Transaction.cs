using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Domain.Models;

public class Transaction
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Description { get; private set; }
    public string? Source { get; private set; }
    public string? Destination { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public string? ExternalId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }

    public Account Account { get; private set; } = null!;
    public Category? Category { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        decimal amount,
        TransactionType type,
        string? description,
        string? source,
        string? destination,
        DateOnly transactionDate,
        Guid accountId,
        Guid? categoryId = null,
        string? externalId = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Type = type,
            Description = description,
            Source = source,
            Destination = destination,
            TransactionDate = transactionDate,
            AccountId = accountId,
            CategoryId = categoryId,
            ExternalId = externalId
        };
    }

    public void Categorize(Guid? categoryId) => CategoryId = categoryId;
}
