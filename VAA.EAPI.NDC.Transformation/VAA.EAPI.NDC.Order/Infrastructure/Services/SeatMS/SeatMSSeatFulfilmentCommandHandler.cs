using MediatR;
using System.Collections.Generic;
using System.Net;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using System.Net.Http;
using VAA.EAPI.Domain.Entities.Common;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSSeatFulfillmentCommandHandler(IHttpConnector httpConnector, ILogger<SeatMSSeatFulfillmentCommandHandler> logger) : IRequestHandler<SeatMSSeatFulfilmentCommand, SeatFulfilmentResponse>
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<SeatMSSeatFulfillmentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<SeatFulfilmentResponse> Handle(SeatMSSeatFulfilmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)},
                { "ChannelId","NDC" }
            };
                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.SeatMSSeatFulfilmentUrl), headers, string.Empty, HttpMethod.Patch, Json.Serialize(request.SeatFulfilmentRequest));
                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK) 
                { 
                        return Json.Deserialize<SeatFulfilmentResponse>(response);
                 }
                else
                {
                    return new SeatFulfilmentResponse() { Error = Json.Deserialize<List<ErrorDetails>>(response) };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex , $"Failure in SeatFulfillment call - {ex.Message}");
                return null;
            }
        }
    }
}
