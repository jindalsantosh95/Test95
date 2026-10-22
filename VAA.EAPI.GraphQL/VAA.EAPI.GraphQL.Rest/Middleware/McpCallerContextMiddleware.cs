using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Rest.FunctionHost;

namespace VAA.EAPI.GraphQL.Rest.Middleware
{
    // The existing marker/feature names are retained for loaded-domain compatibility.
    // AuthenticationMiddleware validates the bearer signature and lifetime first.
    public sealed class McpCallerContextMiddleware(Router router) : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var request = await context.GetHttpRequestDataAsync();
            var isMcp = request is not null && request.Method.Is("POST") &&
                        (request.Headers.TryGetValues("x-mcp-request", out _) ||
                         request.Url.AbsolutePath.StartsWith("/api/mcp", StringComparison.OrdinalIgnoreCase));
            var isMarkedRestRoute = request is not null && router.Routes.Any(group =>
                group.Key.Method.Is(request.Method) &&
                group.Value.Any(route => route.RequiresMcpCallerContext &&
                    request.Url.AbsolutePath.TrimEnd('/').Is("/api/v1/" + route.Name)));
            if (isMcp || isMarkedRestRoute)
            {
                var feature = context.Features.Get<JwtPrincipalFeature>();
                if (IsTrustedApplication(context.GetHeader("Authorization"), feature))
                    context.Features.Set(new McpCallerContextFeature(feature.Principal));
            }

            // The router/tool gates only opted-in operations. Legacy calls are unchanged.
            await next(context);
        }

        private static bool IsTrustedApplication(string authorization, JwtPrincipalFeature feature)
        {
            if (feature?.Principal?.Identity?.IsAuthenticated != true || authorization.IsNullOrWhiteSpace()) return false;
            // Match the primary token using the shared authentication header convention.
            // A secondary broker token remains available to its owning domain unchanged.
            var primary = authorization.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (primary?.Length != 2 || !primary[0].Is("Bearer") ||
                !string.Equals(primary[1], feature.AccessToken?.Token, StringComparison.Ordinal)) return false;

            var configuredApp = ConfigReader.Try(Common.Constants.Environment.McpCallerContextApplicationId);
            var appId = feature.Principal.FindFirst("azp")?.Value ?? feature.Principal.FindFirst("appid")?.Value;
            if (configuredApp.IsNullOrWhiteSpace() || appId.IsNullOrWhiteSpace() || !appId.Is(configuredApp)) return false;

            // Authorize only signature-validated claims, never re-read the raw token.
            // Shared authentication can select a principal from either supported token.
            var principal = feature.Principal;
            var issuer = principal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value;
            var tokenAudiences = principal.FindAll(JwtRegisteredClaimNames.Aud).Select(claim => claim.Value).ToArray();
            var tenant = ConfigReader.Try(Common.Constants.Environment.AADTenant);
            var v1 = $"{ConfigReader.Try(Common.Constants.Environment.AADTokenV1Issuer)?.TrimEnd('/')}/{tenant}/";
            var v2 = $"{ConfigReader.Try(Common.Constants.Environment.AADAuthority)?.TrimEnd('/')}/{tenant}/v2.0";
            var audiences = ConfigReader.Try(Common.Constants.Environment.EAPIGraphAppID, "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            return !tenant.IsNullOrWhiteSpace() && (issuer == v1 || issuer == v2) &&
                tokenAudiences.Length == 1 && audiences.Contains(tokenAudiences[0], StringComparer.Ordinal) &&
                !principal.HasClaim(claim => claim.Type is "scp" or "http://schemas.microsoft.com/identity/claims/scope");
        }
    }

    public sealed class McpCallerContextFeature(ClaimsPrincipal principal)
    {
        public ClaimsPrincipal Principal { get; } = principal ?? throw new ArgumentNullException(nameof(principal));
    }
}
