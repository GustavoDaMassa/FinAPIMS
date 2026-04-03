namespace FinanceApi.Finance.Application.Exceptions;

public abstract class NotFoundException(string message) : Exception(message);

public class AccountNotFoundException(Guid id)
    : NotFoundException($"Account '{id}' not found.");

public class TransactionNotFoundException(Guid id)
    : NotFoundException($"Transaction '{id}' not found.");

public class CategoryNotFoundException(Guid id)
    : NotFoundException($"Category '{id}' not found.");

public class FinancialIntegrationNotFoundException : NotFoundException
{
    public FinancialIntegrationNotFoundException(Guid id)
        : base($"Financial integration '{id}' not found.") { }

    public FinancialIntegrationNotFoundException(string linkId)
        : base($"Financial integration with linkId '{linkId}' not found.") { }
}
