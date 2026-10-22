using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS
{
    public class FlightServicesDocQueryHandler : IRequestHandler<FlightServiceOfferDocQuery, FlightOfferServiceRS>
    {
        private readonly IHttpConnector _httpConnector;

        public FlightServicesDocQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<FlightOfferServiceRS> Handle(FlightServiceOfferDocQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.FlightServicesMSSubscriptionKey)},
                {"Channelid",Constants.Application.NDC }
            };

            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.FlightServicesMSUrl)}{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return  new FlightOfferServiceRS() { ServiceListResponse = Json.Deserialize<FlightServiceResponse>(response) };
            else
                return new FlightOfferServiceRS() { Error = Json.Deserialize<IEnumerable<ErrorDetails>>(response) }; ;

        }
    }
}

