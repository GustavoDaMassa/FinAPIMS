using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class AccountQueries
{
    public async Task<AccountDto> Account(
        Guid id,
        [Service] IAccountService service) =>
        await service.FindByIdAsync(id);

    public async Task<IReadOnlyList<AccountDto>> Accounts(
        [Service] IAccountService service,
        [Service] IUserContext user) =>
        await service.ListByUserAsync(user.UserId);
}
