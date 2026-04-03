namespace FinanceApi.Finance.Domain.Models;

public class Account
{
    public Guid Id { get; private set; }
    public string AccountName { get; private set; } = string.Empty;
    public string Institution { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Balance { get; private set; }
    public string? PluggyAccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? IntegrationId { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = [];

    private Account() { }

    public static Account Create(string accountName, string institution, string? description, Guid userId, Guid? integrationId = null)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            AccountName = accountName,
            Institution = institution,
            Description = description,
            Balance = 0m,
            UserId = userId,
            IntegrationId = integrationId
        };
    }

    public void Update(string accountName, string institution, string? description)
    {
        AccountName = accountName;
        Institution = institution;
        Description = description;
    }

    public void UpdateBalance(decimal balance) => Balance = balance;

    public void LinkToPluggy(string pluggyAccountId, Guid integrationId)
    {
        PluggyAccountId = pluggyAccountId;
        IntegrationId = integrationId;
    }
}
