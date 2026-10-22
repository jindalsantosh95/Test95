using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS
{
    public class Air4OrderSearchMSQueryHandler : IRequestHandler<Air4OrderSearchMSQuery, Air4OrderSearchMSResponse>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<Air4OrderSearchMSQueryHandler> _logger;

        public Air4OrderSearchMSQueryHandler(IHttpConnector httpConnector, ILogger<Air4OrderSearchMSQueryHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Air4OrderSearchMSResponse> Handle(Air4OrderSearchMSQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.Air4OrderSearchMSSubscriptionKey)},
                { "Channelid", Constants.Application.NDC }
            };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.Air4OrderSearchMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.OrderSearchRequest));
                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return new Air4OrderSearchMSResponse() { OrderSearchResponse = Json.Deserialize<OrderSearchResponse>(response) };
                else
                    return new Air4OrderSearchMSResponse() { ErrorDetails = Json.Deserialize<List<ErrorDetails>>(response) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in OrderSearchMS call - {ex.Message}");
                return null;
            }
        }
    }
}
