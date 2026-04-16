using FinanceApi.Shared.Contracts.Events;
using FinanceApi.Webhook.Application.Dtos;
using FinanceApi.Webhook.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Webhook.Api.Controllers;

[ApiController]
[Route("webhook/pluggy")]
public class PluggyWebhookController(
    IPluggyClient pluggyClient,
    IWebhookEventProducer producer,
    ILogger<PluggyWebhookController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult HealthCheck() => Ok("Webhook endpoint is active.");

    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] PluggyWebhookPayload payload)
    {
        logger.LogInformation("[WEBHOOK] Notification received — itemId={ItemId} link={Link}",
            payload.ItemId, payload.CreatedTransactionsLink);

        var transactions = await pluggyClient.FetchTransactionsAsync(payload.CreatedTransactionsLink);

        logger.LogInformation("[WEBHOOK] Fetched {Count} transaction(s) from Pluggy — itemId={ItemId}",
            transactions.Count, payload.ItemId);

        var evt = new WebhookEvent(
            payload.ItemId,
            transactions.Select(t => new ExternalTransaction(
                t.Id, t.Amount, t.Type, t.Description, t.Date, t.AccountId
            )).ToList()
        );

        await producer.PublishAsync(evt);

        logger.LogInformation("[WEBHOOK] Published to Kafka — linkId={LinkId} transactions={Count}",
            payload.ItemId, transactions.Count);

        return Ok();
    }
}
