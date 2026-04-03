namespace FinanceApi.Identity.Api.Dtos;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record CreateAdminRequest(string Name, string Email, string Password, string MasterKey);

public record LoginResponse(string Token, Guid UserId, string Email, string Name, string Role);
