using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Enums;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TransactionMutations
{
    public async Task<TransactionDto> CreateTransaction(
        decimal amount,
        TransactionType type,
        string? description,
        string? source,
        string? destination,
        DateOnly transactionDate,
        Guid accountId,
        Guid? categoryId,
        [Service] ITransactionService service) =>
        await service.CreateAsync(new CreateTransactionRequest(
            amount, type, description, source, destination, transactionDate, accountId, categoryId));

    public async Task<TransactionDto> UpdateTransaction(
        Guid id,
        decimal amount,
        TransactionType type,
        string? description,
        string? source,
        string? destination,
        DateOnly transactionDate,
        Guid? categoryId,
        [Service] ITransactionService service)
    {
        // FindById primeiro para lançar not found se não existir,
        // depois recria via delete + create para simplificar o domínio.
        // Alternativa futura: adicionar Update na entidade.
        var existing = await service.FindByIdAsync(id);
        await service.DeleteAsync(id);
        return await service.CreateAsync(new CreateTransactionRequest(
            amount, type, description, source, destination, transactionDate, existing.AccountId, categoryId));
    }

    public async Task<TransactionDto> CategorizeTransaction(
        Guid id,
        Guid? categoryId,
        [Service] ITransactionService service) =>
        await service.CategorizeAsync(id, categoryId);

    public async Task<bool> DeleteTransaction(
        Guid id,
        [Service] ITransactionService service)
    {
        await service.DeleteAsync(id);
        return true;
    }
}
