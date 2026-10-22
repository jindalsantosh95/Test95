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
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class RegisterServiceCommandHandler : IRequestHandler<RegisterServiceCommand, B2CUserResponse>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<RegisterServiceCommandHandler> _logger;

        public RegisterServiceCommandHandler(IHttpConnector httpConnector, ILogger<RegisterServiceCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> Handle(RegisterServiceCommand registerToAzureB2CServiceCommand, CancellationToken cancellationToken)
        {

            var b2cAccessToken = await TokenHelper.GetAzureB2CToken(_httpConnector);

            string url = $"{ConfigReader.Get(Environment.GraphAPIUrl)}{ConfigReader.Get(Environment.GraphApiUrlVersion)}{registerToAzureB2CServiceCommand.Api}";

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);

            req.Headers.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthenticationBearer, b2cAccessToken);
            req.Content = new StringContent(registerToAzureB2CServiceCommand.Content, Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);

            var httpResponse = await _httpConnector.SendAsync(req, cancellationToken);
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var aadErrorResponse = Json.Deserialize<B2CError>(responseMessage);
                _logger.LogError("AzureB2C API enrolment failed. StatusCode: {StatusCode}, ErrorSummary: {ErrorSummary}", httpResponse.StatusCode, aadErrorResponse.Error.Message);

                return new()
                {
                    B2CFaliureMessage = Constants.Application.B2CErrorMessage,
                    IsB2COperationSuccess = false
                };
            }

            var userResponse = Json.Deserialize<B2CUserResponse>(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
            userResponse.IsB2COperationSuccess = true;
            return userResponse;
        }
    }
}
