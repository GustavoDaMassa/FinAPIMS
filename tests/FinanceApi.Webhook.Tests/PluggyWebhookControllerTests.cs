using FinanceApi.Shared.Contracts.Events;
using FinanceApi.Webhook.Api.Controllers;
using FinanceApi.Webhook.Application.Dtos;
using FinanceApi.Webhook.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FinanceApi.Webhook.Tests;

public class PluggyWebhookControllerTests
{
    private static (PluggyWebhookController controller, IPluggyClient pluggyClient, IWebhookEventProducer producer)
        CreateController()
    {
        var pluggyClient = Substitute.For<IPluggyClient>();
        var producer = Substitute.For<IWebhookEventProducer>();
        return (new PluggyWebhookController(pluggyClient, producer), pluggyClient, producer);
    }

    [Fact]
    public async Task Receive_ShouldFetchTransactionsAndPublishEvent()
    {
        var (controller, pluggyClient, producer) = CreateController();

        var transactions = new List<PluggyTransaction>
        {
            new("ext-1", 150.00m, "CREDIT", "Salário", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-1"),
            new("ext-2",  50.00m, "DEBIT",  "Mercado", DateOnly.FromDateTime(DateTime.Today), "pluggy-acc-1")
        };

        pluggyClient
            .FetchTransactionsAsync("http://pluggy.ai/transactions?link=abc")
            .Returns(transactions);

        var payload = new PluggyWebhookPayload("item-123", "http://pluggy.ai/transactions?link=abc");
        var result = await controller.Receive(payload);

        Assert.IsType<OkResult>(result);
        await producer.Received(1).PublishAsync(Arg.Is<WebhookEvent>(e =>
            e.LinkId == "item-123" &&
            e.Transactions.Count == 2 &&
            e.Transactions[0].ExternalId == "ext-1" &&
            e.Transactions[1].PluggyType == "DEBIT"));
    }

    [Fact]
    public async Task Receive_WhenNoTransactions_ShouldPublishEmptyEvent()
    {
        var (controller, pluggyClient, producer) = CreateController();

        pluggyClient
            .FetchTransactionsAsync(Arg.Any<string>())
            .Returns([]);

        var payload = new PluggyWebhookPayload("item-999", "http://pluggy.ai/transactions?link=xyz");
        await controller.Receive(payload);

        await producer.Received(1).PublishAsync(Arg.Is<WebhookEvent>(e =>
            e.LinkId == "item-999" &&
            e.Transactions.Count == 0));
    }

    [Fact]
    public void HealthCheck_ShouldReturnOk()
    {
        var (controller, _, _) = CreateController();

        var result = controller.HealthCheck();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
