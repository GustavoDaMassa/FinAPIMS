using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateAsync(CreateTransactionRequest request);
    Task<TransactionDto> FindByIdAsync(Guid id);
    Task<TransactionListWithBalanceDto> ListByAccountAsync(Guid accountId);
    Task<TransactionListWithBalanceDto> ListByPeriodAsync(Guid accountId, DateOnly start, DateOnly end);
    Task<TransactionListWithBalanceDto> ListByTypeAsync(Guid accountId, TransactionType type);
    Task<TransactionListWithBalanceDto> ListByCategoriesAsync(Guid accountId, IList<Guid> categoryIds);
    Task<TransactionDto> CategorizeAsync(Guid id, Guid? categoryId);
    Task<bool> ExistsByExternalIdAsync(string externalId);
    Task DeleteAsync(Guid id);
}
