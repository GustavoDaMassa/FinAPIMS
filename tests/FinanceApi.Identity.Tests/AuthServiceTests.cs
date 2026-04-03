using FinanceApi.Identity.Api.Dtos;
using FinanceApi.Identity.Application.Services;
using FinanceApi.Identity.Domain.Models;
using FinanceApi.Identity.Infrastructure.Persistence;
using FinanceApi.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace FinanceApi.Identity.Tests;

public class AuthServiceTests
{
    private static IdentityDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    private static IConfiguration CreateConfiguration(string masterKey = "test-master-key")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "super-secret-key-for-testing-only-32chars",
                ["Jwt:ExpirationHours"] = "24",
                ["Jwt:Issuer"] = "financeapi-identity",
                ["Jwt:Audience"] = "financeapi",
                ["App:MasterKey"] = masterKey
            })
            .Build();
    }

    private static AuthService CreateService(IdentityDbContext db, IConfiguration? config = null)
    {
        var cfg = config ?? CreateConfiguration();
        var jwt = new JwtService(cfg);
        return new AuthService(db, jwt, cfg);
    }

    [Fact]
    public async Task Register_ShouldCreateUser_AndReturnToken()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);

        var response = await service.RegisterAsync(new RegisterRequest("Gustavo", "g@email.com", "senha123"));

        Assert.NotNull(response.Token);
        Assert.Equal("g@email.com", response.Email);
        Assert.Equal("Gustavo", response.Name);
        Assert.Equal("User", response.Role);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldThrow()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);

        await service.RegisterAsync(new RegisterRequest("Gustavo", "g@email.com", "senha123"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest("Outro", "g@email.com", "outrasenha")));
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ShouldReturnToken()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest("Gustavo", "g@email.com", "senha123"));

        var response = await service.LoginAsync(new LoginRequest("g@email.com", "senha123"));

        Assert.NotNull(response.Token);
        Assert.Equal("g@email.com", response.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrowUnauthorized()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest("Gustavo", "g@email.com", "senha123"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("g@email.com", "senha-errada")));
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ShouldThrowUnauthorized()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("naoexiste@email.com", "qualquer")));
    }

    [Fact]
    public async Task CreateAdmin_WithValidMasterKey_ShouldCreateAdminUser()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);

        var response = await service.CreateAdminAsync(
            new CreateAdminRequest("Admin", "admin@email.com", "senha123", "test-master-key"));

        Assert.Equal("Admin", response.Role);
        Assert.Equal("admin@email.com", response.Email);
    }

    [Fact]
    public async Task CreateAdmin_WithInvalidMasterKey_ShouldThrowUnauthorized()
    {
        using var db = CreateInMemoryDb();
        var service = CreateService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CreateAdminAsync(
                new CreateAdminRequest("Admin", "admin@email.com", "senha123", "chave-errada")));
    }
}
