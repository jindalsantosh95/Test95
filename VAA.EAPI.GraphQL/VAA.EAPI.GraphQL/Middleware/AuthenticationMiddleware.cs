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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Middleware
{
    public sealed class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {

        private static readonly dynamic error = new { Errors = new[] { new { Message = "Authorization token is missing/invalid", Extensions = new { Code = "401" } } } };
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> cache = new();
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        private static readonly Tenant[] tenants =
        [
            new()
            {
                Type = IssuerType.AzureAD,
                Issuer = $"{ConfigReader.Get(Common.Constants.Environment.AADAuthority)}/{ConfigReader.Get(Common.Constants.Environment.AADTenant)}/v2.0",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Common.Constants.Environment.AADAuthority)}/{ConfigReader.Get(Common.Constants.Environment.AADTenant)}/v2.0",
                ClientId =  [ConfigReader.Get(Common.Constants.Environment.EAPIGraphAppID)]
            },
            new ()
            {
                Type = IssuerType.AzureAD,
                Issuer = $"{ConfigReader.Get(Common.Constants.Environment.AADTokenV1Issuer)}/{ConfigReader.Get(Common.Constants.Environment.AADTenant)}/",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Common.Constants.Environment.AADAuthority)}/{ConfigReader.Get(Common.Constants.Environment.AADTenant)}",
                ClientId =  [ConfigReader.Get(Common.Constants.Environment.EAPIGraphAppID)]
            },
            new()
            {
                Type = IssuerType.Identity,
                Issuer = ConfigReader.Get(Common.Constants.Environment.IdentityIssuer)
            },
            new()
            {
                Type = IssuerType.FlyingClub,
                Issuer = ConfigReader.Try(Common.Constants.Environment.FlyingClubTokenIssuer)
            },
            new()
            {
                Type = IssuerType.AzureB2C,
                Issuer = $"{ConfigReader.Get(Common.Constants.Environment.AB2CAuthority)}/{ConfigReader.Get(Common.Constants.Environment.AB2CTenant)}/v2.0/",
                OpenIdConnectConfiguration = $"{ConfigReader.Get(Common.Constants.Environment.AB2CAuthority)}/{ConfigReader.Get(Common.Constants.Environment.AB2COpenIdConnection)}/v2.0",
                ClientId = ConfigReader.Get(Common.Constants.Environment.AB2CAudience).Split(Application.CommaDelimiter, StringSplitOptions.RemoveEmptyEntries)
            },
            new()
            {
                Type = IssuerType.Entra,
                Issuer = $"https://{ConfigReader.Get(Common.Constants.Environment.EntraTenant)}.ciamlogin.com/{ConfigReader.Get(Common.Constants.Environment.EntraTenant)}/v2.0",
                OpenIdConnectConfiguration = $"https://{ConfigReader.Get(Common.Constants.Environment.EntraTenant)}.ciamlogin.com/{ConfigReader.Get(Common.Constants.Environment.EntraTenant)}/v2.0",
                ClientId = ConfigReader.Get(Common.Constants.Environment.AB2CAudience).Split(Application.CommaDelimiter, StringSplitOptions.RemoveEmptyEntries)
            }
        ];

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var log = context.GetLogger("HttpFunction");

            try
            {
                // Some Graph Endpoints are public open access (e.g. CJS) so we don't authenticate / validate every request
                // We only attempt to authenticate if an authorization header is present. If it's not present, we allow the
                // request to proceed unauthenticated and let downstream code handle any authorization decisions based on
                // the absence of a principal.
                if (TryGetTokenFromHeaders(context, out var authToken))
                    await ValidateAndSetTokenPrincipal(context, authToken, log);
            }
            catch (SecurityTokenExpiredException ex)
            {
                context.SetHttpResponseData(HttpStatusCode.Unauthorized, (string)Json.Serialize(error));

                return;
            }
            catch (SecurityTokenValidationException ex)
            {
                context.SetHttpResponseData(HttpStatusCode.Unauthorized, (string)Json.Serialize(error));

                return;
            }
            catch (ArgumentException ex)
            {
                context.SetHttpResponseData(HttpStatusCode.Unauthorized, (string)Json.Serialize(error));

                return;
            }
            catch (Exception exception)
            {
                log.LogInformation("AuthenticationMiddleware-Exception " + exception.Message);

                context.SetHttpResponseData(HttpStatusCode.Unauthorized, (string)Json.Serialize(error));

                return;
            }

            await next(context);
        }

        private static async Task<ConfigurationManager<OpenIdConnectConfiguration>> GetConfigurationManager(string openIdConnectConfigurationUrl)
        {
            if (cache.TryGetValue(openIdConnectConfigurationUrl, out var configManager))
                return configManager;

            await semaphoreSlim.WaitAsync();

            try
            {
                if (cache.TryGetValue(openIdConnectConfigurationUrl, out configManager))
                    return configManager;

                ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new($"{openIdConnectConfigurationUrl}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

                cache.AddOrUpdate(openIdConnectConfigurationUrl, configurationManager, (_, _) => configurationManager);

                return configurationManager;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private static async Task ValidateAndSetTokenPrincipal(FunctionContext context, AuthToken token, ILogger log)
        {
            if (!string.IsNullOrEmpty(token.Token) && jwtSecurityTokenHandler.CanReadToken(token.Token))
                await ValidateTokenAndSetPrincipal(context, token.Token, true, token, log);

            if (!string.IsNullOrEmpty(token.OtherToken) && jwtSecurityTokenHandler.CanReadToken(token.OtherToken))
            {
                bool setPrincipal = context.Features.Get<JwtPrincipalFeature>() == null;

                await ValidateTokenAndSetPrincipal(context, token.OtherToken, setPrincipal, token, log);
            }
        }

        private static async Task ValidateTokenAndSetPrincipal(FunctionContext context, string tokenString, bool setPrincipal, AuthToken originalToken, ILogger log)
        {
            try
            {
                var tenant = CheckValidIssuer(tokenString, out var clientId);

                if (tenant.Issuer == null)
                    throw new SecurityTokenValidationException("Invalid issuer for token");

                // The Account query delegates authoritative broker-token validation to Loyalty Identity,
                // which owns the Key Vault signing boundary. Do not trust claims at this stage.
                if (tenant.Type == IssuerType.FlyingClub)
                {
                    context.Features.Set(new FlyingClubTokenFeature(tokenString));
                    return;
                }

                var tokenValidationParameters = await CreateTokenValidationParameters(tenant, clientId);
                var principal = jwtSecurityTokenHandler.ValidateToken(tokenString, tokenValidationParameters, out _);

                if (setPrincipal)
                {
                    var validatedAccessToken = string.Equals(originalToken.Token, tokenString, StringComparison.Ordinal)
                        ? originalToken
                        : new AuthToken { Token = tokenString, OtherToken = originalToken.Token };
                    context.Features.Set(new JwtPrincipalFeature(principal, validatedAccessToken));
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                log.LogInformation("SecurityTokenExpiredException:" + ex.Message);

                throw;
            }
            catch (SecurityTokenValidationException ex)
            {
                log.LogInformation("SecurityTokenValidationException:" + ex.Message);

                throw;
            }
            catch (ArgumentException ex)
            {
                log.LogInformation("ArgumentException " + ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Token validation failed: {ex.Message}");
            }
        }

        private static async Task<TokenValidationParameters> CreateTokenValidationParameters(Tenant tenant, string clientId)
        {
            if (tenant.Issuer == ConfigReader.Get(Common.Constants.Environment.IdentityIssuer))
            {
                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigReader.Get(Common.Constants.Environment.IdentitySignKey))),
                    ValidIssuer = tenant.Issuer,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = ConfigReader.Get(Common.Constants.Environment.EAPIGraphAppID),
                    ClockSkew = TimeSpan.Zero
                };
            }
            else
            {
                var configurationManager = await GetConfigurationManager(tenant.OpenIdConnectConfiguration);
                var openIdConnectConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None);

                return new TokenValidationParameters
                {
                    ValidAudience = clientId,
                    ValidIssuer = openIdConnectConfiguration.Issuer,
                    IssuerSigningKeys = openIdConnectConfiguration.SigningKeys,
                };
            }
        }

        private static bool TryGetTokenFromHeaders(FunctionContext context, out AuthToken token)
        {
            token = null;

            var value = context.GetHeader("authorization");

            if (value.IsNullOrWhiteSpace()) return false;

            var headerParts = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim())
                                   .ToList();

            var tokenValue = headerParts.First().Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

            token = new()
            {
                Token = tokenValue,
                OtherToken = headerParts.Count > 1 ? headerParts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : null
            };

            return true;
        }

        private static Tenant CheckValidIssuer(string token, out string clientId)
        {
            var issuer = jwtSecurityTokenHandler.ReadToken(token).Issuer;
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            clientId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Aud)?.Value;

            return tenants.FirstOrDefault(x => x.Issuer.Is(issuer)) ?? new Tenant();
        }
    }
}
