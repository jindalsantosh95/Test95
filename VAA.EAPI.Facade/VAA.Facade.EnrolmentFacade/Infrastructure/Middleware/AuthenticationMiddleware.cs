using VAA.Facade.EnrolmentFacade.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Helper;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities.Enums;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var log = context.GetLogger("HttpFunction");
            PathReader.SetFunctionAppDirRootPath();
            try
            {
                if (!context.FunctionDefinition.EntryPoint.Contains("Triggers.Http"))
                {
                    await next(context);
                    return;


                }

                if (!TryGetTokenFromHeaders(context, out var token))
                {
                    await SetResponseData(context, HttpStatusCode.Unauthorized);
                    return;
                }

                JwtSecurityTokenHandler tokenValidator = new JwtSecurityTokenHandler();
                if (!tokenValidator.CanReadToken(token))
                {
                    await SetResponseData(context, HttpStatusCode.Unauthorized);
                    return;
                }

                var tenant = CheckValidIssuer(token);
                if (tenant.Issuer == null)
                {
                    SetResponseData(context, HttpStatusCode.Unauthorized);
                    return;
                }

                ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{tenant.OpenIdConnectConfiguration}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

                var openIdConfig = await configurationManager.GetConfigurationAsync(default);
                TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = tenant.ClientId,
                    ValidIssuer = openIdConfig.Issuer,
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                };

                var principal = tokenValidator.ValidateToken(token, tokenValidationParameters, out _);
                if (principal != null)
                {
                    UnpackClaimsandAttachtoContext(context, principal);
                    await next(context);
                }
            }
            catch (SecurityTokenException exception)
            {
                log.LogInformation("AuthenticationMiddleware-Exception" + exception.Message);
                await SetResponseData(context, HttpStatusCode.Unauthorized);
            }
        }

        private void UnpackClaimsandAttachtoContext(FunctionContext context, ClaimsPrincipal principal)
        {
            context.Items.Add("CustomerID", principal.Claims.FirstOrDefault(c => c.Type == "extension_customerId")?.Value);
        }

        private static bool TryGetTokenFromHeaders(FunctionContext context, out string token)
        {
            token = null;
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            {
                return false;
            }
            if (headersObj is not string headersStr)
            {
                return false;
            }
            var headers = Json.DeserializeObject<Dictionary<string, string>>(headersStr);
            var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            {
                return false;
            }
            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            token = authHeaderValue.Substring("Bearer ".Length).Trim();
            return true;
        }

        private Tenant CheckValidIssuer(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            List<Tenant> tenant = new List<Tenant>
            {
            new() { Type = IssuerType.AzureAD, Issuer = $"{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdInstance)}/{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdTenantId)}/v2.0", OpenIdConnectConfiguration = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdInstance) + "/" + Environment.GetEnvironmentVariable(Constants.Constants.AzureAdTenantId) + "/v2.0", ClientId = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdClientId) },
            new() { Type = IssuerType.AzureB2C, Issuer = $"{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CInstance)}/{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CTenantId)}/v2.0/", OpenIdConnectConfiguration = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CInstance) + "/tfp/" + Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CTenantId) + "/" + Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CSignUpSignInPolicyId) + "/v2.0", ClientId = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdB2CClientId) },
            new() { Type = IssuerType.AzureAD, Issuer = $"{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdTokenV1Issuer)}/{Environment.GetEnvironmentVariable(Constants.Constants.AzureAdTenantId)}" + "/", OpenIdConnectConfiguration = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdInstance) + "/" + Environment.GetEnvironmentVariable(Constants.Constants.AzureAdTenantId) , ClientId = Environment.GetEnvironmentVariable(Constants.Constants.AzureAdClientId) },

            };

            var tokenIssuer = tenant.Where(x => string.Equals(x.Issuer, tokenS.Issuer, StringComparison.CurrentCultureIgnoreCase));
            if (tokenIssuer.Any())
            {
                return tokenIssuer.FirstOrDefault();
            }
            return new Tenant();
        }

        private async Task SetResponseData(FunctionContext context, HttpStatusCode statusCode)
        {
            var req = context.GetHttpRequestData();
            var response = req.CreateResponse();
            response.StatusCode = statusCode;
            await context.InvokeResult(response);
        }

    }
}
