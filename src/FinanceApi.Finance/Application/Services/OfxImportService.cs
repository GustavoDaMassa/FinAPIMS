using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Infrastructure.Ofx;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Application.Services;

public class OfxImportService(
    ITransactionService transactionService,
    FinanceDbContext db,
    OfxParser parser) : IOfxImportService
{
    public async Task<OfxImportResult> ImportAsync(Stream ofxStream, Guid accountId)
    {
        var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId);
        if (!accountExists)
            throw new AccountNotFoundException(accountId);

        using var reader = new StreamReader(ofxStream);
        var content = await reader.ReadToEndAsync();
        var rows = parser.Parse(content);

        var imported = new List<TransactionDto>();
        var skipped = 0;

        foreach (var row in rows)
        {
            if (await transactionService.ExistsByExternalIdAsync(row.FitId))
            {
                skipped++;
                continue;
            }

            var dto = await transactionService.CreateAsync(new CreateTransactionRequest(
                row.Amount, row.Type, row.Description, null, null,
                row.Date, accountId, null, row.FitId));

            imported.Add(dto);
        }

        return new OfxImportResult(imported.Count, skipped, imported);
    }
}
