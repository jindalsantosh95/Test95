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
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4TokenGenerator
{
    public sealed class Air4TokenGenerator : IAir4TokenGenerator
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, TokenResponse> cache = new();

        private readonly IHttpConnector _apiConnector;

        public Air4TokenGenerator(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<string> GetToken(string clientId, string clientSecret)
        {
            if (cache.ContainsKey(clientId) && IsAir4TokenValid(cache[clientId])) return cache[clientId].Access_token;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(clientId) && IsAir4TokenValid(cache[clientId])) return cache[clientId].Access_token;

                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, clientId },
                    { Constants.Application.ClientSecret, clientSecret },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

                var httpResponseMessage = await _apiConnector.PostAsync(ConfigReader.Get(Constants.Environment.DlPaymentAuthenticationUrl), new FormUrlEncodedContent(tokenRequest));

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var content = await httpResponseMessage.Content.ReadAsStringAsync();
                    var tokenResponse = Xml.Deserialize<TokenResponse>(content);

                    cache.AddOrUpdate(clientId, tokenResponse, (k, v) => tokenResponse);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(clientId) && IsAir4TokenValid(cache[clientId]) ? cache[clientId].Access_token : string.Empty;
        }

        private static bool IsAir4TokenValid(TokenResponse source) => DateTime.UtcNow < DateTimeOffset.FromUnixTimeMilliseconds(source.Issued_at.Parse<long>()).AddSeconds(source.Expires_in.Parse<double>());
    }
}
