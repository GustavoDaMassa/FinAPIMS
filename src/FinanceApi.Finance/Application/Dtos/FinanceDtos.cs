using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Application.Dtos;

public record AccountDto(
    Guid Id,
    string AccountName,
    string Institution,
    string? Description,
    decimal Balance,
    Guid UserId,
    Guid? IntegrationId,
    string? PluggyAccountId);

public record CategoryDto(Guid Id, string Name, Guid UserId);

public record TransactionDto(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    string? Description,
    string? Source,
    string? Destination,
    DateOnly TransactionDate,
    Guid AccountId,
    Guid? CategoryId,
    string? ExternalId);

public record TransactionListWithBalanceDto(decimal Balance, IReadOnlyList<TransactionDto> Transactions);

public record FinancialIntegrationDto(
    Guid Id,
    AggregatorType Aggregator,
    string LinkId,
    string? Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    Guid UserId);

public record CreateAccountRequest(string AccountName, string Institution, string? Description, Guid UserId, Guid? IntegrationId = null);
public record UpdateAccountRequest(string AccountName, string Institution, string? Description);
public record LinkAccountRequest(Guid IntegrationId, string PluggyAccountId, string AccountName, string Institution, string? Description);

public record CreateCategoryRequest(string Name, Guid UserId);
public record UpdateCategoryRequest(string Name);

public record CreateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    string? Description,
    string? Source,
    string? Destination,
    DateOnly TransactionDate,
    Guid AccountId,
    Guid? CategoryId = null,
    string? ExternalId = null);

public record UpdateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    string? Description,
    string? Source,
    string? Destination,
    DateOnly TransactionDate,
    Guid? CategoryId);

public record CreateFinancialIntegrationRequest(AggregatorType Aggregator, string LinkId, Guid UserId);
