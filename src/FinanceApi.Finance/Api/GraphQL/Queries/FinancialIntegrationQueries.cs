using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class FinancialIntegrationQueries
{
    public async Task<FinancialIntegrationDto> FinancialIntegration(
        Guid id,
        [Service] IFinancialIntegrationService service) =>
        await service.FindByIdAsync(id);

    public async Task<IReadOnlyList<FinancialIntegrationDto>> FinancialIntegrations(
        [Service] IFinancialIntegrationService service,
        [Service] IUserContext user) =>
        await service.ListByUserAsync(user.UserId);
}
