namespace FinanceApi.Webhook.Application.Dtos;

public record PluggyWebhookPayload(string ItemId, string CreatedTransactionsLink);

public record PluggyTransaction(
    string Id,
    decimal Amount,
    string Type,
    string? Description,
    DateOnly Date,
    string AccountId);

public record PluggyAuthResponse(string ApiKey);

public record PluggyTransactionsResponse(IReadOnlyList<PluggyTransaction> Results);
