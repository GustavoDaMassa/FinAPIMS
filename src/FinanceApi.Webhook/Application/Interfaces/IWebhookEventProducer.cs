using FinanceApi.Shared.Contracts.Events;

namespace FinanceApi.Webhook.Application.Interfaces;

public interface IWebhookEventProducer
{
    Task PublishAsync(WebhookEvent evt);
}
