using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class CategoryQueries
{
    public async Task<CategoryDto> Category(
        Guid id,
        [Service] ICategoryService service) =>
        await service.FindByIdAsync(id);

    public async Task<IReadOnlyList<CategoryDto>> Categories(
        [Service] ICategoryService service,
        [Service] IUserContext user) =>
        await service.ListByUserAsync(user.UserId);
}
