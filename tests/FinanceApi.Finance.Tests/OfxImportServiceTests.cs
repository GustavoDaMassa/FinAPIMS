using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Ofx;

namespace FinanceApi.Finance.Tests;

public class OfxImportServiceTests
{
    private static (IOfxImportService service, FinanceDbContext db) CreateService()
    {
        var db = TestHelpers.CreateDb();
        var parser = new OfxParser();
        var transactionService = new TransactionService(db);
        return (new OfxImportService(transactionService, db, parser), db);
    }

    private static Account SeedAccount(FinanceDbContext db)
    {
        var account = Account.Create("Conta", "Banco", null, TestHelpers.UserId);
        db.Accounts.Add(account);
        db.SaveChanges();
        return account;
    }

    private static Stream ToStream(string content) =>
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

    private const string TwoTransactionsOfx = """
        <OFX>
        <BANKMSGSRSV1><STMTTRNRS><STMTRS><BANKTRANLIST>
        <STMTTRN>
        <TRNTYPE>CREDIT
        <DTPOSTED>20240101
        <TRNAMT>1000.00
        <FITID>EXT001
        <MEMO>SALARIO
        </STMTTRN>
        <STMTTRN>
        <TRNTYPE>DEBIT
        <DTPOSTED>20240102
        <TRNAMT>-200.00
        <FITID>EXT002
        <MEMO>MERCADO
        </STMTTRN>
        </BANKTRANLIST></STMTRS></STMTTRNRS></BANKMSGSRSV1>
        </OFX>
        """;

    [Fact]
    public async Task Import_ShouldCreateTransactionsAndReturnResult()
    {
        var (service, db) = CreateService();
        var account = SeedAccount(db);

        var result = await service.ImportAsync(ToStream(TwoTransactionsOfx), account.Id);

        Assert.Equal(2, result.Imported);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(2, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Import_ShouldPreserveExternalId()
    {
        var (service, db) = CreateService();
        var account = SeedAccount(db);

        var result = await service.ImportAsync(ToStream(TwoTransactionsOfx), account.Id);

        Assert.Contains(result.Transactions, t => t.ExternalId == "EXT001");
        Assert.Contains(result.Transactions, t => t.ExternalId == "EXT002");
    }

    [Fact]
    public async Task Import_WhenExternalIdAlreadyExists_ShouldSkip()
    {
        var (service, db) = CreateService();
        var account = SeedAccount(db);
        await service.ImportAsync(ToStream(TwoTransactionsOfx), account.Id);

        // Second import with same file
        var result = await service.ImportAsync(ToStream(TwoTransactionsOfx), account.Id);

        Assert.Equal(0, result.Imported);
        Assert.Equal(2, result.Skipped);
        Assert.Equal(2, await db.Transactions.CountAsync()); // still 2, not 4
    }

    [Fact]
    public async Task Import_WhenAccountNotFound_ShouldThrow()
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.ImportAsync(ToStream(TwoTransactionsOfx), Guid.NewGuid()));
    }
}
