using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AccountMutations
{
    public async Task<AccountDto> CreateAccount(
        string accountName,
        string institution,
        string? description,
        [Service] IAccountService service,
        [Service] IUserContext user) =>
        await service.CreateAsync(new CreateAccountRequest(accountName, institution, description, user.UserId));

    public async Task<AccountDto> UpdateAccount(
        Guid id,
        string accountName,
        string institution,
        string? description,
        [Service] IAccountService service) =>
        await service.UpdateAsync(id, new UpdateAccountRequest(accountName, institution, description));

    public async Task<bool> DeleteAccount(
        Guid id,
        [Service] IAccountService service)
    {
        await service.DeleteAsync(id);
        return true;
    }

    public async Task<AccountDto> LinkAccount(
        Guid integrationId,
        string pluggyAccountId,
        string accountName,
        string institution,
        string? description,
        [Service] IAccountService service) =>
        await service.LinkToPluggyAsync(new LinkAccountRequest(integrationId, pluggyAccountId, accountName, institution, description));
}
