using FinanceApi.Finance.Application.Dtos;

namespace FinanceApi.Finance.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto> FindByIdAsync(Guid id);
    Task<IReadOnlyList<CategoryDto>> ListByUserAsync(Guid userId);
    Task DeleteAsync(Guid id);
}
