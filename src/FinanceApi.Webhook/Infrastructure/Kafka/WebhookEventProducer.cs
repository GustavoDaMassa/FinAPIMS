using System.Text.Json;
using Confluent.Kafka;
using FinanceApi.Shared.Contracts.Events;
using FinanceApi.Webhook.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceApi.Webhook.Infrastructure.Kafka;

public class WebhookEventProducer(IConfiguration configuration, ILogger<WebhookEventProducer> logger)
    : IWebhookEventProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer = new ProducerBuilder<Null, string>(
        new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] }).Build();

    private readonly string _topic = configuration["Kafka:Topics:WebhookEvents"] ?? "webhook.events";

    public async Task PublishAsync(WebhookEvent evt)
    {
        var message = JsonSerializer.Serialize(evt);
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        logger.LogInformation("Published WebhookEvent for linkId={LinkId} with {Count} transactions",
            evt.LinkId, evt.Transactions.Count);
    }

    public void Dispose() => _producer.Dispose();
}
