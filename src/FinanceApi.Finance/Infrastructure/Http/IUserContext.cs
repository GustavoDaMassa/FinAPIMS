namespace FinanceApi.Finance.Infrastructure.Http;

public interface IUserContext
{
    Guid UserId { get; }
    bool IsAdmin { get; }
}
