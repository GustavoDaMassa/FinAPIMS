using FinanceApi.Shared.Contracts.Events;
using FinanceApi.Webhook.Application.Dtos;
using FinanceApi.Webhook.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Webhook.Api.Controllers;

[ApiController]
[Route("webhook/pluggy")]
public class PluggyWebhookController(IPluggyClient pluggyClient, IWebhookEventProducer producer) : ControllerBase
{
    [HttpGet]
    public IActionResult HealthCheck() => Ok("Webhook endpoint is active.");

    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] PluggyWebhookPayload payload)
    {
        var transactions = await pluggyClient.FetchTransactionsAsync(payload.CreatedTransactionsLink);

        var evt = new WebhookEvent(
            payload.ItemId,
            transactions.Select(t => new ExternalTransaction(
                t.Id, t.Amount, t.Type, t.Description, t.Date, t.AccountId
            )).ToList()
        );

        await producer.PublishAsync(evt);
        return Ok();
    }
}
