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
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
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
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)},
            };
            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.SeatMSUrl)+ "/" + $"{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new SeatRS() { SeatAvailabilityResponse = Json.Deserialize<SeatAvailabilityResponse>(response) };
            else
                return new SeatRS() { Error = Json.Deserialize<IEnumerable<ErrorDetails>>(response) };
        }
    }
}



