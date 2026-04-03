using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Kafka;
using FinanceApi.Shared.Contracts.Events;

namespace FinanceApi.Finance.Tests;

public class WebhookEventConsumerTests
{
    private static (WebhookEventHandler handler, FinanceDbContext db) CreateHandler()
    {
        var db = TestHelpers.CreateDb();
        ITransactionService txService = new TransactionService(db);
        return (new WebhookEventHandler(db, txService), db);
    }

    private static async Task<(FinancialIntegration integration, Account account)> SeedIntegrationAndAccount(
        FinanceDbContext db,
        string linkId = "link-abc",
        string pluggyAccountId = "pluggy-acc-1")
    {
        var integration = FinancialIntegration.Create(AggregatorType.Pluggy, linkId, TestHelpers.UserId);
        var account = Account.Create("Conta", "Banco", null, TestHelpers.UserId);
        account.LinkToPluggy(pluggyAccountId, integration.Id);
        db.FinancialIntegrations.Add(integration);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return (integration, account);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactionsForKnownAccount()
    {
        var (handler, db) = CreateHandler();
        await SeedIntegrationAndAccount(db);

        var evt = new WebhookEvent("link-abc",
        [
            new ExternalTransaction("ext-1", 200m, "CREDIT", "Salário", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-1"),
            new ExternalTransaction("ext-2",  80m, "DEBIT",  "Mercado", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-1")
        ]);

        await handler.HandleAsync(evt);

        Assert.Equal(2, await db.Transactions.CountAsync());
        var transactions = await db.Transactions.ToListAsync();
        Assert.Contains(transactions, t => t.ExternalId == "ext-1" && t.Type == TransactionType.Inflow);
        Assert.Contains(transactions, t => t.ExternalId == "ext-2" && t.Type == TransactionType.Outflow);
    }

    [Fact]
    public async Task Handle_ShouldSkipDuplicateExternalId()
    {
        var (handler, db) = CreateHandler();
        await SeedIntegrationAndAccount(db);

        var evt = new WebhookEvent("link-abc",
        [
            new ExternalTransaction("ext-dup", 100m, "CREDIT", "Já existe", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-1")
        ]);

        await handler.HandleAsync(evt);
        await handler.HandleAsync(evt); // segundo processamento

        Assert.Equal(1, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Handle_ShouldSkipTransactionWhenAccountNotFoundForPluggyId()
    {
        var (handler, db) = CreateHandler();
        await SeedIntegrationAndAccount(db, pluggyAccountId: "pluggy-acc-known");

        var evt = new WebhookEvent("link-abc",
        [
            new ExternalTransaction("ext-unknown", 50m, "CREDIT", "Desc", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-unknown")
        ]);

        await handler.HandleAsync(evt);

        Assert.Equal(0, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenLinkIdNotFound_ShouldThrow()
    {
        var (handler, _) = CreateHandler();

        var evt = new WebhookEvent("link-inexistente", []);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(evt));
    }
}
