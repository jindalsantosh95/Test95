using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper
{
    public class OfferSearchItems : IOfferSearchItems
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<OfferSearchItems> _logger;
        public OfferSearchItems(IHttpConnector apiConnector, ILogger<OfferSearchItems> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> GetOfferSearchItemsById(string OfferRefId, string channelId)
        {
            try
            {
                _logger.LogInformation("Offer Search API call started");
                string clientId = ConfigReader.Get(Constants.Environment.OfferSearchClientId);
                string clientSecret = ConfigReader.Get(Constants.Environment.OfferSearchClientSecret);
                string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
                string resource = ConfigReader.Get(Constants.Environment.OfferSearchResouceId);
                string url = ConfigReader.Get(Constants.Environment.OfferSearchEndPoint) + OfferRefId;
                string token = await ExternalHelper.GetBearerToken(clientId, clientSecret, authority, resource);
                Dictionary<string, string> headers = new()
                {
                    { Constants.Environment.OcpKeyHeader, ConfigReader.Get(Constants.Environment.OfferSearchOCPKey) },
                    { Constants.Environment.ChannelId, channelId }
                };
                var httpResponse = await _apiConnector.GetAsync<string>(uri: url, headers: headers, bearerToken: token);
                _logger.LogInformation("Offer Search API call Ended");
                return httpResponse != null && httpResponse.Contains(OfferRefId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Offer Search API call Failed due to invalid Offer Id || Message : {exception.Message}");
                return false;
            }

        }
    }
}
