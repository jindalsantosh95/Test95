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
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class GetAdUserDetailsService : IGetAdUserDetailsService
    {
        private readonly IHttpConnector _httpConnector;

        public GetAdUserDetailsService(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<string> GetDetailsFromGraphApi(string api, string verifyB2CServiceQuery, CancellationToken cancellationToken)
        {

            var token = await TokenHelper.GetAzureB2CToken(_httpConnector);

            string graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlVersion)}{api}";
                        
            if (!string.IsNullOrEmpty(verifyB2CServiceQuery))            
                graphApiUrl += $"{"?"}{verifyB2CServiceQuery}";            

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, graphApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);
            string responseMessage = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)            
                return responseMessage;            

            return null;
        }
    }
}