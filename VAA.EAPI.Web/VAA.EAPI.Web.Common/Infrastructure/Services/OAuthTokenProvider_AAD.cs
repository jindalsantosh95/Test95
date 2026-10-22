using Microsoft.Identity.Client;
using System.Collections.Concurrent;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Interfaces;

namespace VAA.EAPI.Web.Common.Infrastructure.Services
{
    public sealed class OAuthTokenProvider_AAD : IOAuthTokenProvider
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
            var clientId = ConfigReader.Get(Common.Constants.Environment.AppId);
            var clientSecret = ConfigReader.Get(Common.Constants.Environment.ClientSecret);
            var key = $"{clientId}/{resource}";

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            if (string.IsNullOrEmpty(AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()?.Principal.Identity.Name))
                return await AcquireApplicationTokenAsync(clientId, clientSecret, resource);
            else
                return await AcquireDelegateTokenAsync(clientId, clientSecret, resource);
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
            var key = $"{clientId}/{resource}";

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                        .WithClientSecret(clientSecret)
                                        .WithAuthority(authority, false)
                                        .Build();

                var result = await client.AcquireTokenForClient(new string[] { resource + "/.default" }).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(key) && cache[key].IsJwtTokenValid() ? cache[key] : string.Empty;
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

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

                var authority = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
                var principalFeature = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>();
                var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithClientSecret(clientSecret)
                                                    .WithAuthority(authority, false)
                                                    .Build();

                var result = await client.AcquireTokenOnBehalfOf(new string[] { resource + "/.default" }, new UserAssertion(principalFeature.AccessToken)).ExecuteAsync();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(key) && cache[key].IsJwtTokenValid() ? cache[key] : string.Empty;
        }
    }
}
