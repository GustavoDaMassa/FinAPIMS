using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Enums;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class TransactionQueries
{
    public async Task<TransactionDto> Transaction(
        Guid id,
        [Service] ITransactionService service) =>
        await service.FindByIdAsync(id);

    public async Task<TransactionListWithBalanceDto> Transactions(
        Guid accountId,
        [Service] ITransactionService service) =>
        await service.ListByAccountAsync(accountId);

    public async Task<TransactionListWithBalanceDto> TransactionsByPeriod(
        Guid accountId,
        DateOnly startDate,
        DateOnly endDate,
        [Service] ITransactionService service) =>
        await service.ListByPeriodAsync(accountId, startDate, endDate);

    public async Task<TransactionListWithBalanceDto> TransactionsByType(
        Guid accountId,
        TransactionType type,
        [Service] ITransactionService service) =>
        await service.ListByTypeAsync(accountId, type);

    public async Task<TransactionListWithBalanceDto> TransactionsByCategories(
        Guid accountId,
        List<Guid> categoryIds,
        [Service] ITransactionService service) =>
        await service.ListByCategoriesAsync(accountId, categoryIds);
}
