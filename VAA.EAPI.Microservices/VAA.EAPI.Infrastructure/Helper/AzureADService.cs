using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;


namespace VAA.EAPI.Infrastructure.Helper
{
    public static class AzureADService
    {
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, string> cache = new();

        public static async Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource)
        {
            var key = $"{clientId}/{resource}";

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            try
            {
                await semaphoreSlim.WaitAsync();

                if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

                AuthenticationContext context = new(authority, false);

                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result = await context.AcquireTokenAsync(resource, new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret));

                if (result != null)
                    cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) ? cache[key] : string.Empty;
        }



        public static async Task<string> GetAccessTokenV2(string clientId, string clientSecret, string authority, string scope)
        {
            var key = $"{clientId}/{scope}";

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            try
            {
                await semaphoreSlim.WaitAsync();

                if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

                var app = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                var result = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();

                if (result != null && !string.IsNullOrEmpty(result.AccessToken))
                {
                    cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
                    return result.AccessToken;
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return string.Empty;
        }
    }

}