using Aliencube.AdalWrapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class IdentityServiceHandler : IRequestHandler<IdentityServiceCommand, bool>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<IdentityServiceHandler> _logger;

        public IdentityServiceHandler(IHttpConnector httpConnector, ILogger<IdentityServiceHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(IdentityServiceCommand b2CServiceQuery, CancellationToken cancellationToken)
        {
            var b2cAccessToken = await TokenHelper.GetAzureB2CToken(_httpConnector);

            string url = $"{ConfigReader.Get(Environment.GraphAPIUrl)}{ConfigReader.Get(Environment.GraphApiUrlVersion)}{b2CServiceQuery.Api}";

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);

            req.Headers.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthenticationBearer, b2cAccessToken);
            req.Content = new StringContent(b2CServiceQuery.Content, Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);

            var httpResponse = await _httpConnector.SendAsync(req, cancellationToken);
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogError("Azure B2C enrolment failed. StatusCode: {StatusCode}", (int)httpResponse.StatusCode);
                return false;
            }

            return true;
        }
    }
}
