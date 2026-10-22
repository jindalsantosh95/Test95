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
    public class DeleteAPIMSubscriptionServiceCommandHandler : IRequestHandler<DeleteAPIMSubscriptionServiceCommand, KeyValuePair<HttpStatusCode, string>>
    {
        private readonly ILogger<DeleteAPIMSubscriptionServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;

        public DeleteAPIMSubscriptionServiceCommandHandler(ILogger<DeleteAPIMSubscriptionServiceCommandHandler> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<KeyValuePair<HttpStatusCode, string>> Handle(DeleteAPIMSubscriptionServiceCommand subscriptionDeleteRequest, CancellationToken cancellationToken)
        {

            string clientId = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientId);
            string clientSecret = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureAdInstance) + "/" + ConfigReader.Get(Constants.Environment.AzureAdTenantId);
            string resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource);



            var authToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
            var headers = new Dictionary<string, string> { { "Accept", Constants.Application.ContenTypeApplicationJson } };
            var apiResponse = await _apiConnector.SendAsync(subscriptionDeleteRequest.Uri, headers, authToken, subscriptionDeleteRequest.Verb, string.Empty);
            string responseMessage = await apiResponse.Content.ReadAsStringAsync(cancellationToken);


            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("APIM subscription deletion failed. StatusCode: {StatusCode}", apiResponse.StatusCode);
                return new KeyValuePair<HttpStatusCode, string>(apiResponse.StatusCode, responseMessage);

            }


            return new KeyValuePair<HttpStatusCode, string>(apiResponse.StatusCode, responseMessage);
        }
    }
}
