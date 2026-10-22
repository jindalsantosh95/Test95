using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Entities.Enums;

namespace VAA.EAPI.Infrastructure.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> cache = new();
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        private static readonly Tenant[] tenant = new Tenant[]
        {
            new()
            {
                Type = IssuerType.AzureAD,
                Issuer = $"{ConfigReader.Get(Constants.Constants.AzureAdInstance)}/{ConfigReader.Get(Constants.Constants.AzureAdTenantId)}/v2.0",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Constants.Constants.AzureAdInstance)}/{ConfigReader.Get(Constants.Constants.AzureAdTenantId)}/v2.0",
                ClientId = ConfigReader.Get(Constants.Constants.AzureAdClientId)
            },
            new()
            {
                Type = IssuerType.AzureB2C,
                Issuer = $"{ConfigReader.Get(Constants.Constants.AzureAdB2CInstance)}/{ConfigReader.Get(Constants.Constants.AzureAdB2CTenantId)}/v2.0/",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Constants.Constants.AzureAdB2CInstance)}/tfp/{ConfigReader.Get(Constants.Constants.AzureAdB2CTenantId)}/{ConfigReader.Get(Constants.Constants.AzureAdB2CSignUpSignInPolicyId)}/v2.0",
                ClientId = ConfigReader.Get(Constants.Constants.AzureAdB2CClientId)
            },
            new()
            {
                Type = IssuerType.AzureAD,
                Issuer = $"{ConfigReader.Get(Constants.Constants.AzureAdTokenV1Issuer)}/{ConfigReader.Get(Constants.Constants.AzureAdTenantId)}" + "/",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Constants.Constants.AzureAdInstance)}/{ConfigReader.Get(Constants.Constants.AzureAdTenantId)}" ,
                ClientId = ConfigReader.Get(Constants.Constants.AzureAdClientId)
            }
        };


        private static ConfigurationManager<OpenIdConnectConfiguration> GetConfigurationManager(string openIdConnectConfigurationUrl)
        {
            if (cache.ContainsKey(openIdConnectConfigurationUrl)) return cache[openIdConnectConfigurationUrl];

            semaphoreSlim.Wait();

            try
            {
                if (cache.ContainsKey(openIdConnectConfigurationUrl)) return cache[openIdConnectConfigurationUrl];

                ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new($"{openIdConnectConfigurationUrl}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

                cache.AddOrUpdate(openIdConnectConfigurationUrl, configurationManager, (k, v) => configurationManager);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return cache.ContainsKey(openIdConnectConfigurationUrl) ? cache[openIdConnectConfigurationUrl] : default;
        }


        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var log = context.GetLogger("HttpFunction");

            try
            {
                // Authentication only required for HTTP requests
                if (!context.FunctionDefinition.InputBindings.Any(x => x.Value.Type.Is("httptrigger")))
                {
                    await next(context);

                    return;
                }

                var httpRequestData = await context.GetHttpRequestDataAsync();

                // Authentication not required for Swagger Definition
                if (httpRequestData.Url.AbsolutePath.EndsWith("swagger/ui", StringComparison.OrdinalIgnoreCase) || httpRequestData.Url.AbsolutePath.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);

                    return;
                }

                if (!TryGetTokenFromHeaders(context, out var token))
                {
                    await SetResponseData(context, HttpStatusCode.Unauthorized, log);

                    return;
                }


                if (!jwtSecurityTokenHandler.CanReadToken(token))
                {
                    await SetResponseData(context, HttpStatusCode.Unauthorized, log);

                    return;
                }

                var tenant = CheckValidIssuer(token);

                if (tenant.Issuer == null)
                {
                    await SetResponseData(context, HttpStatusCode.Unauthorized, log);

                    return;
                }

                var openIdConnectConfiguration = await GetConfigurationManager(tenant.OpenIdConnectConfiguration).GetConfigurationAsync(CancellationToken.None);

                TokenValidationParameters tokenValidationParameters = new()
                {
                    ValidAudience = tenant.ClientId,
                    ValidIssuer = openIdConnectConfiguration.Issuer,
                    IssuerSigningKeys = openIdConnectConfiguration.SigningKeys,
                };

                var principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out _);

                if (principal != null)
                {
                    // Add principal + token into Features collection to be made available to the Authorization middleware
                    context.Features.Set(new JwtPrincipalFeature(principal, token));

                    UnpackClaimsandAttachtoContext(context, principal);

                    await next(context);
                }
            }
            catch (SecurityTokenException exception)
            {
                log.LogInformation("AuthenticationMiddleware-Exception" + exception.Message);

                await SetResponseData(context, HttpStatusCode.Unauthorized, log);
            }
        }

        private bool TryGetTokenFromHeaders(FunctionContext context, out string token)
        {
            token = null;

            var value = context.GetHeader("authorization");

            if (value.IsNullOrWhiteSpace()) return false;

            if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;

            token = value["Bearer ".Length..].Trim();

            return true;
        }

        private Tenant CheckValidIssuer(string token)
        {
            var securityToken = new JwtSecurityTokenHandler().ReadToken(token);

            return tenant.FirstOrDefault(x => x.Issuer.Is(securityToken.Issuer)) ?? new Tenant();
        }

        private void UnpackClaimsandAttachtoContext(FunctionContext context, ClaimsPrincipal principal)
        {
            context.Items.Add("CustomerID", principal.Claims.FirstOrDefault(c => c.Type == "extension_customerId")?.Value);
            context.Items.Add("MemberStatus", principal.Claims.FirstOrDefault(c => c.Type == "extension_MemberStatus")?.Value);
            context.Items.Add("ClaimsPrincipal", principal);
        }

        private async Task SetResponseData(FunctionContext context, HttpStatusCode statusCode, ILogger logger)
        {
            var response = (await context.GetHttpRequestDataAsync()).CreateResponse();

            response.StatusCode = statusCode;

            await context.InvokeResult(response, logger);
        }
    }
}
