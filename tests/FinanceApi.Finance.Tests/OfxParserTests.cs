using FinanceApi.Finance.Domain.Enums;
using FinanceApi.Finance.Infrastructure.Ofx;

namespace FinanceApi.Finance.Tests;

public class OfxParserTests
{
    private static readonly OfxParser Parser = new();

    private const string V1Content = """
        OFXHEADER:100
        DATA:OFXSGML
        VERSION:102
        SECURITY:NONE
        ENCODING:UTF-8
        CHARSET:1252
        COMPRESSION:NONE
        OLDFILEUID:NONE
        NEWFILEUID:NONE

        <OFX>
        <BANKMSGSRSV1>
        <STMTTRNRS>
        <STMTRS>
        <BANKTRANLIST>
        <STMTTRN>
        <TRNTYPE>DEBIT
        <DTPOSTED>20240115120000[-3:BRT]
        <TRNAMT>-150.00
        <FITID>TX001
        <MEMO>PAGAMENTO CARTAO
        </STMTTRN>
        <STMTTRN>
        <TRNTYPE>CREDIT
        <DTPOSTED>20240120
        <TRNAMT>3000.00
        <FITID>TX002
        <MEMO>SALARIO
        </STMTTRN>
        </BANKTRANLIST>
        </STMTRS>
        </STMTTRNRS>
        </BANKMSGSRSV1>
        </OFX>
        """;

    private const string V2Content = """
        <?OFX OFXHEADER="200" VERSION="220" SECURITY="NONE" OLDFILEUID="NONE" NEWFILEUID="NONE"?>
        <OFX>
          <BANKMSGSRSV1>
            <STMTTRNRS>
              <STMTRS>
                <BANKTRANLIST>
                  <STMTTRN>
                    <TRNTYPE>DEBIT</TRNTYPE>
                    <DTPOSTED>20240215</DTPOSTED>
                    <TRNAMT>-75.50</TRNAMT>
                    <FITID>TX003</FITID>
                    <MEMO>COMPRA MERCADO</MEMO>
                  </STMTTRN>
                  <STMTTRN>
                    <TRNTYPE>CREDIT</TRNTYPE>
                    <DTPOSTED>20240301</DTPOSTED>
                    <TRNAMT>500.00</TRNAMT>
                    <FITID>TX004</FITID>
                    <NAME>TRANSFERENCIA RECEBIDA</NAME>
                  </STMTTRN>
                </BANKTRANLIST>
              </STMTRS>
            </STMTTRNRS>
          </BANKMSGSRSV1>
        </OFX>
        """;

    [Fact]
    public void ParseV1_ShouldExtractAllTransactions()
    {
        var rows = Parser.Parse(V1Content);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void ParseV1_NegativeAmount_ShouldBeOutflow()
    {
        var rows = Parser.Parse(V1Content);

        var debit = rows.First(r => r.FitId == "TX001");
        Assert.Equal(TransactionType.Outflow, debit.Type);
        Assert.Equal(150.00m, debit.Amount);
        Assert.Equal(new DateOnly(2024, 1, 15), debit.Date);
        Assert.Equal("PAGAMENTO CARTAO", debit.Description);
    }

    [Fact]
    public void ParseV1_PositiveAmount_ShouldBeInflow()
    {
        var rows = Parser.Parse(V1Content);

        var credit = rows.First(r => r.FitId == "TX002");
        Assert.Equal(TransactionType.Inflow, credit.Type);
        Assert.Equal(3000.00m, credit.Amount);
        Assert.Equal(new DateOnly(2024, 1, 20), credit.Date);
    }

    [Fact]
    public void ParseV2_ShouldExtractAllTransactions()
    {
        var rows = Parser.Parse(V2Content);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void ParseV2_ShouldReadMemoAndName()
    {
        var rows = Parser.Parse(V2Content);

        Assert.Equal("COMPRA MERCADO", rows.First(r => r.FitId == "TX003").Description);
        Assert.Equal("TRANSFERENCIA RECEBIDA", rows.First(r => r.FitId == "TX004").Description);
    }

    [Fact]
    public void Parse_EmptyContent_ShouldReturnEmptyList()
    {
        var rows = Parser.Parse(string.Empty);

        Assert.Empty(rows);
    }
}
