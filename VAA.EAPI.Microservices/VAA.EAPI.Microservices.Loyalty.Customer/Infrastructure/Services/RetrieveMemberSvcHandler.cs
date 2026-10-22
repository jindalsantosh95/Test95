using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services
{
    public class RetrieveMemberSvcHandler : IRequestHandler<RetrieveMemberQuery, KeyValuePair<bool, string>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RetrieveMemberSvcHandler> _logger;

        public RetrieveMemberSvcHandler(IHttpClientFactory httpClientFactory, ILogger<RetrieveMemberSvcHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<KeyValuePair<bool, string>> Handle(RetrieveMemberQuery retrieveMemberQuery, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Customer Profile Service handler");
            string clientId = ConfigReader.Get(Constants.Environment.RetrieveMemberApiClientId);
            string clientSecret = ConfigReader.Get(Constants.Environment.RetrieveMemberApiClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.RetrieveMemberApiAuthority);
            string resource = ConfigReader.Get(Constants.Environment.RetrieveMemberApiResource);
            string ocpApimSubscriptionKey = ConfigReader.Get(Constants.Environment.RetrieveMemberOcpKey);
            string url = ConfigReader.Get(Constants.Environment.RetrieveMemberDetailsEndpoint);

            string bearerToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
            _logger.LogInformation("Customer Profile Service handler || Token generated");
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new(HttpMethod.Get, $"{url}{retrieveMemberQuery?.CustomerId}");
            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(request, cancellationToken);
            string responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"Customer Profile Service handler || SCVPlus Member Profile API called {url}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Customer Profile Service handler || RetrieveMember Details API Error Response");
                return new KeyValuePair<bool, string>(false, responseMessage);

            }
            _logger.LogInformation("Customer Profile Service handler || RetrieveMember Details API Success Response");
            return new KeyValuePair<bool, string>(true, responseMessage);
        }
    }
}
