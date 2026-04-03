using FinanceApi.Webhook.Application.Interfaces;
using FinanceApi.Webhook.Application.Services;
using FinanceApi.Webhook.Infrastructure.Kafka;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();

// Pluggy HTTP client
builder.Services.AddHttpClient<IPluggyClient, PluggyClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Pluggy:BaseUrl"]!);
});

// Kafka producer
builder.Services.AddSingleton<IWebhookEventProducer, WebhookEventProducer>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
