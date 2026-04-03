using FinanceApi.Webhook.Application.Dtos;

namespace FinanceApi.Webhook.Application.Interfaces;

public interface IPluggyClient
{
    Task<IReadOnlyList<PluggyTransaction>> FetchTransactionsAsync(string transactionsLink);
}
