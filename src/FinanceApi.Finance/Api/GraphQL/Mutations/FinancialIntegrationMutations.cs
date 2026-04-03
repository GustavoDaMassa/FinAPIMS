using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class FinancialIntegrationMutations
{
    public async Task<FinancialIntegrationDto> CreateFinancialIntegration(
        string linkId,
        AggregatorType aggregator,
        [Service] IFinancialIntegrationService service,
        [Service] IUserContext user) =>
        await service.CreateAsync(new CreateFinancialIntegrationRequest(aggregator, linkId, user.UserId));

    public async Task<bool> DeleteFinancialIntegration(
        Guid id,
        [Service] IFinancialIntegrationService service)
    {
        await service.DeleteAsync(id);
        return true;
    }
}
