using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders
{
    public class B2CGraphSvcHandler : IRequestHandler<B2CGraphSvcQuery, KeyValuePair<bool, string>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<B2CGraphSvcHandler> _logger;

        public B2CGraphSvcHandler(IHttpClientFactory httpClientFactory, ILogger<B2CGraphSvcHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<KeyValuePair<bool, string>> Handle(B2CGraphSvcQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Graph API Get Loyalty No Service handler");
            string clientId = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.GraphB2CApiClientId);
            string clientSecret = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.GraphB2CApiClientSecret);
            string authority = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.GraphB2CApiAuthority);
            string resource = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.GraphB2CApiResource);
            string url = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.GraphB2CDetailsEndpoint);
            string b2cExtensionAppId = ConfigReader.Get(Constants.Environment.AzureAdB2CExtensionsAppId).Replace("-", "");

            // Converting Request Params to Key Value Pair.  
            Dictionary<string, string> allIputParams = new Dictionary<string, string>();
            allIputParams.Add("api-version", ConfigReader.Get(Constants.Environment.GraphB2CAPIVersion));
            allIputParams.Add("$filter", $"extension_{b2cExtensionAppId}_customerId eq '{request.CustomerId}'");

            var uri = QueryHelpers.AddQueryString(url, allIputParams);

            string bearerToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage req = new(HttpMethod.Get, $"{uri}");
            req.Headers.Add("cache-control", "no-cache");
            req.Headers.Add("Accept", "*/*");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(req, cancellationToken);
            string responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogInformation($"Graph API Error Response {responseMessage}");
                return new KeyValuePair<bool, string>(false, responseMessage);
            }

            else
            {
                _logger.LogInformation("Graph API Success Response");
                return new KeyValuePair<bool, string>(true, responseMessage);
            }
        }
    }
}
