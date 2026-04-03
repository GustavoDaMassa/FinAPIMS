using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Domain.Models;

namespace FinanceApi.Finance.Tests;

public class FinancialIntegrationServiceTests
{
    private static IFinancialIntegrationService CreateService(out FinanceDbContext db)
    {
        db = TestHelpers.CreateDb();
        return new FinancialIntegrationService(db);
    }

    [Fact]
    public async Task Create_ShouldPersistIntegrationAndReturnDto()
    {
        var service = CreateService(out var db);
        var request = new CreateFinancialIntegrationRequest(AggregatorType.Pluggy, "link-abc", TestHelpers.UserId);

        var result = await service.CreateAsync(request);

        Assert.Equal("link-abc", result.LinkId);
        Assert.Equal(AggregatorType.Pluggy, result.Aggregator);
        Assert.Equal(TestHelpers.UserId, result.UserId);
        Assert.Equal("UPDATED", result.Status);
        Assert.NotNull(result.ExpiresAt);
        Assert.Equal(1, await db.FinancialIntegrations.CountAsync());
    }

    [Fact]
    public async Task FindById_WhenExists_ShouldReturnDto()
    {
        var service = CreateService(out var db);
        var integration = FinancialIntegration.Create(AggregatorType.Pluggy, "link-xyz", TestHelpers.UserId);
        db.FinancialIntegrations.Add(integration);
        await db.SaveChangesAsync();

        var result = await service.FindByIdAsync(integration.Id);

        Assert.Equal(integration.Id, result.Id);
        Assert.Equal("link-xyz", result.LinkId);
    }

    [Fact]
    public async Task FindById_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<FinancialIntegrationNotFoundException>(() =>
            service.FindByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindByLinkId_WhenExists_ShouldReturnDto()
    {
        var service = CreateService(out var db);
        var integration = FinancialIntegration.Create(AggregatorType.Pluggy, "link-pluggy-1", TestHelpers.UserId);
        db.FinancialIntegrations.Add(integration);
        await db.SaveChangesAsync();

        var result = await service.FindByLinkIdAsync("link-pluggy-1");

        Assert.Equal(integration.Id, result.Id);
    }

    [Fact]
    public async Task FindByLinkId_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<FinancialIntegrationNotFoundException>(() =>
            service.FindByLinkIdAsync("inexistente"));
    }

    [Fact]
    public async Task ListByUser_ShouldReturnOnlyUserIntegrations()
    {
        var service = CreateService(out var db);
        db.FinancialIntegrations.Add(FinancialIntegration.Create(AggregatorType.Pluggy, "link-1", TestHelpers.UserId));
        db.FinancialIntegrations.Add(FinancialIntegration.Create(AggregatorType.Pluggy, "link-2", TestHelpers.UserId));
        db.FinancialIntegrations.Add(FinancialIntegration.Create(AggregatorType.Pluggy, "link-3", TestHelpers.OtherUserId));
        await db.SaveChangesAsync();

        var result = await service.ListByUserAsync(TestHelpers.UserId);

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(TestHelpers.UserId, i.UserId));
    }

    [Fact]
    public async Task Delete_ShouldRemoveIntegration()
    {
        var service = CreateService(out var db);
        var integration = FinancialIntegration.Create(AggregatorType.Pluggy, "link-del", TestHelpers.UserId);
        db.FinancialIntegrations.Add(integration);
        await db.SaveChangesAsync();

        await service.DeleteAsync(integration.Id);

        Assert.Equal(0, await db.FinancialIntegrations.CountAsync());
    }
}
