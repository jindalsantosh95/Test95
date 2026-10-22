using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;


namespace VAA.EAPI.Infrastructure.Helper
{
    public sealed class TokenHelper : ITokenHelper
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, string> cache = new();

        private readonly IHttpConnector _httpConnector;

        public TokenHelper(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<string> GetToken(string key, string uri, HttpContent content)
        {
            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            await _semaphoreSlim.WaitAsync();

            if (cache.ContainsKey(key) && cache[key].IsJwtTokenValid()) return cache[key];

            try
            {
                var httpResponse = (await _httpConnector.PostAsync(uri, content)).EnsureSuccessStatusCode();

                var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);

                var result = Json.Deserialize<Domain.Models.Token>(responseMessage).AccessToken;

                cache.AddOrUpdate(key, result, (k, v) => result);
            }
            catch { }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(key) && cache[key].IsJwtTokenValid() ? cache[key] : string.Empty;
        }
    }
}
