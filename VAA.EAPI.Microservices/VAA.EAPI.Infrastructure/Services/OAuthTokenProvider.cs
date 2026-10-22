using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Infrastructure.Services
{
    public sealed class OAuthTokenProvider(ILogger<OAuthTokenProvider> logger) : IOAuthTokenProvider
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, string> cache = new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<OAuthTokenProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<string> AcquireTokenAsync(string scope)
        {
            var clientId = ConfigReader.Get(Common.Constants.Environment.EapiMsClientID);
            var clientSecret = ConfigReader.Get(Common.Constants.Environment.EapiMSSecret);
            var key = $"{clientId}/{scope}";

            if (cache.TryGetValue(key, out string value) && value.IsJwtTokenValid()) return value;

            if (string.IsNullOrEmpty(AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()?.Principal.Identity.Name))
                return await AcquireApplicationTokenAsync(clientId, clientSecret, scope);
            else
                return await AcquireDelegateTokenAsync(clientId, clientSecret, scope);
        }

        private async Task<string> AcquireApplicationTokenAsync(string clientId, string clientSecret, string scope)
        {
            var key = $"{clientId}/{scope}";

            if (cache.TryGetValue(key, out string value) && value.IsJwtTokenValid()) return value;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.TryGetValue(key, out value) && value.IsJwtTokenValid()) return value;

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                                                 .WithClientSecret(clientSecret)
                                                                 .WithAuthority(authority, false)
                                                                 .Build();

                var result = await client.AcquireTokenForClient([$"{scope}/.default"]).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.TryGetValue(key, out value) && value.IsJwtTokenValid() ? value : string.Empty;
        }

        private async Task<string> AcquireDelegateTokenAsync(string clientId, string clientSecret, string scope)
        {
            var key = $"{clientId}/{scope}";

            if (cache.TryGetValue(key, out string value) && value.IsJwtTokenValid()) return value;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.TryGetValue(key, out value) && value.IsJwtTokenValid()) return value;

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var principalFeature = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>();
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                                                 .WithClientSecret(clientSecret)
                                                                 .WithAuthority(authority, false)
                                                                 .Build();

                var result = await client.AcquireTokenOnBehalfOf([$"{scope}/.default"], new UserAssertion(principalFeature.AccessToken)).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.TryGetValue(key, out value) && value.IsJwtTokenValid() ? value : string.Empty;
        }
    }
}
