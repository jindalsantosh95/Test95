using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    // Opt in only the new account-linking surface; legacy Identity functions keep their existing pipeline.
    public sealed class AccountLinkingAuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var entryPoint = context.FunctionDefinition.EntryPoint;
            var controller = typeof(AccountLinkingController);
            var prefix = controller.FullName + ".";
            MethodInfo method = null;
            if (entryPoint.StartsWith(prefix, StringComparison.Ordinal))
                method = controller.GetMethod(entryPoint.Substring(prefix.Length));
            else
            {
                await next(context);
                return;
            }

            var principal = context.Features.Get<JwtPrincipalFeature>()?.Principal;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }
            var roles = method?.GetCustomAttribute<RoleAuthorizationAttribute>()?.Roles;
            if (roles is null || !principal.FindAll(ClaimTypes.Role).Any(claim => roles.Contains(claim.Value, StringComparer.Ordinal)))
            {
                context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                return;
            }
            await next(context);
        }
    }
}
