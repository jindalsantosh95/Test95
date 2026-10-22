using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class GbgService : IGbgService
    {
        private readonly ILogger<GbgService> _logger;
        private readonly IHttpConnector _httpConnector;
        private static readonly IEnumerable<GBGConfig> _gbgConfig = Json.Deserialize<IEnumerable<GBGConfig>>(ConfigReader.Get(Constants.Environment.GbgServiceKeyMap));


        public GbgService(IHttpConnector httpConnector, ILogger<GbgService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<GbgResponse<FindResponse>> Find(string addressText, string countryCode, CancellationToken cancellationToken)
        {
            try
            {
                string url = string.Empty;
                string endpoint = ConfigReader.Get(Constants.Environment.GbgFindEndpoint);
                string serviceKey = GetGbgServiceKey();

                url = endpoint
                    .Replace("{keyValue}", serviceKey)
                    .Replace("{textValue}", addressText);

                if (!string.IsNullOrEmpty(countryCode))
                    url = $"{url}&Countries={countryCode}";

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);
                string responseMessage = await httpResponse.Content.ReadAsStringAsync();

                return Json.Deserialize<GbgResponse<FindResponse>>(responseMessage);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling Gbg API: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error occurred Gbg API: {ex.Message}");
                return null;

            }
        }

        public async Task<GbgResponse<RetrieveResponse>> Retrieve(string addressId, CancellationToken cancellationToken)
        {
            try
            {
                string url = ConfigReader.Get(Constants.Environment.GbgRetrieveEndpoint).Replace("{keyValue}", GetGbgServiceKey()).Replace("{IdValue}", addressId);

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                using HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);
                string responseMessage = await httpResponse.Content.ReadAsStringAsync();

                return Json.Deserialize<GbgResponse<RetrieveResponse>>(responseMessage);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling GBG API for Id={addressId}");
                throw;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error for Id={addressId}");
                throw;

            }
        }

        /// <summary>
        /// Gets the GBG service key based on the consumer's client_id from the authorization header. 
        /// It checks the GBG config for a matching client_id and returns the corresponding key. If no match is found, it falls back to the default key.
        /// </summary>
        /// <returns></returns>
        private string GetGbgServiceKey()
        {
            try
            {
                // Get the authorization header from the current context
                var authHeader = AsyncContext.Current.Context.GetHeader("authorization");

                if (!authHeader.IsNullOrWhiteSpace())
                {
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader["Bearer ".Length..].Trim();
                        var consumer = token.ExtractJwtClaim("appid") ??
                                       token.ExtractJwtClaim("client_id") ??
                                       token.ExtractJwtClaim("azp");


                        if (consumer != null)
                        {
                            // Check if the consumer is in the GBG config and return the corresponding key
                            var config = _gbgConfig.FirstOrDefault(c => c.ClientId.Equals(consumer, StringComparison.InvariantCultureIgnoreCase));
                            if (config != null)
                            {
                                return config.GbgKey;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing JWT token from authorization header to get GBG service key. Falling back to default key.");
                // If no auth, just fall back to default key, but log the exception for any unexpected errors during token parsing
            }
            // Fall back to the default key if no matching client_id is found or not auth header found
            return _gbgConfig.First(c => c.ClientId.IsNullOrWhiteSpace()).GbgKey;
        }
    }
}
