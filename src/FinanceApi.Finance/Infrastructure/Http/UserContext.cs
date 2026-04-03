namespace FinanceApi.Finance.Infrastructure.Http;

// O gateway valida o JWT e injeta X-User-Id e X-User-Role nos headers.
// O finance-service confia nesses headers sem revalidar o token.
public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers["X-User-Id"].ToString();
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var id))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Id header.");
            return id;
        }
    }

    public bool IsAdmin =>
        accessor.HttpContext?.Request.Headers["X-User-Role"].ToString() == "Admin";
}
