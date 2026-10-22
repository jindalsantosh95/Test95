using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class UpdateB2CServiceCommandHandler : IRequestHandler<UpdateB2CServiceCommand, bool>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UpdateB2CServiceCommandHandler> _logger;

        public UpdateB2CServiceCommandHandler(IHttpClientFactory http, ILogger<UpdateB2CServiceCommandHandler> logger)
        {
            _httpClientFactory = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateB2CServiceCommand request, CancellationToken cancellationToken)
        {
            var token = await TokenHelper.GetAzureB2CToken(_httpClientFactory);

            string graphApiUrl = $"{ConfigReader.Get(Constants.Environment.GraphAPIUrl)}{ConfigReader.Get(Constants.Environment.GraphApiUrlProxyAddressVersion)}{Constants.Application.Users}{request.EmailB2CUpdateData.Value.FirstOrDefault().Id}";

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Patch, graphApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            httpRequest.Content = new StringContent(Json.Serialize(request.EmailB2CUpdateData.Value.First()), Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseMessage = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                _logger.LogInformation($"update in b2c failed: {responseMessage}");

            return httpResponse.IsSuccessStatusCode;
        }
    }
}
