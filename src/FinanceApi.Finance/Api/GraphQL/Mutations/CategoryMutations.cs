using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Http;
using HotChocolate;
using HotChocolate.Types;

namespace FinanceApi.Finance.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class CategoryMutations
{
    public async Task<CategoryDto> CreateCategory(
        string name,
        [Service] ICategoryService service,
        [Service] IUserContext user) =>
        await service.CreateAsync(new CreateCategoryRequest(name, user.UserId));

    public async Task<bool> DeleteCategory(
        Guid id,
        [Service] ICategoryService service)
    {
        await service.DeleteAsync(id);
        return true;
    }
}
