using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Application.Services;

public class FinancialIntegrationService(FinanceDbContext db) : IFinancialIntegrationService
{
    public async Task<FinancialIntegrationDto> CreateAsync(CreateFinancialIntegrationRequest request)
    {
        var integration = FinancialIntegration.Create(request.Aggregator, request.LinkId, request.UserId);
        db.FinancialIntegrations.Add(integration);
        await db.SaveChangesAsync();
        return ToDto(integration);
    }

    public async Task<FinancialIntegrationDto> FindByIdAsync(Guid id)
    {
        var integration = await db.FinancialIntegrations.FindAsync(id)
            ?? throw new FinancialIntegrationNotFoundException(id);
        return ToDto(integration);
    }

    public async Task<FinancialIntegrationDto> FindByLinkIdAsync(string linkId)
    {
        var integration = await db.FinancialIntegrations.SingleOrDefaultAsync(f => f.LinkId == linkId)
            ?? throw new FinancialIntegrationNotFoundException(linkId);
        return ToDto(integration);
    }

    public async Task<IReadOnlyList<FinancialIntegrationDto>> ListByUserAsync(Guid userId)
    {
        var integrations = await db.FinancialIntegrations
            .Where(f => f.UserId == userId)
            .ToListAsync();
        return integrations.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        var integration = await db.FinancialIntegrations.FindAsync(id)
            ?? throw new FinancialIntegrationNotFoundException(id);
        db.FinancialIntegrations.Remove(integration);
        await db.SaveChangesAsync();
    }

    private static FinancialIntegrationDto ToDto(FinancialIntegration f) =>
        new(f.Id, f.Aggregator, f.LinkId, f.Status, f.CreatedAt, f.ExpiresAt, f.UserId);
}
