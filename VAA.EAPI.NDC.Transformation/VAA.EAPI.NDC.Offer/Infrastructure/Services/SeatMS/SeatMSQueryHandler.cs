using MediatR;
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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.SeatMS
{
    public class SeatMSQueryHandler : IRequestHandler<SeatMSQuery, SeatRS>
    {
        private readonly IHttpConnector _httpConnector;

        public SeatMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<SeatRS> Handle(SeatMSQuery request, CancellationToken cancellationToken)
        {
            SeatRS response = new();
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)},
            };

            var seatMSEndpoint = ConfigReader.Get(Constants.Environment.SeatMSUrl);
            using var httpResponseMessage = await _httpConnector.SendAsync(seatMSEndpoint, headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new SeatRS() { SeatAvailabilityResponse = await Json.DeserializeAsync<SeatMSAvailabilityResponse>(responseStream, cancellationToken) };
            else
                return new SeatRS() { Error = await Json.DeserializeAsync<IEnumerable<ErrorDetails>>(responseStream, cancellationToken) };
        }
    }
}