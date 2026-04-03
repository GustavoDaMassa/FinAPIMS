namespace FinanceApi.Finance.Domain.Enums;

public enum TransactionType
{
    Inflow,
    Outflow
}

public static class TransactionTypeExtensions
{
    public static decimal Apply(this TransactionType type, decimal amount) =>
        type == TransactionType.Inflow ? amount : -amount;

    public static TransactionType FromPluggy(string pluggyType) =>
        pluggyType.ToUpperInvariant() == "CREDIT" ? TransactionType.Inflow : TransactionType.Outflow;
}
