using FinanceApi.Finance.Application.Exceptions;
using HotChocolate;

namespace FinanceApi.Finance.Api.GraphQL.Errors;

public class FinanceErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        return error.Exception switch
        {
            NotFoundException ex => error
                .WithMessage(ex.Message)
                .WithCode("NOT_FOUND")
                .RemoveException(),

            InvalidOperationException ex => error
                .WithMessage(ex.Message)
                .WithCode("CONFLICT")
                .RemoveException(),

            UnauthorizedAccessException ex => error
                .WithMessage(ex.Message)
                .WithCode("UNAUTHORIZED")
                .RemoveException(),

            _ => error
        };
    }
}
