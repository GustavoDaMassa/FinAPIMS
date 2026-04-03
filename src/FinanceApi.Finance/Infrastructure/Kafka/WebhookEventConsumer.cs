using System.Text.Json;
using Confluent.Kafka;
using FinanceApi.Shared.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApi.Finance.Infrastructure.Kafka;

public class WebhookEventConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<WebhookEventConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:ConsumerGroup"] ?? "finance-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        var topic = configuration["Kafka:Topics:WebhookEvents"] ?? "webhook.events";
        consumer.Subscribe(topic);

        logger.LogInformation("Kafka consumer started, listening on topic {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                var evt = JsonSerializer.Deserialize<WebhookEvent>(result.Message.Value, JsonOptions);
                if (evt is null)
                {
                    logger.LogWarning("Received null or invalid WebhookEvent, skipping");
                    consumer.Commit(result);
                    continue;
                }

                await ProcessAsync(evt);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook event");
            }
        }

        consumer.Close();
    }

    private async Task ProcessAsync(WebhookEvent evt)
    {
        // Novo scope por mensagem — DbContext é scoped e não é thread-safe
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<WebhookEventHandler>();
        await handler.HandleAsync(evt);
    }
}
