using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Infrastructure.Ofx;

public record OfxTransactionRow(
    string FitId,
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description);
