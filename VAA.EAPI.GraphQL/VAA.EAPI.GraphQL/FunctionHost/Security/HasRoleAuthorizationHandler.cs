using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Middleware;

namespace VAA.EAPI.GraphQL.FunctionHost.Security
{
    public sealed class HasRoleAuthorizationHandler : IAuthorizationHandler
    {
        public ValueTask<AuthorizeResult> AuthorizeAsync(IMiddlewareContext context, AuthorizeDirective directive)
        {
            if (directive is null) return ValueTask.FromResult(AuthorizeResult.NotAllowed);

            var feature = AsyncContext.Current?.Context?.Features?.FirstOrDefault(x => x.Key == typeof(JwtPrincipalFeature));
            var jwtPrincipalFeature = feature?.Value as JwtPrincipalFeature;
            var principal = jwtPrincipalFeature?.Principal;

            if (directive.Roles.IsNullOrEmpty())
            {
                bool isAuthenticated = principal?.Identity?.IsAuthenticated ?? false;
                return ValueTask.FromResult(isAuthenticated ? AuthorizeResult.Allowed : AuthorizeResult.NotAllowed);
            }

            var requiredRoles = directive.Roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool hasAcceptableRole = principal?.FindAll(ClaimTypes.Role).Any(userRole => requiredRoles.Contains(userRole.Value)) ?? false;

            return ValueTask.FromResult(hasAcceptableRole ? AuthorizeResult.Allowed : AuthorizeResult.NotAllowed);
        }
    }
}
