namespace FinanceApi.Shared.Contracts.Events;

// Evento publicado pelo webhook-service após buscar as transações no Pluggy.
// O finance-service consome esse evento e persiste as transações no domínio.
public record WebhookEvent(
    string LinkId,
    IReadOnlyList<ExternalTransaction> Transactions
);

public record ExternalTransaction(
    string ExternalId,
    decimal Amount,
    string PluggyType,   // "CREDIT" | "DEBIT" — convertido para TransactionType no consumer
    string? Description,
    DateOnly Date,
    string PluggyAccountId
);
