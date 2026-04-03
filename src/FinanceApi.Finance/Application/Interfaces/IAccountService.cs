using FinanceApi.Finance.Application.Dtos;

namespace FinanceApi.Finance.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDto> CreateAsync(CreateAccountRequest request);
    Task<AccountDto> FindByIdAsync(Guid id);
    Task<IReadOnlyList<AccountDto>> ListByUserAsync(Guid userId);
    Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request);
    Task DeleteAsync(Guid id);
    Task<AccountDto> LinkToPluggyAsync(LinkAccountRequest request);
}
