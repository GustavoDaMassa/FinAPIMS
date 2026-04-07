using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Application.Dtos;

public record OfxImportResult(int Imported, int Skipped, IReadOnlyList<TransactionDto> Transactions);
