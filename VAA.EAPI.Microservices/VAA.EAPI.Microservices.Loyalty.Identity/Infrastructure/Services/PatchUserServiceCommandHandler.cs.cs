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
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class PatchUserServiceCommandHandler : IRequestHandler<PatchUserServiceCommand, B2CUserResponse>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<RegisterServiceCommandHandler> _logger;

        public PatchUserServiceCommandHandler(IHttpConnector httpConnector,     ILogger<RegisterServiceCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> Handle(PatchUserServiceCommand patchToAzureB2CServiceCommand, CancellationToken cancellationToken)
        {
            var b2cAccessToken = await TokenHelper.GetAzureB2CToken(_httpConnector);
            string url = $"{ConfigReader.Get(Environment.GraphAPIUrl)}{ConfigReader.Get(Environment.GraphApiUrlVersion)}{patchToAzureB2CServiceCommand.Api}{patchToAzureB2CServiceCommand.ObjectId}";
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Patch, url);

            req.Headers.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthenticationBearer, b2cAccessToken);
            req.Content = new StringContent(patchToAzureB2CServiceCommand.Content, Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);

            var httpResponse = await _httpConnector.SendAsync(req, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return new()
                {
                    IsB2COperationSuccess = false,
                    Id = patchToAzureB2CServiceCommand.ObjectId

                };
            }
            return new()
            {
                IsB2COperationSuccess = true,
                Id = patchToAzureB2CServiceCommand.ObjectId

            };
        }
    }
}
