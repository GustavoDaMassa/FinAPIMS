using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Domain.Models;

public class FinancialIntegration
{
    public Guid Id { get; private set; }
    public AggregatorType Aggregator { get; private set; }
    public string LinkId { get; private set; } = string.Empty;
    public string? Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public Guid UserId { get; private set; }

    public ICollection<Account> Accounts { get; private set; } = [];

    private FinancialIntegration() { }

    public static FinancialIntegration Create(AggregatorType aggregator, string linkId, Guid userId)
    {
        return new FinancialIntegration
        {
            Id = Guid.NewGuid(),
            Aggregator = aggregator,
            LinkId = linkId,
            Status = "UPDATED",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(12),
            UserId = userId
        };
    }

    public void UpdateStatus(string status) => Status = status;
    public void Renew() => ExpiresAt = DateTime.UtcNow.AddMonths(12);
}
