using FinanceApi.Finance.Application.Dtos;

namespace FinanceApi.Finance.Application.Interfaces;

public interface IFinancialIntegrationService
{
    Task<FinancialIntegrationDto> CreateAsync(CreateFinancialIntegrationRequest request);
    Task<FinancialIntegrationDto> FindByIdAsync(Guid id);
    Task<FinancialIntegrationDto> FindByLinkIdAsync(string linkId);
    Task<IReadOnlyList<FinancialIntegrationDto>> ListByUserAsync(Guid userId);
    Task DeleteAsync(Guid id);
}
