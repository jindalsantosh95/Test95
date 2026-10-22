using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Entities;

namespace VAA.EAPI.Infrastructure.Middleware
{
    public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(
            FunctionContext context,
            FunctionExecutionDelegate next)
        {
            if (context.FunctionDefinition.InputBindings.First().Value.Type.Is("httptrigger"))
            {
                // If running in Visual Studio, bypass authorization
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    // Authentication not required for Swagger Defnition
                    var httpRequestData = await context.GetHttpRequestDataAsync();

                    if (httpRequestData.Url.AbsolutePath.EndsWith("swagger/ui", StringComparison.OrdinalIgnoreCase) || httpRequestData.Url.AbsolutePath.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
                    {
                        await next(context);

                        return;
                    }

                    var principalFeature = context.Features.Get<JwtPrincipalFeature>();

                    if (!AuthorizeApplicationPermissions(context, principalFeature.Principal))
                    {
                        context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);

                        return;
                    }
                }
            }
            await next(context);
        }

        private static bool AuthorizeApplicationPermissions(FunctionContext context, ClaimsPrincipal principal)
        {
            var targetMethod = context.GetTargetFunctionMethod();
            var acceptedAppRoles = GetAcceptedAppRoles(targetMethod);

            // If there are no roles defined, then assumption is method being called is Anonymous / Open access
            if (!acceptedAppRoles.Any())
                return true;

            // Check to see if request as the require claim.
            return principal.FindAll(ClaimTypes.Role).Any(ur => acceptedAppRoles.Contains(ur.Value));
        }

        private static IEnumerable<string> GetAcceptedAppRoles(MethodInfo targetMethod)
        {
            var attributes = GetCustomAttributesOnClassAndMethod<RoleAuthorizationAttribute>(targetMethod);

            return (attributes?.FirstOrDefault()?.Roles) ?? Enumerable.Empty<string>();
        }

        private static IEnumerable<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod) where T : Attribute
        {
            var methodAttributes = targetMethod.GetCustomAttributes<T>();
            var classAttributes = targetMethod.DeclaringType.GetCustomAttributes<T>();

            return methodAttributes.Concat(classAttributes);
        }
    }
}