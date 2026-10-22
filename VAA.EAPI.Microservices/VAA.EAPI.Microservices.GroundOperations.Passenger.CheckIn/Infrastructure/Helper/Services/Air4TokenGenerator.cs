using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Services
{
    public sealed class Air4TokenGenerator(IHttpConnector apiConnector) : IAir4TokenGenerator
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, TokenResponse> cache = new();

        private readonly IHttpConnector _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));

        public async Task<string> GetToken(string clientId, string clientSecret)
        {
            if (cache.ContainsKey(clientId) && cache[clientId].Access_token.IsJwtTokenValid()) return cache[clientId].Access_token;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(clientId) && cache[clientId].Access_token.IsJwtTokenValid()) return cache[clientId].Access_token;

                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "client_credentials" },
                    { "scope", "read" }
                };
                var authUri = ConfigReader.Get(Constants.Environment.DeltaAir4TokenEndpoint);

                var httpResponseMessage = await _apiConnector.PostAsync(authUri, new FormUrlEncodedContent(tokenRequest));

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var content = await httpResponseMessage.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

                    cache.AddOrUpdate(clientId, tokenResponse, (k, v) => tokenResponse);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return (cache.ContainsKey(clientId) && cache[clientId].Access_token.IsJwtTokenValid()) ? cache[clientId].Access_token : string.Empty;
        }
    }
}
