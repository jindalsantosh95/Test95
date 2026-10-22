using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class DeleteB2CAccountService : IDeleteB2CAccountService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<DeleteB2CAccountService> _logger;

        public DeleteB2CAccountService(IHttpConnector httpConnector, ILogger<DeleteB2CAccountService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> DeleteB2CAccount(string customerId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("customerId is required", nameof(customerId));

            try
            {

                var token = await TokenHelper.GetAzureB2CToken(_httpConnector);

                var graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlVersion)}/users/{customerId}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, graphApiUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new B2CUserResponse
                    {
                        IsB2COperationSuccess = true
                    };
                }

                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Microsoft Graph account deletion failed. StatusCode: {StatusCode}.",
                        (int)httpResponse.StatusCode, responseMessage);

                    return new B2CUserResponse
                    {
                        B2CFaliureMessage = "Error Deleting Account",
                        IsB2COperationSuccess = false
                    };
                }

                return new B2CUserResponse
                {
                    IsB2COperationSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refreshing token: {ex.Message}");
                return new B2CUserResponse
                {
                    B2CFaliureMessage = "Error Deleting Account",
                    IsB2COperationSuccess = false
                };
            }
        }
    }
}