using FinanceApi.Identity.Api.Dtos;

namespace FinanceApi.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> CreateAdminAsync(CreateAdminRequest request);
}
