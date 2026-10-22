using Aliencube.AdalWrapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class UnVerifiedMemberValidationServiceHandler : IRequestHandler<UnVerifiedMemberValidationCommand, B2CGraphResponse>
    {
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UnVerifiedMemberValidationServiceHandler> _logger;

        public UnVerifiedMemberValidationServiceHandler(System.Net.Http.IHttpClientFactory http, ILogger<UnVerifiedMemberValidationServiceHandler> logger)
        {
            _httpClientFactory = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CGraphResponse> Handle(UnVerifiedMemberValidationCommand verifyB2CServiceQuery, CancellationToken cancellationToken)
        {

            var token = await TokenHelper.GetAzureB2CToken(_httpClientFactory);

            string graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlVersion)}{verifyB2CServiceQuery.Api}";

            // Append the access token for the Graph API to the Authorization header of the request, using the Bearer scheme.
            if (!string.IsNullOrEmpty(verifyB2CServiceQuery.Query))
            {
                graphApiUrl += $"{"?"}{verifyB2CServiceQuery.Query}";
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, graphApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
            string responseMessage = await httpResponse.Content.ReadAsStringAsync();
            if (httpResponse.IsSuccessStatusCode)
            {
                return Json.Deserialize<B2CGraphResponse>(responseMessage);

            }

            return null;
        }
    }
}
