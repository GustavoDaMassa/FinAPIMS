using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Infrastructure.Persistence;
using FinanceApi.Shared.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceApi.Finance.Infrastructure.Kafka;

// Lógica de negócio separada do loop Kafka para permitir testes unitários.
public class WebhookEventHandler(
    FinanceDbContext db,
    ITransactionService transactionService,
    ILogger<WebhookEventHandler>? logger = null)
{
    public async Task HandleAsync(WebhookEvent evt)
    {
        var integration = await db.FinancialIntegrations
            .SingleOrDefaultAsync(f => f.LinkId == evt.LinkId)
            ?? throw new InvalidOperationException($"Integration with linkId '{evt.LinkId}' not found.");

        foreach (var tx in evt.Transactions)
        {
            if (await transactionService.ExistsByExternalIdAsync(tx.ExternalId))
            {
                logger?.LogDebug("Skipping duplicate externalId={ExternalId}", tx.ExternalId);
                continue;
            }

            var account = await db.Accounts.SingleOrDefaultAsync(a =>
                a.PluggyAccountId == tx.PluggyAccountId &&
                a.UserId == integration.UserId);

            if (account is null)
            {
                logger?.LogWarning("No account found for pluggyAccountId={PluggyAccountId}, skipping", tx.PluggyAccountId);
                continue;
            }

            var type = TransactionTypeExtensions.FromPluggy(tx.PluggyType);

            await transactionService.CreateAsync(new CreateTransactionRequest(
                tx.Amount, type, tx.Description,
                null, null, tx.Date,
                account.Id, null, tx.ExternalId));
        }
    }
}
