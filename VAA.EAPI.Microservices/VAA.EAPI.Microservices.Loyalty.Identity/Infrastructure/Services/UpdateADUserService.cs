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
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class UpdateAdUserService : IUpdateAdUserService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<UpdateAdUserService> _logger;

        public UpdateAdUserService(IHttpConnector httpConnector, ILogger<UpdateAdUserService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> UpdateAdUser(string api, string b2CServiceQuery, string content, HttpMethod httpMethod , CancellationToken cancellationToken)
        {
            var token = await TokenHelper.GetAzureB2CToken(_httpConnector);

            string graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlVersion)}{api}";
                        
            if (!string.IsNullOrEmpty(b2CServiceQuery))            
                graphApiUrl += b2CServiceQuery;            

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, graphApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(content, Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);

            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);
            string responseMessage = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                var aadErrorResponse = Json.Deserialize<B2CError>(responseMessage);

                _logger.LogError("Microsoft Graph user update failed. StatusCode: {StatusCode}", (int)httpResponse.StatusCode);

                return new()
                {
                    B2CFaliureMessage = Constants.Application.B2CErrorMessage,
                    IsB2COperationSuccess = false
                };
            }

            B2CUserResponse userResponse = null;

            if (httpResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var httpContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                userResponse = Json.Deserialize<B2CUserResponse>(httpContent);
            }
            else
            {
                userResponse = new B2CUserResponse();
            }

            userResponse.IsB2COperationSuccess = true;
            return userResponse;
        }

        public async Task<bool> UpdateMFADetailsFromGraphApi(string api, string content, CancellationToken cancellationToken)
        {

            var token = await TokenHelper.GetAzureB2CToken(_httpConnector);

            string graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlVersion)}{api}";

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Patch, graphApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);

            if (httpResponse.IsSuccessStatusCode)
                return true;

            return false;
        }
    }
}
