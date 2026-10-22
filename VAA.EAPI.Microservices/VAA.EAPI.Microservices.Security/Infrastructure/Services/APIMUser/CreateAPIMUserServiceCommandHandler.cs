using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMUser
{
    public class CreateAPIMUserServiceCommandHandler : IRequestHandler<CreateAPIMUserServiceCommand, KeyValuePair<bool, string>>
    {
        private readonly ILogger<CreateAPIMUserServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;

        public CreateAPIMUserServiceCommandHandler(ILogger<CreateAPIMUserServiceCommandHandler> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<KeyValuePair<bool, string>> Handle(CreateAPIMUserServiceCommand request, CancellationToken cancellationToken)
        {

            string clientId = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientId);
            string clientSecret = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureAdInstance) + "/" + ConfigReader.Get(Constants.Environment.AzureAdTenantId);
            string resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource);
            string url = request.Uri;
            string bearerToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
            var headers = new Dictionary<string, string> { { "Accept", Constants.Application.ContenTypeApplicationJson } };

            var apiResponse = await _apiConnector.SendAsync(url, headers, bearerToken, HttpMethod.Put, JsonConvert.SerializeObject(request.Request));
            string responseMessage = await apiResponse.Content.ReadAsStringAsync(cancellationToken);



            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("APIM user creation failed. StatusCode: {StatusCode}", apiResponse.StatusCode);
                return new KeyValuePair<bool, string>(false, responseMessage);

            }
            return new KeyValuePair<bool, string>(true, responseMessage);
        }
    }
}
