using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace FinanceApi.Gateway.Infrastructure;

// Injetado no pipeline do YARP para propagar claims do JWT
// como headers para os serviços downstream.
// finance-service lê X-User-Id e X-User-Role via IUserContext.
public class UserHeaderTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true) return;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? user.FindFirstValue("sub");

            var role = user.FindFirstValue(ClaimTypes.Role)
                    ?? user.FindFirstValue("role");

            if (userId is not null)
            {
                transformContext.ProxyRequest.Headers.Remove("X-User-Id");
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
            }

            if (role is not null)
            {
                transformContext.ProxyRequest.Headers.Remove("X-User-Role");
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Role", role);
            }

            await ValueTask.CompletedTask;
        });
    }
}
