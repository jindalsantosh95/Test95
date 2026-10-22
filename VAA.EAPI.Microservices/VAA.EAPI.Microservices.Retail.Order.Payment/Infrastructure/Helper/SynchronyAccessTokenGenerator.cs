using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper
{
    public sealed class SynchronyAccessTokenGenerator : ISynchronyAccessTokenGenerator
    {
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, TokenResponse> cache = new();
        string clientId = ConfigReader.Try(Constants.Environment.SynchronyApiClientId);
        string clientSecret = ConfigReader.Try(Constants.Environment.SynchronyApiClientSecret);
        private readonly IHttpConnector _apiConnector;

        public SynchronyAccessTokenGenerator(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<string> GetToken()
        {
            if (cache.ContainsKey(clientId) && IsSynchronyTokenValid(cache[clientId])) return cache[clientId].Access_token;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(clientId) && IsSynchronyTokenValid(cache[clientId])) return cache[clientId].Access_token;

                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, clientId },
                    { Constants.Application.ClientSecret, clientSecret },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

                var httpResponseMessage = await _apiConnector.PostAsync(ConfigReader.Get(Constants.Environment.SynchronyAuthenticationUrl), new FormUrlEncodedContent(tokenRequest));

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var content = await httpResponseMessage.Content.ReadAsStringAsync();
                    var tokenResponse = Json.Deserialize<TokenResponse>(content);

                    cache.AddOrUpdate(clientId, tokenResponse, (k, v) => tokenResponse);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(clientId) && IsSynchronyTokenValid(cache[clientId]) ? cache[clientId].Access_token : string.Empty;
        }

        private static bool IsSynchronyTokenValid(TokenResponse source) => DateTime.UtcNow < DateTimeOffset.FromUnixTimeMilliseconds(source.Issued_at.Parse<long>()).AddSeconds(source.Expires_in.Parse<double>());
    }
}
