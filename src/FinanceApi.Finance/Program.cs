using FinanceApi.Finance.Api.GraphQL.Errors;
using FinanceApi.Finance.Api.GraphQL.Mutations;
using FinanceApi.Finance.Api.GraphQL.Queries;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Infrastructure.Http;
using FinanceApi.Finance.Infrastructure.Kafka;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// Persistence
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// User context (lê headers injetados pelo gateway)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// Application services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFinancialIntegrationService, FinancialIntegrationService>();

// Kafka consumer
builder.Services.AddScoped<WebhookEventHandler>();
builder.Services.AddHostedService<WebhookEventConsumer>();



// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name(OperationTypeNames.Query))
    .AddTypeExtension<AccountQueries>()
    .AddTypeExtension<CategoryQueries>()
    .AddTypeExtension<TransactionQueries>()
    .AddTypeExtension<FinancialIntegrationQueries>()
    .AddMutationType(d => d.Name(OperationTypeNames.Mutation))
    .AddTypeExtension<AccountMutations>()
    .AddTypeExtension<CategoryMutations>()
    .AddTypeExtension<TransactionMutations>()
    .AddTypeExtension<FinancialIntegrationMutations>()
    .AddErrorFilter<FinanceErrorFilter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    db.Database.Migrate();
}

app.UseSerilogRequestLogging();
app.MapGraphQL();

app.Run();
