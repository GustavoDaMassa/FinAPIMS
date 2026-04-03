namespace FinanceApi.Shared.Contracts.Events;

public record WebhookEvent(
    string ItemId,
    string TransactionsLink
);
