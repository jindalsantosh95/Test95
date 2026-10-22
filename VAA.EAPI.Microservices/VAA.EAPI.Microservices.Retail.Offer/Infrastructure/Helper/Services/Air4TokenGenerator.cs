using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
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

        public async Task<string> GetToken(string clientId, string clientSecret, bool switchToAws)
        {
            if (cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId]))) return cache[clientId].Access_token;

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId]))) return cache[clientId].Access_token;

                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, clientId },
                    { Constants.Application.ClientSecret, clientSecret },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials },
                    { Constants.Application.Scope, Constants.Application.Read }
                };
                var authUri = switchToAws ? ConfigReader.Get(Constants.Environment.DlAwsOfferAuthenticationUrl) : ConfigReader.Get(Constants.Environment.DlOfferAuthenticationUrl);

                var httpResponseMessage = await _apiConnector.PostAsync(authUri, new FormUrlEncodedContent(tokenRequest));

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var content = await httpResponseMessage.Content.ReadAsStringAsync();
                    var tokenResponse = switchToAws ? JsonConvert.DeserializeObject<TokenResponse>(content) : Xml.Deserialize<TokenResponse>(content);

                    cache.AddOrUpdate(clientId, tokenResponse, (k, v) => tokenResponse);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId])) ? cache[clientId].Access_token : string.Empty;

        }

        private static bool IsAir4TokenValid(TokenResponse source) => DateTime.UtcNow < DateTimeOffset.FromUnixTimeMilliseconds(source.Issued_at.Parse<long>()).AddSeconds(source.Expires_in.Parse<double>());
    }
}
