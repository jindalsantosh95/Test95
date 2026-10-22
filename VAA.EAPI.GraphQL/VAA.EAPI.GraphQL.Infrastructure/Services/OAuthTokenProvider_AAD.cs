using Microsoft.Identity.Client;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Infrastructure.Services
{
    public sealed class OAuthTokenProvider_AAD : IOAuthTokenProvider, IApplicationOAuthTokenProvider
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, string> cache = new();

        public OAuthTokenProvider_AAD() { }

        /// <summary>
        /// Acquire a token for the requested resource. Depending on type of token used in the the Graph QL request (App or Delegate)
        /// a corresponding token will generated.
        /// </summary>
        /// <param name="resource">Resource ID being consumed</param>
        /// <returns>OAuth token</returns>
        public async Task<string> AcquireTokenAsync(string resource)
        {
            var clientId = ConfigReader.Get(Common.Constants.Environment.EAPIGraphAppID);
            var clientSecret = ConfigReader.Get(Common.Constants.Environment.EAPIGraphSecret);
            var principalName = AsyncContext.Current?.Context?.Features.Get<JwtPrincipalFeature>()?.Principal?.Identity?.Name;
            if (string.IsNullOrEmpty(principalName))
                return await AcquireApplicationTokenAsync(clientId, clientSecret, resource);
            else
                return await AcquireDelegateTokenAsync(clientId, clientSecret, resource);
        }

        public async Task<string> AcquireApplicationTokenAsync(string resource)
        {
            foreach (var cacheEntry in cache)
                if (!cacheEntry.Value.IsJwtTokenValid()) cache.TryRemove(cacheEntry.Key, out _);

            var clientId = ConfigReader.Get(Common.Constants.Environment.EAPIGraphAppID);
            var key = $"{clientId}/{resource}/application";
            if (cache.TryGetValue(key, out var cached) && cached.IsJwtTokenValid()) return cached;

            var token = await AcquireApplicationTokenAsync(
                clientId,
                ConfigReader.Get(Common.Constants.Environment.EAPIGraphSecret),
                resource);
            cache[key] = token;
            return token;
        }

        /// <summary>
        /// Acquire an APPLICATION token for the requested resource.
        /// </summary>
        /// <param name="clientId">Graph QL client ID</param>
        /// <param name="clientSecret">Graph QL client secret</param>
        /// <param name="resource">Resource ID being consumed</param>
        /// <returns>OAuth token</returns>
        private async Task<string> AcquireApplicationTokenAsync(string clientId, string clientSecret, string resource)
        {
            var key = $"{clientId}/{resource}/application";

            if (cache.TryGetValue(key, out var cached) && cached.IsJwtTokenValid()) return cached;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.TryGetValue(key, out cached) && cached.IsJwtTokenValid()) return cached;

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                        .WithClientSecret(clientSecret)
                                        .WithAuthority(authority, false)
                                        .Build();

                var result = await client.AcquireTokenForClient(new string[] { $"{resource}/.default" }).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.TryGetValue(key, out cached) && cached.IsJwtTokenValid() ? cached : string.Empty;
        }

        /// <summary>
        /// Acquire a delegated token for the requested resource on behalf of the user making the request to Graph QL 
        /// </summary>
        /// <param name="clientId">Graph QL client ID</param>
        /// <param name="clientSecret">Graph QL client secret</param>
        /// <param name="resource">Resource ID being consumed</param>
        /// <returns>OAuth token</returns>
        private async Task<string> AcquireDelegateTokenAsync(string clientId, string clientSecret, string resource)
        {
            var key = $"{clientId}/{resource}";

            if (cache.TryGetValue(key, out var cached) && cached.IsJwtTokenValid()) return cached;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.TryGetValue(key, out cached) && cached.IsJwtTokenValid()) return cached;

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var principalFeature = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>();
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithClientSecret(clientSecret)
                                                    .WithAuthority(authority, false)
                                                    .Build();

                var result = await client.AcquireTokenOnBehalfOf(new string[] { $"{resource}/.default" }, new UserAssertion(principalFeature.AccessToken.Token)).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.TryGetValue(key, out cached) && cached.IsJwtTokenValid() ? cached : string.Empty;
        }
    }
}
