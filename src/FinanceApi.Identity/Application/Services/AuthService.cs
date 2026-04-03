using FinanceApi.Identity.Api.Dtos;
using FinanceApi.Identity.Application.Interfaces;
using FinanceApi.Identity.Domain.Enums;
using FinanceApi.Identity.Domain.Models;
using FinanceApi.Identity.Infrastructure.Persistence;
using FinanceApi.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Identity.Application.Services;

public class AuthService(IdentityDbContext db, JwtService jwt, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var emailTaken = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            throw new InvalidOperationException($"Email '{request.Email}' already in use.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, hash);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return BuildResponse(user);
    }

    public async Task<LoginResponse> CreateAdminAsync(CreateAdminRequest request)
    {
        var expectedKey = configuration["App:MasterKey"]
            ?? throw new InvalidOperationException("Master key not configured.");

        if (request.MasterKey != expectedKey)
            throw new UnauthorizedAccessException("Invalid master key.");

        var emailTaken = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            throw new InvalidOperationException($"Email '{request.Email}' already in use.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, hash, Role.Admin);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    private LoginResponse BuildResponse(User user) =>
        new(jwt.GenerateToken(user), user.Id, user.Email, user.Name, user.Role.ToString());
}
