using FinanceApi.Finance.Application.Dtos;

namespace FinanceApi.Finance.Application.Interfaces;

public interface IOfxImportService
{
    Task<OfxImportResult> ImportAsync(Stream ofxStream, Guid accountId);
}
