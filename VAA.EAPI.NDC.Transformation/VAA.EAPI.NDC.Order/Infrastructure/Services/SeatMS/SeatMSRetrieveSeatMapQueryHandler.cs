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
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSRetrieveSeatMapQueryHandler(IHttpConnector httpConnector, ILogger<SeatMSRetrieveSeatMapQueryHandler> logger) : IRequestHandler<SeatMSRetrieveSeatMapQuery, SeatRS>
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<SeatMSRetrieveSeatMapQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<SeatRS> Handle(SeatMSRetrieveSeatMapQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = new Dictionary<string, string>()
                {
                    { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)},
                };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.SeatMSRetrieveSeatMapUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
                var seatMSResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return new SeatRS() { SeatAvailabilityResponse = Json.Deserialize<SeatAvailabilityResponse>(seatMSResponse) };
                else
                    return new SeatRS() { Error = Json.Deserialize<IEnumerable<ErrorDetails>>(seatMSResponse) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in RetrieveSeatMap call - {ex.Message}");
                return null;
            }
        }
    }
}
