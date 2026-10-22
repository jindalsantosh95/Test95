using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription
{
    public class GetAPIMSubscriptionKeysServiceQueryHandler : IRequestHandler<GetAPIMSubscriptionKeysServiceQuery, KeyValuePair<HttpStatusCode, string>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<GetAPIMSubscriptionKeysServiceQueryHandler> _logger;

        public GetAPIMSubscriptionKeysServiceQueryHandler(IHttpConnector apiConnector, ILogger<GetAPIMSubscriptionKeysServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<KeyValuePair<HttpStatusCode, string>> Handle(GetAPIMSubscriptionKeysServiceQuery subscriptionRequest, CancellationToken cancellationToken)
        {
            string clientId = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientId);
            string clientSecret = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureAdInstance) + "/" + ConfigReader.Get(Constants.Environment.AzureAdTenantId);
            string resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource);
            string url = subscriptionRequest.Uri;

            var authToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
            var headers = new Dictionary<string, string> { { "Accept", Constants.Application.ContenTypeApplicationJson } };
            var apiResponse = await _apiConnector.SendAsync(url, headers, authToken, subscriptionRequest.Verb, string.Empty);
            string responseMessage = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!apiResponse.IsSuccessStatusCode)
            {
                return new KeyValuePair<HttpStatusCode, string>(apiResponse.StatusCode, responseMessage);
            }
            return new KeyValuePair<HttpStatusCode, string>(apiResponse.StatusCode, responseMessage);
        }
    }
}
